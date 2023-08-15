namespace Folder_Backup_Test
{
    public static class Utils
    {
        public static void DeleteFolderIfExists(string folder) 
        {
            if(Directory.Exists(folder)) 
            {
                ClearFolder(folder);
                Directory.Delete(folder, true);
            }
        }

        public static void ClearFolder(string folder)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            System.IO.DirectoryInfo directoryInfo = new(folder);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                while (IsFileLocked(file))
                {
                    Thread.Sleep(1000);
                }
                file.Delete();
            }
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static string GetLogFileName()
        {
            return $"folder_backup_log_{DateTime.Today.Year}_{DateTime.Today.Month}_{DateTime.Today.Day}.txt";
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream? stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }
    }
}
