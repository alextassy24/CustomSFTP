using CustomSftpTool.Interfaces;
using Renci.SshNet;

namespace CustomSftpTool.Services
{
    public class SftpServiceFactory(
        string host,
        string userName,
        string? password = null,
        string? privateKeyPath = null
    ) : ISftpServiceFactory
    {
        private readonly string _host = host;
        private readonly string _userName = userName;
        private readonly string _password = password ?? string.Empty;
        private readonly string _privateKeyPath = privateKeyPath ?? string.Empty;

        public ISftpService Create()
        {
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
            var sftpClient = new SftpClient(connectionInfo);

            return new SftpService(sftpClient);
        }
    }
}
