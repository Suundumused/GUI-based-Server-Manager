using System.Net;
using Server_Manager_Application.Models.Security;


namespace Server_Manager_Application.Services.Security.Interface
{
    public interface IService
    {
        Task<short> ValidateUserAsync(string username, string password, IPAddress ipAddress);

        Dictionary<IPAddress, UserState> GetStaticRemoteAttempts();
    }
}
