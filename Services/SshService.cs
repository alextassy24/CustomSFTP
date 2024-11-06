using CustomSftpTool.Interfaces;
using Renci.SshNet;
using Serilog;

namespace CustomSftpTool.Services
{
    public class SshService(
        SshClient sshClient,
        string host,
        string userName,
        string password = null,
        string privateKeyPath = null
    ) : ISshService
    {
        private SshClient _sshClient = sshClient;
        private readonly string _host = host;
        private readonly string _userName = userName;
        private readonly string _password = password ?? string.Empty;
        private readonly string _privateKeyPath = privateKeyPath ?? string.Empty;

        public void Connect()
        {
            Console.WriteLine($"Connecting to SSH with Host: {_host}, User: {_userName}");

            AuthenticationMethod authMethod;
            if (!string.IsNullOrEmpty(_password))
            {
                authMethod = new PasswordAuthenticationMethod(_userName, _password);
            }
            else if (!string.IsNullOrEmpty(_privateKeyPath))
            {
                PrivateKeyFile keyFile = new(_privateKeyPath);
                authMethod = new PrivateKeyAuthenticationMethod(_userName, keyFile);
            }
            else
            {
                throw new ArgumentException(
                    "Either password or private key path must be provided."
                );
            }

            var connectionInfo = new ConnectionInfo(_host, _userName, authMethod);

            try
            {
                _sshClient = new SshClient(connectionInfo);
                _sshClient.Connect();
                Log.Information("Connected to SSH server.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to connect to SSH server.");
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_sshClient.IsConnected)
                {
                    _sshClient.Disconnect();
                    Log.Information("Disconnected from SSH server.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to disconnect from SSH server.");
                throw;
            }
        }

        public string? ExecuteCommand(string command)
        {
            try
            {
                if (!_sshClient.IsConnected)
                {
                    Log.Error("SSH client is not connected. Command execution failed.");
                    return null;
                }

                SshCommand cmd = _sshClient.CreateCommand(command);
                string commandResult = cmd.Execute();

                if (!string.IsNullOrEmpty(cmd.Error))
                {
                    Log.Error($"Error executing command '{command}': {cmd.Error}");
                    return null;
                }

                Log.Debug($"Executed command '{command}' with result: {commandResult.Trim()}");
                return commandResult;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception executing command '{command}'.");
                return null;
            }
        }
    }
}
