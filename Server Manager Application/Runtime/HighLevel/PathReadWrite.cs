namespace Server_Manager_Application.Runtime.HighLevel
{
    public static class PathReadWrite
    {
        private static string currentPath = GetHome();


        public static string GetHome()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        private static string[] ViewPath()
        {
            return Directory.GetDirectories(currentPath).Concat(Directory.GetFiles(currentPath)).ToArray();
        }

        public static (string[], string, string?, bool) AccessDirectory(string? selectedPath = null, short maxSize = -1)
        {
            string? errorMsg = null;
            bool pathExists;

            try
            {
                if (selectedPath is not null)
                {
                    pathExists = Path.Exists(selectedPath);
                    if (pathExists)
                    {
                        currentPath = selectedPath;

                        return (
                            ViewPath()[..maxSize],
                            currentPath,
                            errorMsg,
                            pathExists
                        );
                    }

                    throw new DirectoryNotFoundException("No such directory.");
                }

                pathExists = Path.Exists(currentPath);
                if (pathExists)
                {
                    return (
                        ViewPath()[..maxSize],
                        currentPath,
                        errorMsg,
                        pathExists
                    );
                }

                throw new DirectoryNotFoundException("No such directory.");
            }
            catch (Exception ex)
            {
                pathExists = false;
                errorMsg = ex.Message;
            }

            return (
                ViewPath()[..maxSize],
                currentPath,
                errorMsg,
                pathExists
            );
        }

        public static (string[], string, string?, bool) BackDirectory(short maxSize = -1)
        {
            string? errorMsg = null;
            bool pathExists;

            try
            {
                string? tempPath = Directory.GetParent(currentPath)?.ToString();
                if (tempPath is not null)
                {
                    pathExists = Path.Exists(tempPath);
                    if (pathExists)
                    {
                        currentPath = tempPath;

                        return (
                            ViewPath()[..maxSize],
                            currentPath,
                            errorMsg,
                            pathExists
                        );
                    }
                }

                throw new DirectoryNotFoundException("No such directory.");
            }
            catch (Exception ex)
            {
                currentPath = GetHome();
                pathExists = false;
                errorMsg = ex.Message;
            }

            return (
                ViewPath()[..maxSize],
                currentPath,
                errorMsg,
                pathExists
            );
        }
    }
}