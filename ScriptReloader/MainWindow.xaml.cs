using System.Collections.ObjectModel;
using System.Windows;
using ScriptReloader.Models;
using ScriptReloader.Services;

namespace ScriptReloader;

public partial class MainWindow : Window
{
    private readonly SshOptions _defaults;
    private readonly SshDockerService _sshDocker = new();
    private readonly ObservableCollection<ContainerInfo> _containers = new();
    private CancellationTokenSource? _operationCts;

    public MainWindow(SshOptions defaults)
    {
        _defaults = defaults;
        InitializeComponent();
        ContainersGrid.ItemsSource = _containers;
        Loaded += OnLoaded;
        Closed += OnClosedHandler;
        ContainersGrid.SelectionChanged += (_, _) => UpdateActionButtons();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var hadSavedSession = false;
        if (SessionConnectionStore.TryLoad(out var savedHost, out var savedPort, out var savedUser, out var savedPassword))
        {
            hadSavedSession = true;
            HostBox.Text = savedHost;
            PortBox.Text = savedPort > 0 ? savedPort.ToString() : "22";
            UserBox.Text = savedUser;
            if (!string.IsNullOrEmpty(savedPassword))
            {
                PasswordBox.Password = savedPassword;
            }

            RememberConnectionCheckBox.IsChecked = true;
        }
        else
        {
            HostBox.Text = _defaults.Host;
            PortBox.Text = _defaults.Port > 0 ? _defaults.Port.ToString() : "22";
            UserBox.Text = _defaults.Username;
            if (!string.IsNullOrEmpty(_defaults.Password))
            {
                PasswordBox.Password = _defaults.Password;
            }

            RememberConnectionCheckBox.IsChecked = false;
        }

        if (hadSavedSession)
        {
            _ = AutoConnectFromSavedSessionAsync();
        }
        else
        {
            SetStatus("Disconnected. Enter host and credentials, then Connect.");
        }
    }

    private void OnClosedHandler(object? sender, EventArgs e)
    {
        if (RememberConnectionCheckBox.IsChecked == true)
        {
            PersistRememberedSession();
        }
        else
        {
            SessionConnectionStore.Clear();
        }

        _operationCts?.Cancel();
        _operationCts?.Dispose();
        _sshDocker.Dispose();
    }

