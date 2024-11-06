using Renci.SshNet;
using Serilog;

namespace CustomSftpTool.Commands
{
    public static class SshCommands
    {
        public static SshClient CreateSshClient(
            string host,
            string username,
            string? password = null,
            string? privateKeyPath = null
        )
        {
            try
            {
                AuthenticationMethod authMethod;
                if (!string.IsNullOrEmpty(password))
                {
                    authMethod = new PasswordAuthenticationMethod(username, password);
                }
                else if (!string.IsNullOrEmpty(privateKeyPath))
                {
                    PrivateKeyFile keyFile = new(privateKeyPath);
                    authMethod = new PrivateKeyAuthenticationMethod(username, keyFile);
                }
                else
                {
                    throw new ArgumentException(
                        "Either password or private key path must be provided."
                    );
                }

                ConnectionInfo connectionInfo = new(host, username, authMethod);
                return new SshClient(connectionInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create SSH client.");
                throw;
            }
        }

        public static string? ExecuteCommand(SshClient sshClient, string command)
        {
            try
            {
                SshCommand cmd = sshClient.CreateCommand(command);
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
