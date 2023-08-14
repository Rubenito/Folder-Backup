using Folder_Backup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Folder_Backup_Test
{
    internal class FileBackupWriterTests
    {
        private static readonly string? _folderLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private DirectoryInfo _sourceDirectory;
        private DirectoryInfo _targetDirectory;
        private DirectoryInfo _logsDirectory;

        private FileInfo _sourceFile;
        private FileInfo _targetFile;
        private FileInfo _logFile;

        private CancellationTokenSource _cancellationTokenSource;
        private IHost _host;

        [OneTimeSetUp]
        public void Init()
        {
            if (_folderLocation == null)
            {
                throw new FileNotFoundException("Folder location is null");
            }

            string _sourceDirectoryPath = Path.Combine(_folderLocation, "Source");
            string _targetDirectoryPath = Path.Combine(_folderLocation, "Target");
            string _logDirectoryPath = Path.Combine(_folderLocation, "Logs");

            _sourceDirectory = Directory.CreateDirectory(_sourceDirectoryPath);
            _targetDirectory = Directory.CreateDirectory(_targetDirectoryPath);
            _logsDirectory = Directory.CreateDirectory(_logDirectoryPath);

            string _sourceFilePath = Path.Combine(_sourceDirectoryPath, "File.txt");
            string _targetFilePath = Path.Combine(_targetDirectoryPath, "File.txt");
            string _logFilePath = Path.Combine(_logDirectoryPath, Utils.GetLogFileName());

            _sourceFile = new FileInfo(_sourceFilePath);
            _targetFile = new FileInfo(_targetFilePath);
            _logFile = new FileInfo(_logFilePath);
        }

        [SetUp]
        public void SetUp()
        {
            _cancellationTokenSource = new();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(
                    services =>
                        services.AddHostedService<FileBackupWriter>(
                            serviceProvider => new FileBackupWriter(
                                _sourceDirectory.FullName,
                                _targetDirectory.FullName,
                                1,
                                _logsDirectory.FullName,
                                serviceProvider.GetRequiredService<ILogger<FileBackupWriter>>())))
                .Build();
        }

        [TearDown]
        public void Teardown()
        {
            Utils.ClearFolder(_sourceDirectory.FullName);
            Utils.ClearFolder(_targetDirectory.FullName);
            Utils.ClearFolder(_logsDirectory.FullName);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Directory.Delete(_sourceDirectory.FullName, true);
            Directory.Delete(_targetDirectory.FullName, true);
            Directory.Delete(_logsDirectory.FullName, true);
        }

        [Test]
        public void ExecuteAsync_NewFileCreatedBeforeStart_CopiedFile()
        {
            File.WriteAllText(_sourceFile.FullName, "Content");

            Task task = _host.StartAsync(_cancellationTokenSource.Token);
            Task.Delay(2000).Wait();
            _cancellationTokenSource.Cancel();

            _targetFile.Refresh();

            string text = File.ReadAllText(_logFile.FullName);
            bool containsText = text.Contains("New file created:", StringComparison.OrdinalIgnoreCase);

            Assert.Multiple(() =>
            {
                Assert.That(_targetFile.Exists, Is.True);
                Assert.That(containsText, Is.True);
            });

        }

        [Test]
        public void ExecuteAsync_NewFileCreatedDuringRuntime_CopiedFile()
        {
            Task task = _host.StartAsync(_cancellationTokenSource.Token);

            Task.Delay(2000).Wait();
            File.WriteAllText(_sourceFile.FullName, "Content");
            Task.Delay(2000).Wait();

            _cancellationTokenSource.Cancel();

            _targetFile.Refresh();

            string text = File.ReadAllText(_logFile.FullName);
            bool containsText = text.Contains("New file created:", StringComparison.OrdinalIgnoreCase);

            Assert.Multiple(() =>
            {
                Assert.That(_targetFile.Exists, Is.True);
                Assert.That(containsText, Is.True);
            });
        }

        [Test]
        public void ExecuteAsync_DeleteFileBeforeStart_RemovedFile()
        {
            File.WriteAllText(_targetFile.FullName, "Content");

            Task task = _host.StartAsync(_cancellationTokenSource.Token);
            Task.Delay(2000).Wait();
            _cancellationTokenSource.Cancel();

            _targetFile.Refresh();

            string text = File.ReadAllText(_logFile.FullName);
            bool containsText = text.Contains("File deleted:", StringComparison.OrdinalIgnoreCase);

            Assert.Multiple(() =>
            {
                Assert.That(_targetFile.Exists, Is.False);
                Assert.That(containsText, Is.True);
            });

        }

        [Test]
        public void ExecuteAsync_DeleteSourceFileDuringRuntime_RemovedFile()
        {
            File.WriteAllText(_sourceFile.FullName, "Content");
            File.WriteAllText(_targetFile.FullName, "Content");

            Task task = _host.StartAsync(_cancellationTokenSource.Token);
            Task.Delay(2000).Wait();
            _sourceFile.Delete();
            Task.Delay(2000).Wait();

            _cancellationTokenSource.Cancel();

            _targetFile.Refresh();

            string text = File.ReadAllText(_logFile.FullName);
            bool containsText = text.Contains("File deleted:", StringComparison.OrdinalIgnoreCase);

            Assert.Multiple(() =>
            {
                Assert.That(_targetFile.Exists, Is.False);
                Assert.That(containsText, Is.True);
            });
        }

        [Test]
        public void ExecuteAsync_DeleteTargetFileDuringRuntime_CopiedFile()
        {
            File.WriteAllText(_sourceFile.FullName, "Content");
            File.WriteAllText(_targetFile.FullName, "Content");

            Task task = _host.StartAsync(_cancellationTokenSource.Token);
            Task.Delay(2000).Wait();
            _targetFile.Delete();
            Task.Delay(2000).Wait();

            _cancellationTokenSource.Cancel();

            _targetFile.Refresh();

            string text = File.ReadAllText(_logFile.FullName);
            bool containsText = text.Contains("New file created:", StringComparison.OrdinalIgnoreCase);

            Assert.Multiple(() =>
            {
                Assert.That(_targetFile.Exists, Is.True);
                Assert.That(containsText, Is.True);
            });
        }

        [Test]
        public void ExecuteAsync_CreatedManyFilesDuringRuntime_CopiedFiles()
        {
            Task task = _host.StartAsync(_cancellationTokenSource.Token);

            Task.Delay(2000).Wait();
            for (int i = 0; i < 100; i++)
            {
                string path = _sourceFile.Directory.FullName + $"/{i}_" + _sourceFile.Name;
                File.WriteAllText(path, $"Content: {i}");
            }
            Task.Delay(2000).Wait();

            _cancellationTokenSource.Cancel();

            _targetFile.Refresh();

            string[] text = File.ReadAllLines(_logFile.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(_targetDirectory.EnumerateFiles().Count(), Is.EqualTo(100));
                Assert.That(text, Has.Length.EqualTo(100));
            });
        }

        [Test]
        public void ExecuteAsync_CreatedFileThatTakesLongerThanInterval_CopiedFiles()
        {
            Task task = _host.StartAsync(_cancellationTokenSource.Token);

            Task.Delay(2000).Wait();

            using (FileStream fileStreamSource = new(_sourceFile.FullName, FileMode.Create))
            {
                fileStreamSource.SetLength((long)512 * 1024 * 1024);
                fileStreamSource.Position = (long)512 * 1024 * 1024 - 1;
                fileStreamSource.WriteByte(1);
                fileStreamSource.Close();
            }

            Task.Delay(40000).Wait();

            _cancellationTokenSource.Cancel();
            _targetFile.Refresh();

            string text = File.ReadAllText(_logFile.FullName);
            bool containsText = text.Contains("New file created:", StringComparison.OrdinalIgnoreCase);

            Task.Delay(2000).Wait();

            Assert.Multiple(() =>
            {
                Assert.That(_targetFile.Exists, Is.True);
                Assert.That(containsText, Is.True);
            });
        }
    }
}
