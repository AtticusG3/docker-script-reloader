using System.Windows;
using Microsoft.Extensions.Configuration;
using ScriptReloader.Models;

namespace ScriptReloader;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(typeof(App).Assembly, optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();
        var ssh = config.GetSection("Ssh").Get<SshOptions>() ?? new SshOptions();

        MainWindow = new MainWindow(ssh);
        MainWindow.Show();
    }
}
