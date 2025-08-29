using CustomSftpTool.Interfaces;
using Renci.SshNet;
using Serilog;
using ShellProgressBar;

namespace CustomSftpTool.Services;

public class SshService : ISshService
{
    private readonly SshClient _sshClient;
    private readonly string _host;

    public SshService(
        string host,
        string userName,
        string? password = null,
        string? privateKeyPath = null
    )
    {
        _host = host;
        var password1 = password ?? string.Empty;
        var privateKeyPath1 = privateKeyPath ?? string.Empty;

        AuthenticationMethod authMethod = !string.IsNullOrEmpty(password1)
            ? new PasswordAuthenticationMethod(userName, password1)
            : new PrivateKeyAuthenticationMethod(
                userName,
                new PrivateKeyFile(privateKeyPath1)
            );

        var connectionInfo = new ConnectionInfo(_host, userName, authMethod);
        _sshClient = new SshClient(connectionInfo);
    }

    public void Connect()
    {
        try
        {
            if (!_sshClient.IsConnected)
            {
                _sshClient.Connect();
                Log.Information("Connected to SSH server at {host}.", _host);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to connect to SSH server.");
            throw;
        }
    }

    public void Disconnect()
    {
        if (_sshClient.IsConnected)
        {
            _sshClient.Disconnect();
            Log.Information("Disconnected from SSH server.");
        }
    }

    public string? ExecuteCommand(string command)
    {
        if (!_sshClient.IsConnected)
        {
            Log.Error("SSH client is not connected. Command execution failed.");
            return null;
        }

        var cmd = _sshClient.CreateCommand(command);
        cmd.Execute();

        if (!string.IsNullOrEmpty(cmd.Error))
        {
            Log.Error("Error executing command '{s}': {cmdError}", command, cmd.Error);
            return null;
        }

        return cmd.Result.Trim();
    }

    public int CountFilesInDirectory(string directoryPath)
    {
        var result = ExecuteCommand($"find {directoryPath} -type f | wc -l");
        return int.TryParse(result, out int fileCount) ? fileCount : 0;
    }

    public bool CreateTarArchive(string sourceDir, string archivePath, ProgressBar progressBar)
    {
        try
        {
            var command = $"tar -czvf {archivePath} -C / {sourceDir.TrimStart('/')}";
            using var cmd = _sshClient.CreateCommand(command);
            cmd.CommandTimeout = TimeSpan.FromMinutes(10);

            var asyncResult = cmd.BeginExecute();
            using var reader = new StreamReader(cmd.OutputStream);

            while (!asyncResult.IsCompleted || !reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    progressBar.Tick($"Adding: {Path.GetFileName(line)}");
                }
            }

            cmd.EndExecute(asyncResult);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create tar archive.");
            return false;
        }
    }
}