    private void RememberConnectionCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        SessionConnectionStore.Clear();
    }

    private void PersistRememberedSession()
    {
        if (RememberConnectionCheckBox.IsChecked != true)
        {
            return;
        }

        _ = TryParsePort(out var port);
        var host = HostBox.Text.Trim();
        var user = UserBox.Text.Trim();
        var pwd = PasswordBox.Password;
        SessionConnectionStore.Save(host, port, user, string.IsNullOrEmpty(pwd) ? null : pwd);
    }

    private void SetStatus(string message)
    {
        StatusText.Text = message;
    }

    private void SetBusy(bool busy)
    {
        var connected = _sshDocker.IsConnected;
        ConnectButton.IsEnabled = !busy && !connected;
        DisconnectButton.IsEnabled = !busy && connected;
        RefreshButton.IsEnabled = !busy && connected;
        RestartAllRunningButton.IsEnabled = !busy && connected;
        HostBox.IsEnabled = !busy && !connected;
        PortBox.IsEnabled = !busy && !connected;
        UserBox.IsEnabled = !busy && !connected;
        PasswordBox.IsEnabled = !busy && !connected;
        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        var hasSelection = ContainersGrid.SelectedItems.Count > 0;
        var canAct = _sshDocker.IsConnected && hasSelection && RefreshButton.IsEnabled;
        StopButton.IsEnabled = canAct;
        StartButton.IsEnabled = canAct;
        RestartButton.IsEnabled = canAct;
    }

    private bool TryParsePort(out int port)
    {
        port = 22;
        var text = PortBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        if (!int.TryParse(text, out var p) || p is < 1 or > 65535)
        {
            return false;
        }

        port = p;
        return true;
    }

    private string GetPassword()
    {
        var fromUi = PasswordBox.Password;
        if (!string.IsNullOrEmpty(fromUi))
        {
            return fromUi;
        }

        return _defaults.Password ?? "";
    }

    private TimeSpan ConnectionTimeout => TimeSpan.FromSeconds(Math.Clamp(_defaults.CommandTimeoutSeconds, 5, 600));

    private TimeSpan CommandTimeout => TimeSpan.FromSeconds(Math.Clamp(_defaults.CommandTimeoutSeconds, 5, 600));

    private async Task AutoConnectFromSavedSessionAsync()
    {
        await Task.Yield();

        var host = HostBox.Text.Trim();
        var user = UserBox.Text.Trim();
        var password = GetPassword();

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
        {
            SetStatus("Disconnected. Saved connection needs host, user, and password. Enter missing values and Connect.");
            return;
        }

        if (!TryParsePort(out var port))
        {
            SetStatus("Disconnected. Fix the port, then Connect.");
            return;
        }

        _operationCts?.Cancel();
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();
        await RunConnectFlowAsync(host, port, user, password, _operationCts.Token).ConfigureAwait(true);
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        var host = HostBox.Text.Trim();
        var user = UserBox.Text.Trim();
        var password = GetPassword();

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show(this, "Host, user, and password are required.", "Connect", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!TryParsePort(out var port))
        {
            MessageBox.Show(this, "Port must be a number between 1 and 65535.", "Connect", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _operationCts?.Cancel();
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();
        var token = _operationCts.Token;

        await RunConnectFlowAsync(host, port, user, password, token).ConfigureAwait(true);
    }

    private async Task RunConnectFlowAsync(string host, int port, string user, string password, CancellationToken token)
    {
        try
        {
            SetBusy(true);
            SetStatus("Connecting...");
            await Task.Run(
                    () => _sshDocker.Connect(host, port, user, password, ConnectionTimeout),
                    token)
                .ConfigureAwait(true);

            SetStatus("Connected. Loading containers...");
            await RefreshContainersAsync(token).ConfigureAwait(true);
            SetStatus($"Connected to {host}:{port}. {_containers.Count} container(s).");

            if (RememberConnectionCheckBox.IsChecked == true)
            {
                SessionConnectionStore.Save(host, port, user, password);
            }
        }
        catch (OperationCanceledException)
        {
            SetStatus("Canceled.");
        }
        catch (Exception ex)
        {
            SetStatus("Connection failed.");
            MessageBox.Show(this, ex.Message, "SSH connect", MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                _sshDocker.Disconnect();
            }
            catch
            {
                // ignore cleanup errors
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        _operationCts?.Cancel();
        _sshDocker.Disconnect();
        _containers.Clear();
        SetStatus("Disconnected.");
        SetBusy(false);
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        _operationCts?.Cancel();
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();
        try
        {
            SetBusy(true);
            SetStatus("Refreshing...");
            await RefreshContainersAsync(_operationCts.Token).ConfigureAwait(true);
            SetStatus($"Refreshed. {_containers.Count} container(s).");
        }
        catch (OperationCanceledException)
        {
            SetStatus("Canceled.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Docker", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatus("Refresh failed.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void RestartAllRunningButton_OnClick(object sender, RoutedEventArgs e)
    {
        const string confirmMessage =
            "Restart every running container on the remote host? This runs: docker restart $(docker ps -q)";
        var confirm = MessageBox.Show(
            this,
            confirmMessage,
            "Restart all running",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        _operationCts?.Cancel();
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();
        var token = _operationCts.Token;
        try
        {
            SetBusy(true);
            SetStatus("Restarting all running containers...");
            await _sshDocker.RestartAllRunningContainersAsync(CommandTimeout, token).ConfigureAwait(true);
            SetStatus("Refreshing...");
            await RefreshContainersAsync(token).ConfigureAwait(true);
            SetStatus($"Restart all running completed. {_containers.Count} container(s) listed.");
        }
        catch (OperationCanceledException)
        {
            SetStatus("Canceled.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Docker restart all", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatus("Restart all running failed.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RefreshContainersAsync(CancellationToken cancellationToken)
    {
        var list = await _sshDocker.ListContainersAsync(CommandTimeout, cancellationToken).ConfigureAwait(true);
        _containers.Clear();
        foreach (var row in list)
        {
            _containers.Add(row);
        }
    }

    private static string BuildActionConfirmMessage(string verb, IReadOnlyList<ContainerInfo> items)
    {
        if (items.Count == 0)
        {
            return "";
        }

        if (items.Count == 1)
        {
            var c = items[0];
            return $"{verb} container \"{c.DisplayName}\" ({c.RestartTarget})?";
        }

        const int maxList = 12;
        var lines = items.Take(maxList).Select(c => $"- {c.DisplayName}");
        var body = string.Join(Environment.NewLine, lines);
        if (items.Count > maxList)
        {
            body += $"{Environment.NewLine}... and {items.Count - maxList} more.";
        }

        return $"{verb} {items.Count} containers?{Environment.NewLine}{Environment.NewLine}{body}";
    }

    /// <summary>
    /// One SSH round-trip: <c>docker stop|start|restart</c> with every selected id/name as its own quoted argument.
    /// </summary>
    private async Task RunBatchedLifecycleAsync(
        string confirmVerb,
        string dialogTitle,
        string progressGerund,
        string dockerCliVerb,
        string errorDialogTitle,
        string failedStatusPhrase,
        string doneSummaryNoun,
        Func<IReadOnlyList<string>, TimeSpan, CancellationToken, Task> dockerInvoke)
    {
        var selected = ContainersGrid.SelectedItems.OfType<ContainerInfo>().ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var confirmText = BuildActionConfirmMessage(confirmVerb, selected);
        var confirm = MessageBox.Show(
            this,
            confirmText,
            dialogTitle,
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        var targets = selected.ConvertAll(static c => c.RestartTarget);

        _operationCts?.Cancel();
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();
        var token = _operationCts.Token;
        try
        {
            SetBusy(true);
            SetStatus(
                targets.Count == 1
                    ? $"{progressGerund} {selected[0].DisplayName}..."
                    : $"{progressGerund} {targets.Count} containers (one docker {dockerCliVerb})...");
            await dockerInvoke(targets, CommandTimeout, token).ConfigureAwait(true);
            SetStatus("Refreshing...");
            await RefreshContainersAsync(token).ConfigureAwait(true);
            SetStatus($"{doneSummaryNoun} issued for {targets.Count} container(s). {_containers.Count} listed.");
        }
        catch (OperationCanceledException)
        {
            SetStatus("Canceled.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, errorDialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatus(failedStatusPhrase);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void StopButton_OnClick(object sender, RoutedEventArgs e)
    {
        await RunBatchedLifecycleAsync(
                "Stop",
                "Stop",
                "Stopping",
                "stop",
                "Docker stop",
                "Stop failed.",
                "Stop",
                (ids, to, ct) => _sshDocker.StopContainersAsync(ids, to, ct))
            .ConfigureAwait(true);
    }

    private async void StartButton_OnClick(object sender, RoutedEventArgs e)
    {
        await RunBatchedLifecycleAsync(
                "Start",
                "Start",
                "Starting",
                "start",
                "Docker start",
                "Start failed.",
                "Start",
                (ids, to, ct) => _sshDocker.StartContainersAsync(ids, to, ct))
            .ConfigureAwait(true);
    }

    private async void RestartButton_OnClick(object sender, RoutedEventArgs e)
    {
        await RunBatchedLifecycleAsync(
                "Restart",
                "Restart",
                "Restarting",
                "restart",
                "Docker restart",
                "Restart failed.",
                "Restart",
                (ids, to, ct) => _sshDocker.RestartContainersAsync(ids, to, ct))
            .ConfigureAwait(true);
    }
}
