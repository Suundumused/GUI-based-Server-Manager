using System.Diagnostics;
using System.Text;

using Server_Manager_Application.Common.Logging.ConsoleUtils;
using Server_Manager_Application.Resources.Languages;


namespace Server_Manager_Application.Runtime.HighLevel
{
    public class CommandRunner : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Process _process;
        private readonly StringBuilder _outputBuffer = new StringBuilder();
        private readonly TaskCompletionSource<bool> _exitTcs = new TaskCompletionSource<bool>();


        public CommandRunner()
        {
            string shell;
            string args;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                shell = "powershell";
                args = "-NoExit -Command -"; // Keep session open
            }
            else
            {
                shell = "/bin/bash";
                args = "--login"; // Keep session open
            }

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = args,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            _process.Exited += (object? s, EventArgs e) => _exitTcs.TrySetResult(true);

            _process.OutputDataReceived += (object s, DataReceivedEventArgs e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _outputBuffer.AppendLine(e.Data);
            };

            _process.ErrorDataReceived += (object s, DataReceivedEventArgs e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _outputBuffer.AppendLine($"{AppResources.Error}: {e.Data}");
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public async Task<(string, string)> ExecuteCommandAsync(string command)
        {
            if (_process.HasExited)
                return ($"{AppResources.Error}: Shell process has exited.", "text-success");

            _outputBuffer.Clear();

            string marker = $"__CMD_END_{Guid.NewGuid()}__";

            await _process.StandardInput.WriteLineAsync($"{command}; echo {marker}");
            await _process.StandardInput.FlushAsync();

            string messageBuffer = _outputBuffer.ToString();
            string messageClass;

            CancellationToken token = _cancellationTokenSource.Token;
            {
                while (!messageBuffer.Contains(marker))
                {
                    await Task.Delay(60, token);

                    messageBuffer = _outputBuffer.ToString();
                }
            }

            messageBuffer = messageBuffer.Replace(marker, "");

            string messageStatement = messageBuffer.ToLower();
            {
                if (messageStatement.Contains("error"))
                {
                    messageClass = "text-danger";
                }
                else if (messageStatement.Contains("warning") || messageStatement.Contains("alert")) 
                {
                    messageClass = "text-warning";
                }
                else 
                {
                    messageClass = "text-success";
                }
            }

            return (messageBuffer.Replace(Environment.NewLine, "</br>"), messageClass);
        }

        public async Task CloseAsync()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    await _process.WaitForExitAsync();

                    Printer.Print("OK");
                }
            }
            catch (Exception ex)
            {
                Printer.Print($"{AppResources.Error} killing process: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource.Cancel();
                _process.Dispose();
            }
        }

        public void Dispose() => _process?.Dispose();
    }
}
