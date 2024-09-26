using System;
using Renci.SshNet;
using Serilog;

namespace CustomSftpTool.Commands
{
    public static class SshCommands
    {
        public static SshClient CreateSshClient(string host, string username, string privateKeyPath)
        {
            try
            {
                var keyFile = new PrivateKeyFile(privateKeyPath);
                var keyAuth = new PrivateKeyAuthenticationMethod(username, keyFile);
                var connectionInfo = new ConnectionInfo(host, username, keyAuth);

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
                var cmd = sshClient.CreateCommand(command);
                var commandResult = cmd.Execute();

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
