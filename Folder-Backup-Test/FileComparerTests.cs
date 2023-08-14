using Folder_Backup;

namespace Folder_Backup_Test
{
    public class FileComparerTests
    {
        private static readonly string FOLDER_LOCATION = "C:\\Users\\Ruben\\Documents\\UnitTestFolders";

        private string _sourceFolder;
        private string _targetFolder;

        private string _sourceFilePath;
        private string _targetFilePath;

        [OneTimeSetUp]
        public void Init()
        {
            _sourceFolder = Path.Combine(FOLDER_LOCATION, "Source");
            _targetFolder = Path.Combine(FOLDER_LOCATION, "Target");

            Directory.CreateDirectory(_sourceFolder);
            Directory.CreateDirectory(_targetFolder);

            _sourceFilePath = Path.Combine(_sourceFolder, "File");
            _targetFilePath = Path.Combine(_targetFolder, "File");
        }

        [TearDown]
        public void Teardown() 
        {
            Utils.ClearFolder(_sourceFolder);
            Utils.ClearFolder(_targetFolder);
        }

        [OneTimeTearDown]
        public void Cleanup()
        { 
            Directory.Delete(_sourceFolder, true);
            Directory.Delete(_targetFolder, true);
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