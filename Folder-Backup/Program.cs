using CommandLine;
using Folder_Backup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<CommandLineOptions>(args)
        .WithParsed<CommandLineOptions>(clo =>
        {
            Host.CreateDefaultBuilder()
                .ConfigureServices(
                    services => 
                        services.AddHostedService<FileBackupWriter>(
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