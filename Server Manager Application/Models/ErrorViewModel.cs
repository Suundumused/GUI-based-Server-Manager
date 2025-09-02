namespace Server_Manager_Application.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public int? ErrorCode { get; set; }

        public string ErrorDescription { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
