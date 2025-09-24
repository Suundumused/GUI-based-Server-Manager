namespace Server_Manager_Application.Models.Messaging
{
    public class FileData
    {
        public int id { get; set; }
        public bool isFile { get; set; }

        public string _class { get; set; } = string.Empty;
        public string fileName { get; set; } = string.Empty;

        public long fileSize { get; set; }

        public string fileType { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;

        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public DateTime accessed { get; set; }
    }

    public class PathResult
    {
        public List<FileData>? files { get; set; }
        public string currentPath { get; set; } = string.Empty;
    }
}