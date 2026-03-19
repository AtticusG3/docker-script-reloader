using System.Text;
using System.Text.Json;
using Renci.SshNet;
using ScriptReloader.Models;

namespace ScriptReloader.Services;

public sealed class SshDockerService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private SshClient? _client;

    public bool IsConnected => _client is { IsConnected: true };

    public void Connect(string host, int port, string username, string password, TimeSpan connectionTimeout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentNullException.ThrowIfNull(password);

        Disconnect();

        var auth = new PasswordAuthenticationMethod(username, password);
        var connectionInfo = new ConnectionInfo(host, port, username, auth)
        {
            Timeout = connectionTimeout,
        };

        var client = new SshClient(connectionInfo);
        client.Connect();
        _client = client;
    }

    public void Disconnect()
    {
        if (_client is null)
        {
            return;
        }

        if (_client.IsConnected)
        {
            _client.Disconnect();
        }

        _client.Dispose();
        _client = null;
    }

    public async Task<IReadOnlyList<ContainerInfo>> ListContainersAsync(
        TimeSpan commandTimeout,
        CancellationToken cancellationToken = default)
    {
        const string remoteCommand = "docker ps -a --format '{{json .}}'";
        var result = await RunDockerCommandAsync(remoteCommand, commandTimeout, cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                BuildDockerErrorMessage("docker ps", result));
        }

        var list = new List<ContainerInfo>();
        var parseFailures = 0;
        foreach (var line in result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var row = JsonSerializer.Deserialize<ContainerInfo>(line, JsonOptions);
                if (row is not null)
                {
                    list.Add(row);
                }
            }
            catch (JsonException)
            {
                parseFailures++;
            }
        }

        var trimmedOut = result.StdOut.Trim();
        if (list.Count == 0 && !string.IsNullOrEmpty(trimmedOut))
        {
            var sample = trimmedOut.Split('\n')[0];
            if (sample.Length > 240)
            {
                sample = sample[..240] + "...";
            }

            var hint = new StringBuilder();
            hint.Append("docker ps returned output but no rows could be parsed.");
            if (parseFailures > 0)
            {
                hint.Append(" JSON parse failures: ");
                hint.Append(parseFailures);
                hint.Append('.');
            }

            hint.Append(" First line: ");
            hint.Append(sample);
            if (!string.IsNullOrWhiteSpace(result.StdErr))
            {
                hint.Append(" stderr: ");
                hint.Append(result.StdErr.Trim());
            }

            throw new InvalidOperationException(hint.ToString());
        }

        return list;
    }

    public async Task RestartContainerAsync(
        string containerIdOrName,
        TimeSpan commandTimeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerIdOrName);

        var escaped = EscapeForSingleQuotedUnixShell(containerIdOrName);
        var remoteCommand = $"docker restart --time 10 '{escaped}'";
        var result = await RunDockerCommandAsync(remoteCommand, commandTimeout, cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                BuildDockerErrorMessage("docker restart", result));
        }
    }

    private async Task<DockerCommandResult> RunDockerCommandAsync(
        string remoteCommand,
        TimeSpan commandTimeout,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_client is null || !_client.IsConnected)
                    {
                        throw new InvalidOperationException("Not connected. Connect before running Docker commands.");
                    }

                    using var cmd = _client.CreateCommand(remoteCommand);
                    cmd.CommandTimeout = commandTimeout;
                    var asyncResult = cmd.BeginExecute();
                    var waitHandles = new[] { asyncResult.AsyncWaitHandle, cancellationToken.WaitHandle };
                    var index = WaitHandle.WaitAny(waitHandles, commandTimeout);
                    if (index == WaitHandle.WaitTimeout)
                    {
                        throw new TimeoutException($"SSH command timed out after {commandTimeout.TotalSeconds}s.");
                    }

                    if (index == 1)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    cmd.EndExecute(asyncResult);
                    var stdout = cmd.Result ?? "";
                    var stderr = cmd.Error ?? "";
                    return new DockerCommandResult(cmd.ExitStatus ?? -1, stdout, stderr);
                },
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string BuildDockerErrorMessage(string label, DockerCommandResult result)
    {
        var sb = new StringBuilder();
        sb.Append(label);
        sb.Append(" failed (exit ");
        sb.Append(result.ExitCode);
        sb.Append(").");
        if (!string.IsNullOrWhiteSpace(result.StdErr))
        {
            sb.Append(' ');
            sb.Append(result.StdErr.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(result.StdOut))
        {
            sb.Append(' ');
            sb.Append(result.StdOut.Trim());
        }

        return sb.ToString();
    }

    /// <summary>Wrap for use inside single-quoted POSIX shell argument.</summary>
    private static string EscapeForSingleQuotedUnixShell(string value)
    {
        return value.Replace("'", "'\\''", StringComparison.Ordinal);
    }

    public void Dispose()
    {
        Disconnect();
        _gate.Dispose();
    }
}
