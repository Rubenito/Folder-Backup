using CommandLine;
using Folder_Backup;
using System.Reflection;

namespace Folder_Backup_Test
{
    public class CommandLineOptionsTests
    {
        private static readonly string? _folderLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private string _sourcePath;
        private string _targetPath;
        private string _logsPath;

        private DirectoryInfo _sourceDirectory;
        private DirectoryInfo _targetDirectory;

        [OneTimeSetUp]
        public void Init()
        {
            if (_folderLocation == null)
            {
                throw new FileNotFoundException("Folder location is null");
            }

            _sourcePath = Path.Combine(_folderLocation, "Source");
            _targetPath = Path.Combine(_folderLocation, "Target");
            _logsPath = Path.Combine(_folderLocation, "Logs");
        }

        [SetUp]
        public void SetUp()
        {
            _sourceDirectory = Directory.CreateDirectory(_sourcePath);
            _targetDirectory = Directory.CreateDirectory(_targetPath);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_sourcePath))
            {
                CleanDirectory(_sourceDirectory);
            }

            if (Directory.Exists(_targetPath))
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
            string[] args = { "-s", "Wrong_Path", "-t", _targetPath, "-i", "10", "-l", _logsPath };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();
            Assert.That(hasErrorMessage, Is.True);
        }

        [Test]
        public void ParseArguments_FolderDoesExist_NoErrors()
        {
            string[] args = { "-s", _logsPath, "-t", _targetPath, "-i", "10", "-l", _logsPath };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();
            Assert.That(hasErrorMessage, Is.False);
        }

        [Test]
        public void ParseArguments_IntervalGreaterZero_NoErrors()
        {
            string[] args = { "-s", _sourcePath, "-t", _targetPath, "-i", "10" };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.False);
        }

        [Test]
        public void ParseArguments_IntervalToSmall_ThrowsError()
        {
            string[] args = { "-s", _sourcePath, "-t", _targetPath, "-i", "-10" };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.True);
        }

        private static void CleanDirectory(DirectoryInfo directory)
        {
            Directory.Delete(directory.FullName, true);
        }
    }
}
