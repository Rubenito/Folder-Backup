using CommandLine;
using Folder_Backup;
using NUnit.Framework;
using System.Diagnostics;
using System.Security.AccessControl;

namespace Folder_Backup_Test
{
    public class CommandLineOptionsTests
    {
        private static readonly string FOLDER_LOCATION = "C:\\Users\\Ruben\\Documents\\UnitTestFolders";

        private string _sourcePath;
        private string _targetPath;
        private string _logsPath;

        private DirectoryInfo _sourceDirectory;
        private DirectoryInfo _targetDirectory;
        private DirectoryInfo _logsDirectory;

        [OneTimeSetUp]
        public void Init()
        {
            _sourcePath = Path.Combine(FOLDER_LOCATION, "Source");
            _targetPath = Path.Combine(FOLDER_LOCATION, "Target");
            _logsPath = Path.Combine(FOLDER_LOCATION, "Logs");
        }

        [SetUp]
        public void SetUp()
        {
            _sourceDirectory = Directory.CreateDirectory(_sourcePath);
            _targetDirectory = Directory.CreateDirectory(_targetPath);
            _logsDirectory = Directory.CreateDirectory(_logsPath);
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

        [Test]
        public void ParseArguments_FolderDoesNotExist_ThrowsError()
        {
            string[] args = { _sourcePath, _targetPath, "10", _logsPath };

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();
            Assert.That(hasErrorMessage, Is.True);
        }

        [Test]
        public void ParseArguments_FoldersHaveRightPermissions_NoErrors()
        {
            string[] args = { "-s", _sourcePath, "-t", _targetPath };
            Directory.CreateDirectory(_sourcePath);
            Directory.CreateDirectory(_targetPath);

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.False);
        }

        [Test]
        public void ParseArguments_SourceFolderHasWrongPermissionsRead_ThrowsError()
        {
            string[] args = { "-s", _sourcePath, "-t", _targetPath };

            SetDirectoryAccessRights(ref _sourceDirectory, FileSystemRights.ReadData, false);

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.True);
        }

        [Test]
        public void ParseArguments_TargetFolderHasWrongPermissionsWrite_ThrowsError()
        {
            string[] args = { "-s", _sourcePath, "-t", _targetPath };

            SetDirectoryAccessRights(ref _targetDirectory, FileSystemRights.WriteData, false);

            bool hasErrorMessage = Parser.Default.ParseArguments<CommandLineOptions>(args).Errors.Any();

            Assert.That(hasErrorMessage, Is.True);
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

        private void SetDirectoryAccessRights(ref DirectoryInfo directory, FileSystemRights fileSystemRights, bool allow)
        {
            DirectorySecurity securityRulesNoRead = new DirectorySecurity();
            securityRulesNoRead.AddAccessRule(new FileSystemAccessRule("Users", 
                FileSystemRights.ReadData, 
                allow? AccessControlType.Allow : AccessControlType.Deny));
            directory.SetAccessControl(securityRulesNoRead);
        }

        private void CleanDirectory(DirectoryInfo directory)
        {
            SetDirectoryAccessRights(ref directory, FileSystemRights.ReadData, true);
            SetDirectoryAccessRights(ref directory, FileSystemRights.WriteData, true);
            Directory.Delete(directory.FullName, true);
        }
    }
}
