using Microsoft.AspNetCore.Mvc;
using Server_Manager_Application.Models.Messaging;
using Server_Manager_Application.Models.Nativization;


namespace Server_Manager_Application.Runtime.HighLevel
{
    public class PathReadWrite
    {
        private static readonly string basePath = GetHome();


        public string MainPath(string path)
        {
            return path.Replace(basePath, "");
        }
        
        public string? FullPath(string? path)
        {
            if (path is null)
            {
                return path;
            }
            
            return Path.Combine(basePath, path);
        }

        private static string GetHome()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        private static long GetSizeWithRecursion(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists)
            {
                throw new DirectoryNotFoundException("Directory does not exist.");
            }

            long size = 0;

            try
            {
                size += directory.GetFiles().Sum(file => file.Length);
                size += directory.GetDirectories().Sum(GetSizeWithRecursion);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"We do not have access to {directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"We encountered an error processing {directory}: ");
                Console.WriteLine($"{ex.Message}");
            }

            return size;
        }

        private async Task<List<FileData>> ViewPath(string path)
        {
            List<FileData> pathListList = new List<FileData>();

            int i = 0;
            foreach (string file in Directory.GetDirectories(path).Concat(Directory.GetFiles(path)))
            {
                pathListList.Add(await CompileFileInfo(file, i));
                i++;
            }

            return pathListList;
        }

        public string GetParent(string? path)
        {
            if (path is null)
            {
                return basePath;
            }

            try
            {
                path = Directory.GetParent(path)?.ToString();

                if (Path.Exists(path))
                {
                    return path;
                }

                throw new Exception();
            }
            catch
            {
                return GetParent(path);
            }
        }

        public async Task<FileData> CompileFileInfo(string path, int id)
        {
            return await Task.Run(() =>
            {
                bool isFile = File.Exists(path);

                if (isFile)
                {
                    FileInfo fileAttributes = new FileInfo(path);

                    return new FileData
                    {
                        id = id,
                        isFile = isFile,

                        _class = "table-primary",
                        fileName = fileAttributes.Name,

                        fileSize = (long)(fileAttributes.Length * 0.000001),

                        fileType = path.Split(".").Last(),
                        path = path,

                        created = fileAttributes.CreationTimeUtc,
                        modified = fileAttributes.LastWriteTimeUtc,
                        accessed = fileAttributes.LastAccessTimeUtc
                    };
                }

                DirectoryInfo folderAttributes = new DirectoryInfo(path);

                long totalSize = 0;

                try 
                {
                    totalSize += Directory.EnumerateFiles(path).Sum(file => new FileInfo(file).Length);
                }
                catch { }
               
                /*foreach (string dir in Directory.EnumerateDirectories(path))
                {
                    totalSize += GetSizeWithRecursion(new DirectoryInfo(dir));
                }*/

                return new FileData
                {
                    id = id,
                    isFile = isFile,

                    _class = "table-warning",
                    fileName = folderAttributes.Name,

                    fileSize = (long)(totalSize * 0.000001),

                    fileType = CommonWords.folder,
                    path = path,

                    created = folderAttributes.CreationTimeUtc,
                    modified = folderAttributes.LastWriteTimeUtc,
                    accessed = folderAttributes.LastAccessTimeUtc
                };
            });
        }

        public async Task<(List<FileData>, string, string?, bool)> AccessDirectoryAsync(string? selectedPath = null, short maxSize = -1)
        {
            string? errorMsg = null;
            List<FileData> paths;

            try
            {
                if (selectedPath is not null)
                {                     
                    selectedPath = Path.Combine(basePath, selectedPath);
                    paths = await ViewPath(selectedPath);

                    return (
                        (paths.Count > 0) ? paths[..((maxSize > 0) ? maxSize : ^0)] : paths,
                        selectedPath,
                        errorMsg,
                        false
                    );
                }

                paths = await ViewPath(basePath);

                return (
                    (paths.Count > 0) ? paths[..((maxSize > 0) ? maxSize : ^0)] : paths,
                    basePath,
                    errorMsg,
                    false
                );
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                selectedPath = GetParent(selectedPath);
            }

            paths = await ViewPath(selectedPath);

            return (
                (paths.Count > 0) ? paths[..((maxSize > 0) ? maxSize : ^0)] : paths,
                selectedPath,
                errorMsg,
                true
            );
        }

        public async Task<(FileStreamResult?, string, string?)> FileStreamAsync(string path)
        {
            path = Path.Combine(basePath, path);

            try
            {
                FileStream stream = new FileStream(
                    path,

                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,

                    bufferSize: 81920,
                    useAsync: true
                );

                // await a dummy read if you need to ensure open success
                await Task.Yield(); // keeps it truly async-friendly

                FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/octet-stream")
                {
                    FileDownloadName = Path.GetFileName(path)
                };

                return (fileStreamResult, path, null);
            }
            catch (Exception ex)
            {
                return (null, path, ex.Message);
            }

        }
        
        public async Task<(bool, string, string?)> DeleteFile(string path)
        {
            return await Task.Run(() =>
            {
                path = Path.Combine(basePath, path);

                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else
                    {
                        Directory.Delete(path);
                    }

                    return (false, path, string.Empty);
                }
                catch(Exception ex)
                {
                    return (true, path, ex.Message);
                }
            });
        }
    }
}