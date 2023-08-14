using CommandLine;
using System.Reflection;
using System.Security.AccessControl;

namespace Folder_Backup
{
    public class CommandLineOptions
    {
        private string _source;
        [Option('s', "source", Required = true, HelpText = "The folder to backup from.")]
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new DirectoryNotFoundException($"Source directory '{value}' does not exist");
                }

                if (!DirectoryHasAccessRights(value, FileSystemRights.ReadData))
                {
                    throw new UnauthorizedAccessException($"Missing read access for directory '{value}'");
                }

                _source = value;
            }
        }

        private string _target;
        [Option('t', "target", Required = true, HelpText = "The folder to backup to.")]
        public string Target
        {
            get
            {
                return _target;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new DirectoryNotFoundException($"Target directory '{value}' does not exist");
                }

                if (!DirectoryHasAccessRights(value, FileSystemRights.ReadData))
                {
                    throw new UnauthorizedAccessException($"Missing read access for directory '{value}'");
                }

                if (!DirectoryHasAccessRights(value, FileSystemRights.WriteData))
                {
                    throw new UnauthorizedAccessException($"Missing write access for directory '{value}'");
                }

                _target = value;
            }
        }

        private float _interval = 300;
        [Option('i', "interval", Required = false, HelpText = "The time between beackups.")]
        public float Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException($"The interval needs to be greater 0, but is '{value}'");
                }
                _interval = value;
            }

        }

        private string _logFileLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); // Default log location to loaction of executable
        [Option('l', "logFileLocation", Required = false, HelpText = "The location of the logfile.")]
        public string LogFileLocation
        {
            get
            {
                return _logFileLocation;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new DirectoryNotFoundException($"Log file location directory '{value}' does not exist");
                }

                if (!DirectoryHasAccessRights(value, FileSystemRights.WriteData))
                {
                    throw new UnauthorizedAccessException($"Missing write access for directory '{value}'");
                }

                _logFileLocation = value;
            }
        }

        private bool DirectoryHasAccessRights(string path, FileSystemRights fileSystemRights)
        {
            // Check if read rights for directory are present
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var rules = directoryInfo.GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
            foreach (FileSystemAccessRule rule in rules)
            {
                if ((fileSystemRights & rule.FileSystemRights) != fileSystemRights)
                {
                    continue;
                }

                if (rule.AccessControlType == AccessControlType.Deny)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
