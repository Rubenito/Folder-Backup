using CommandLine;
using Folder_Backup;
using System.Reflection;

namespace Folder_Backup_Test
{
    public class CommandLineOptionsTests
    {
        private static readonly string? _folderLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private string _sourceDirectoryPath;
        private string _targetDirectoryPath;
        private string _logsDirectoryPath;

        private DirectoryInfo _sourceDirectory;
        private DirectoryInfo _targetDirectory;

        [OneTimeSetUp]
        public void Init()
        {
            if (_folderLocation == null)
            {
                throw new FileNotFoundException("Folder location is null");
            }

            _sourceDirectoryPath = Path.Combine(_folderLocation, "Source");
            _targetDirectoryPath = Path.Combine(_folderLocation, "Target");
            _logsDirectoryPath = Path.Combine(_folderLocation, "Logs");

            Utils.DeleteFolderIfExists(_sourceDirectoryPath);
            Utils.DeleteFolderIfExists(_targetDirectoryPath);
            Utils.DeleteFolderIfExists(_logsDirectoryPath);
        }

        [SetUp]
        public void SetUp()
        {
            _sourceDirectory = Directory.CreateDirectory(_sourceDirectoryPath);
            _targetDirectory = Directory.CreateDirectory(_targetDirectoryPath);
            _targetDirectory = Directory.CreateDirectory(_logsDirectoryPath);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_sourceDirectoryPath))
            {
                CleanDirectory(_sourceDirectory);
            }

            if (Directory.Exists(_targetDirectoryPath))
            {
                CleanDirectory(_targetDirectory);
            }

            Parser.Default.Dispose();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Directory.Delete(_folderLocation, true);
        }

        [Test]
        public void ParseArguments_FolderDoesNotExist_ThrowsError()
        {
            string[] args = { "-s", "Wrong_Path", "-t", _targetDirectoryPath, "-i", "10", "-l", _logsDirectoryPath };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();
            Assert.That(hasErrorMessage, Is.True);
        }

        [Test]
        public void ParseArguments_FolderDoesExist_NoErrors()
        {
            string[] args = { "-s", _sourceDirectoryPath, "-t", _targetDirectoryPath, };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();
            Assert.That(hasErrorMessage, Is.False);
        }

        [Test]
        public void ParseArguments_IntervalGreaterZero_NoErrors()
        {
            string[] args = { "-s", _sourceDirectoryPath, "-t", _targetDirectoryPath, "-i", "10" };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.False);
        }

        [Test]
        public void ParseArguments_IntervalToSmall_ThrowsError()
        {
            string[] args = { "-s", _sourceDirectoryPath, "-t", _targetDirectoryPath, "-i", "-10" };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.True);
        }

        private static void CleanDirectory(DirectoryInfo directory)
        {
            Directory.Delete(directory.FullName, true);
        }
    }
}
