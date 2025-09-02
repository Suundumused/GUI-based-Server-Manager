namespace Server_Manager_Application.Models.Messaging
{
    public class MultiMediaCardMessage
    {
        public int id { get; set; }

        public string header { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string imageSrc { get; set; } = string.Empty;
        public string buttonText { get; set; } = string.Empty;
        public string? controller {  get; set; } = null;
        public string? action { get; set; } = null;
    }
}
