using System.Net;
using Server_Manager_Application.Models.Security;


namespace Server_Manager_Application.Common.Security.Interface
{
    public interface IService
    {
        Task<Int16> ValidateUserAsync(string username, string password, IPAddress ipAddress);

        Dictionary<IPAddress, UserState> GetStaticRemoteAttempts();
    }
}
