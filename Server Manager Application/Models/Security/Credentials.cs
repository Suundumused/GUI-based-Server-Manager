using System.ComponentModel.DataAnnotations;


namespace Server_Manager_Application.Models.Security
{
    public class Credentials
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
