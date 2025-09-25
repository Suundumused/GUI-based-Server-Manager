namespace Server_Manager_Application.Models.Messaging
{
    public class AlertMessage
    {
        public int id { get; set; }

        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string classLike { get; set; } = "alert-danger";
    }
}
