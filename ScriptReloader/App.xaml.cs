using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using ScriptReloader.Models;

namespace ScriptReloader;

public partial class App : Application
{
    private const string EmbeddedAppSettingsName = "ScriptReloader.appsettings.json";

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory);

        var asm = Assembly.GetExecutingAssembly();
        using (var stream = asm.GetManifestResourceStream(EmbeddedAppSettingsName))
        {
            if (stream is not null)
            {
                builder.AddJsonStream(stream);
            }
        }

        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(typeof(App).Assembly, optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();
        var ssh = config.GetSection("Ssh").Get<SshOptions>() ?? new SshOptions();

        MainWindow = new MainWindow(ssh);
        MainWindow.Show();
    }
}
