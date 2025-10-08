using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Text.Json;

using Server_Manager_Application.Models.Messaging;
using Server_Manager_Application.Models.Options;
using Server_Manager_Application.Resources.Languages;
using Server_Manager_Application.Runtime.HighLevel;
using Server_Manager_Application.Models.Nativization;


namespace Server_Manager_Application.Controllers
{
    public class ToolsController : BaseController
    {
        PathReadWrite _pathReadWrite;


        public ToolsController(IOptions<BasicOptions> basicOptions, PathReadWrite pathReadWrite) : base(basicOptions)
        {
            _pathReadWrite = pathReadWrite;
        }

        [HttpGet]
        public IActionResult Console()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Console([FromBody] JsonElement jsonData)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { response = AppResources.ParserError, state = false });
            }

            if (jsonData.TryGetProperty("mString", out JsonElement commandElement))
            {
                string stringCommand = commandElement.ToString();

                switch (stringCommand.ToLower())
                {
                    case "freset":
                        await Program.commandRunner.CloseAsync();
                        Program.commandRunner = new CommandRunner();

                        return Json(new { response = AppResources.TerminalReboot, state = false });

                    case "fshutdown":
                        await Program.commandRunner.CloseAsync();
                        Environment.Exit(0);

                        break;
                }

                (string, string) cmdResult = await Program.commandRunner.ExecuteCommandAsync(stringCommand);

                return Json(new { response = cmdResult.Item1, state = cmdResult.Item2 });
            }

            return Json(new { response = AppResources.MissingField, state = false });
        }

        [HttpGet]
        [Route("Tools/Path/{*path}")]
        public async Task<IActionResult> Path(string? path)
        {
            if (System.IO.File.Exists(_pathReadWrite.FullPath(path)))
            {
                return Redirect("/Tools/Download/" + path);
            }

            (List<FileData>, string, string?, bool) pathResult = await _pathReadWrite.AccessDirectoryAsync(path);

            string currentPath = pathResult.Item2;
            string? errorMessage = pathResult.Item3;

            if (pathResult.Item4)
            {
                TempData["Error"] = errorMessage;

                return Redirect("/Tools/Path" + _pathReadWrite.MainPath(currentPath));
            }

            return View(new PathResult
                {
                    files = pathResult.Item1,
                    currentPath = currentPath,
                }
            );
        }

        [HttpGet]
        [Route("Tools/Download/{*path}")]
        public async Task<IActionResult?> Download(string path)
        {
            (FileStreamResult?, string, string?) PathResult = await _pathReadWrite.FileStreamAsync(path);

            if (PathResult.Item1 is null)
            {
                TempData["Error"] = PathResult.Item3;

                return Redirect("/Tools/Path" + _pathReadWrite.MainPath(_pathReadWrite.GetParent(PathResult.Item2)));
            }

            return PathResult.Item1;
        }

        [HttpDelete]
        [Route("Tools/Delete/{*path}")]
        public async Task<JsonResult> Delete(string path)
        {
            (bool, string, string?) pathResult = await _pathReadWrite.DeleteFile(path);
            bool state = pathResult.Item1;

            path = pathResult.Item2;

            if (state)
            {
                return Json(new { response = pathResult.Item3, state });
            }
            else
            {
                TempData["Info"] = $@"""{path}""" + $" {CommonPhrases.deleted}";

                return Json(new { state });
            }
        }
    }
}