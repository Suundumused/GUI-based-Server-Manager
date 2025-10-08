using System.Net;
using Server_Manager_Application.Models.Security;
using Server_Manager_Application.Models.Static;
using Server_Manager_Application.Services.Security.Interface;


namespace Server_Manager_Application.Services.Security.Public
{
    public class Service : IService
    {
        private static short _timeOut;
        private static short _maxLoginAttempts;

        private static Dictionary<IPAddress, UserState> remoteAttempts = new Dictionary<IPAddress, UserState>();
        

        public Service(short timeOut, short maxLoginAttempts) 
        {
            _timeOut = timeOut;
            _maxLoginAttempts = maxLoginAttempts;
        }

        private async Task IpRemovalScheduling(CancellationToken token, IPAddress ipAddress, short timeOut) 
        {
            await Task.Delay(TimeSpan.FromSeconds(timeOut), token);

            if (!remoteAttempts[ipAddress]._TokenSource.IsCancellationRequested) 
            {
                remoteAttempts[ipAddress]._TokenSource.Cancel();
            }

            remoteAttempts.Remove(ipAddress);
        }

        public Task<short> ValidateUserAsync(string username, string password, IPAddress ipAddress)
        {
            if (!remoteAttempts.ContainsKey(ipAddress))
            {
                remoteAttempts.Add(ipAddress, new UserState(0));
            }
            else if (remoteAttempts[ipAddress]._cAttempts == _maxLoginAttempts)
            {
                remoteAttempts[ipAddress]._cAttempts++;

                _ = IpRemovalScheduling(remoteAttempts[ipAddress]._TokenSource.Token, ipAddress, _timeOut);

                return Task.FromResult((short)0);
            }
            else if (remoteAttempts[ipAddress]._cAttempts > _maxLoginAttempts) 
            {
                return Task.FromResult((short)0);
            }

            if (username == LocalUserInfo.username)
            {
                if (PasswordInput.SecureEquals(PasswordInput.ConvertString(password)))
                {
                    if (!remoteAttempts[ipAddress]._TokenSource.IsCancellationRequested)
                    {
                        remoteAttempts[ipAddress]._TokenSource.Cancel();
                    }

                    remoteAttempts.Remove(ipAddress);

                    return Task.FromResult((short)2);
                }
            }

            remoteAttempts[ipAddress]._cAttempts++;

            return Task.FromResult((short)1);
        }

        public Dictionary<IPAddress, UserState> GetStaticRemoteAttempts()
        {
            return remoteAttempts;
        }
    }
}
