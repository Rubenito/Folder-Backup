using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Folder_Backup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(clo =>
            {
                Host.CreateDefaultBuilder()
                    .ConfigureServices(
                        services =>
                            services.AddHostedService(
                                serviceProvider => new FileBackupWriter(
                                    clo.Source,
                                    clo.Target,
                                    clo.Interval,
                                    clo.LogFileLocation,
                                    serviceProvider.GetRequiredService<ILogger<FileBackupWriter>>())))
                    .Build()
                    .Run();
            });
        }
    }
}