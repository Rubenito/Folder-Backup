using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Folder_Backup
{
    public class FileBackupWriter : BackgroundService
    {
        private readonly string _source;
        private readonly string _target;
        private readonly float _interval;
        private readonly string _logFileLocation;

        private readonly ILogger _logger;

        public FileBackupWriter(string source,
            string target,
            float interval,
            string logFileLocation,
            ILogger<FileBackupWriter> logger)
        {
            _source = source;
            _target = target;
            _interval = interval;  
            _logFileLocation = logFileLocation;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_interval));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                UpdateFolder();
            }
        }

        private void UpdateFolder()
        {
            GetFilesAndFoldersAtLocation(_source, out HashSet<string> FoldersAtSource, out HashSet<string> FilesAtSource);
            GetFilesAndFoldersAtLocation(_target, out HashSet<string> FoldersAtTarget, out HashSet<string> FilesAtTarget);

            WriteFoldersAtTarget(FoldersAtSource, FoldersAtTarget);
            DeleteFoldersAtTarget(FoldersAtSource, FoldersAtTarget);

            WriteFilesAtTarget(FilesAtSource, FilesAtTarget);
            DeleteFilesAtTarget(FilesAtSource, FilesAtTarget);
        }

        private void GetFilesAndFoldersAtLocation(string location, out HashSet<string> FoldersAtSource, out HashSet<string> FilesAtSource)
        {
            FoldersAtSource = new();
            FilesAtSource = new();

            Queue<string> foldersToCheck = new();
            foldersToCheck.Enqueue(location);

            while (foldersToCheck.Count > 0)
            {
                string currentFolder = foldersToCheck.Dequeue();

                foreach (string folder in GetDirectoryNamesAtLocation(currentFolder))
                {
                    FoldersAtSource.Add(Path.GetRelativePath(location, folder));
                    foldersToCheck.Enqueue(folder);
                }

                foreach (string file in GetFileNamesAtLocation(currentFolder))
                {
                    FilesAtSource.Add(Path.GetRelativePath(location, file));
                }
            }
        }

        private void WriteFoldersAtTarget(HashSet<string> foldersAtSource, HashSet<string> foldersAtTarget)
        {
            foreach (string folder in foldersAtSource.Except(foldersAtTarget))
            {
                string targetPath = GetAbsolutePath(_target, folder);

                try
                {
                    Directory.CreateDirectory(targetPath);
                    Log($"New directory created: {targetPath}", LogLevel.Information);
                }
                catch (Exception e)
                {
                    Log($"Could not create: {targetPath}\nError: {e.Message}", LogLevel.Error);
                }
                
            }
        }

        private void DeleteFoldersAtTarget(HashSet<string> foldersAtSource, HashSet<string> foldersAtTarget)
        {
            foreach (string folder in foldersAtTarget.Except(foldersAtSource))
            {
                string targetPath = GetAbsolutePath(_target, folder);

                try
                {
                    Directory.Delete(targetPath);
                    Log($"Directory deleted: {targetPath}", LogLevel.Information);
                }
                catch (Exception e)
                {
                    Log($"Could not delete: {targetPath}\nError: {e.Message}", LogLevel.Error);
                }
                
            }
        }

        private void WriteFilesAtTarget(HashSet<string> filesAtSource, HashSet<string> filesAtTarget)
        {
            // Case: file does not yet exist at target 
            foreach (string file in filesAtSource.Except(filesAtTarget))
            {
                string sourcePath = GetAbsolutePath(_source, file);
                string targetPath = GetAbsolutePath(_target, file);

                try
                {
                    File.Copy(sourcePath, targetPath, true);
                    Log($"New file created: {targetPath}", LogLevel.Information);
                }
                catch (Exception e)
                {
                    Log($"Could not write: {targetPath}\nError: {e.Message}", LogLevel.Error);
                }
                
            }

            // Case: file does already exist at target 
            foreach (string file in filesAtSource.Intersect(filesAtTarget))
            {                
                string sourcePath = GetAbsolutePath(_source, file);
                string targetPath = GetAbsolutePath(_target, file);

                FileComparer fileComparer = new FileComparer(sourcePath, targetPath);
                if (!fileComparer.AreFilesEqual())
                {
                    try
                    {
                        File.Copy(sourcePath, targetPath, true);
                        Log($"File updated: {targetPath}", LogLevel.Information);
                    }
                    catch (Exception e)
                    {
                        Log($"Could not write: {targetPath}\nError: {e.Message}", LogLevel.Error);
                    }
                }
                
            }
        }

        private void DeleteFilesAtTarget(HashSet<string> filesAtSource, HashSet<string> filesAtTarget)
        {
            foreach (string file in filesAtTarget.Except(filesAtSource))
            {
                string targetPath = GetAbsolutePath(_target, file);

                try
                {
                    File.Delete(targetPath);
                    Log($"File deleted: {targetPath}", LogLevel.Information);
                }
                catch (Exception e)
                {
                    Log($"Could not delete: {targetPath}\nError: {e.Message}", LogLevel.Error);
                }
                
            }
        }

        private void Log(string message, LogLevel logLevel)
        {
            string logFilePath = Path.Combine(_logFileLocation, GetLogFileName());
            FileInfo logFile = new(logFilePath);
            if (!logFile.Exists) 
            { 
                FileStream logStream = logFile.Create();
                logStream.Close();
            }

            using (StreamWriter writer = logFile.AppendText())
            {
                writer.WriteLine($"[{logLevel}] {DateTime.Now}: {message}");
                writer.Close();
            } 
                
            _logger.Log(logLevel, message);
        }

        private string GetAbsolutePath(string location, string relativePath)
        {
            return Path.Combine(location, relativePath);
        }

        private IEnumerable<string> GetFileNamesAtLocation(string location) 
        {
            return Directory.EnumerateFiles(location);
        }

        private IEnumerable<string> GetDirectoryNamesAtLocation(string location)
        {
            return Directory.EnumerateDirectories(location);
        }

        private string GetLogFileName()
        {
            return $"folder_backup_log_{DateTime.Today.Year}_{DateTime.Today.Month}_{DateTime.Today.Day}.txt";
        }
    }
}
