using System.IO;
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
        try
        {
            StartMainWindow();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Script Reloader failed to start",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void StartMainWindow()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory);

        var asm = Assembly.GetExecutingAssembly();
        MemoryStream? embeddedCopy = null;
        try
        {
            using (var stream = asm.GetManifestResourceStream(EmbeddedAppSettingsName))
            {
                if (stream is not null)
                {
                    embeddedCopy = new MemoryStream();
                    stream.CopyTo(embeddedCopy);
                    embeddedCopy.Position = 0;
                    builder.AddJsonStream(embeddedCopy);
                }
            }

            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(App).Assembly, optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var ssh = config.GetSection("Ssh").Get<SshOptions>() ?? new SshOptions();

            var window = new MainWindow(ssh);
            MainWindow = window;
            window.Show();
        }
        finally
        {
            embeddedCopy?.Dispose();
        }
    }
}
