using Folder_Backup;
using System.Reflection;

namespace Folder_Backup_Test
{
    public class FileComparerTests
    {
        private static readonly string? _folderLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private string _sourceDirectoryPath;
        private string _targetDirectoryPath;

        private string _sourceFilePath;
        private string _targetFilePath;

        [OneTimeSetUp]
        public void Init()
        {
            if (_folderLocation == null)
            {
                throw new FileNotFoundException("Folder location is null");
            }

            _sourceDirectoryPath = Path.Combine(_folderLocation, "Source");
            _targetDirectoryPath = Path.Combine(_folderLocation, "Target");

            Utils.DeleteFolderIfExists(_sourceDirectoryPath);
            Utils.DeleteFolderIfExists(_targetDirectoryPath);

            Directory.CreateDirectory(_sourceDirectoryPath);
            Directory.CreateDirectory(_targetDirectoryPath);

            _sourceFilePath = Path.Combine(_sourceDirectoryPath, "File");
            _targetFilePath = Path.Combine(_targetDirectoryPath, "File");
        }

        [TearDown]
        public void Teardown()
        {
            Utils.ClearFolder(_sourceDirectoryPath);
            Utils.ClearFolder(_targetDirectoryPath);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Directory.Delete(_folderLocation, true);
        }

        [Test]
        public void AreFilesEqual_DifferentSize_False()
        {
            File.WriteAllText(_sourceFilePath, "ExampleTextLong");
            File.WriteAllText(_targetFilePath, "ExampleText");

            FileComparer fileComparer = new(_sourceFilePath, _targetFilePath);

            Assert.That(fileComparer.AreFilesEqual(), Is.False);
        }

        [Test]
        public void AreFilesEqual_SameSizeDifferentContent_False()
        {
            File.WriteAllText(_sourceFilePath, "ExampleText1");
            File.WriteAllText(_targetFilePath, "ExampleText2");

            FileComparer fileComparer = new(_sourceFilePath, _targetFilePath);

            Assert.That(fileComparer.AreFilesEqual(), Is.False);
        }

        [Test]
        public void AreFilesEqual_SizeZero_True()
        {
            File.WriteAllText(_sourceFilePath, "");
            File.WriteAllText(_targetFilePath, "");

            FileComparer fileComparer = new(_sourceFilePath, _targetFilePath);

            Assert.That(fileComparer.AreFilesEqual(), Is.True);
        }

        [Test]
        public void AreFilesEqual_SizeModuloULongIsZero_True()
        {
            File.WriteAllText(_sourceFilePath, "11111111");
            File.WriteAllText(_targetFilePath, "11111111");

            FileComparer fileComparer = new(_sourceFilePath, _targetFilePath);

            Assert.That(fileComparer.AreFilesEqual(), Is.True);
        }

        [Test]
        public void AreFilesEqual_BigFilesEqual_True()
        {
            using (FileStream fileStreamSource = new(_sourceFilePath, FileMode.Create))
            using (FileStream fileStreamTarget = new(_targetFilePath, FileMode.Create))
            {
                fileStreamSource.SetLength((long)2048 * 1024 * 1024);
                fileStreamSource.Position = (long)2048 * 1024 * 1024 - 1;
                fileStreamSource.WriteByte(1);

                fileStreamTarget.SetLength((long)2048 * 1024 * 1024);
                fileStreamTarget.Position = (long)2048 * 1024 * 1024 - 1;
                fileStreamTarget.WriteByte(1);
            }

            FileComparer fileComparer = new(_sourceFilePath, _targetFilePath);

            Assert.That(fileComparer.AreFilesEqual(), Is.True);
        }

        [Test]
        public void AreFilesEqual_BigFileNotEqual_False()
        {
            using (FileStream fileStreamSource = new(_sourceFilePath, FileMode.Create))
            using (FileStream fileStreamTarget = new(_targetFilePath, FileMode.Create))
            {
                fileStreamSource.SetLength((long)2048 * 1024 * 1024);
                fileStreamSource.Position = (long)2048 * 1024 * 1024 - 1;
                fileStreamSource.WriteByte(1);

                fileStreamTarget.SetLength((long)2048 * 1024 * 1024);
                fileStreamTarget.Position = (long)2048 * 1024 * 1024 - 1;
                fileStreamTarget.WriteByte(2);
            }

            FileComparer fileComparer = new(_sourceFilePath, _targetFilePath);

            Assert.That(fileComparer.AreFilesEqual(), Is.False);
        }
    }
}