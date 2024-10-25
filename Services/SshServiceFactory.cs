using CustomSftpTool.Interfaces;
using Renci.SshNet;

namespace CustomSftpTool.Services
{
    public class SshServiceFactory(
        string host,
        string userName,
        string password,
        string privateKeyPath
    ) : ISshServiceFactory
    {
        private readonly string _host = host;
        private readonly string _userName = userName;
        private readonly string _password = password;
        private readonly string _privateKeyPath = privateKeyPath;

        public ISshService Create()
        {
            AuthenticationMethod authMethod;

            if (!string.IsNullOrEmpty(_password))
            {
                authMethod = new PasswordAuthenticationMethod(_userName, _password);
            }
            else if (!string.IsNullOrEmpty(_privateKeyPath))
            {
                var keyFile = new PrivateKeyFile(_privateKeyPath);
                authMethod = new PrivateKeyAuthenticationMethod(_userName, keyFile);
            }
            else
            {
                throw new ArgumentException(
                    "Either password or private key path must be provided."
                );
            }

            var connectionInfo = new ConnectionInfo(_host, _userName, authMethod);
            var sshClient = new SshClient(connectionInfo);

            return new SshService(sshClient, _host, _userName, _password, _privateKeyPath);
        }
    }
}
