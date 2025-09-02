namespace Server_Manager_Application.Models.Static
{
    public static class LocalUserInfo
    {
        public static readonly int? id = 0;

        public static readonly string? username = Environment.UserName;
        public static readonly string? email = string.Empty;
        public static readonly string? domain = Environment.UserDomainName;
    }
}
