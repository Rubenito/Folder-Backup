using CommandLine;
using System.Reflection;

namespace Folder_Backup
{
    /// <summary>
    /// The command line oprions expected by the program:
    /// -s "Source" -t "Target" [-i "Interval"] [-l "LogFile"]
    /// </summary>
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

        private string? _logFileLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); // Default log location to loaction of executable
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

                if (_logFileLocation == null)
                {
                    throw new DirectoryNotFoundException($"Could not find executable path to set default log file location");
                }

                _logFileLocation = value;
            }
        }
    }
}
