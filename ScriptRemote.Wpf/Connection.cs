using Renci.SshNet;
using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptRemote.Wpf
{
	public enum ConnectionError
	{
		PassphraseIncorrect,
		NetError,
		AuthenticationError,
	}

	public class ConnectionFailedEventArgs : EventArgs
	{
		public ConnectionError Error
		{ get; }

		public string Message
		{ get; }

		public ConnectionFailedEventArgs(ConnectionError error, string message)
		{
			Error = error;
			Message = message;
		}
	}

	public class Connection
	{
		Thread dataThread = null;

		public SshClient Client
		{ get; private set; }

		public ShellStream Stream
		{ get; private set; }

		public ConnectionSettings Settings
		{ get; private set; }

		public event EventHandler Connected;
		public event EventHandler<ConnectionFailedEventArgs> Failed;

		public Connection()
		{ }

		public void Connect(ConnectionSettings settings, int terminalCols, int terminalRows)
		{
			Settings = settings;
			if (dataThread != null)
				throw new InvalidOperationException("Already connecting to a server.");
			dataThread = new Thread(() =>
			{
				try
				{
					var authentications = new List<AuthenticationMethod>();
					if (!string.IsNullOrEmpty(settings.KeyFilePath))
					{
						var privateKeyFile = new PrivateKeyFile(settings.KeyFilePath, settings.KeyFilePassphrase);
						authentications.Add(new PrivateKeyAuthenticationMethod(settings.Username, privateKeyFile));
					}
					authentications.Add(new PasswordAuthenticationMethod(settings.Username, settings.Password));
					ConnectionInfo connectionInfo = new ConnectionInfo(settings.ServerAddress, Convert.ToInt32(settings.ServerPort), settings.Username, authentications.ToArray());

					Client = new SshClient(connectionInfo);
					Client.Connect();

					Client.KeepAliveInterval = TimeSpan.FromSeconds(20);

					Stream = Client.CreateShellStream("xterm-256color", (uint) terminalCols, (uint) terminalRows, 0, 0, 1000);

					if (Connected != null)
						Connected(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					ConnectionFailedEventArgs args = null;
					if (ex is Renci.SshNet.Common.SshPassPhraseNullOrEmptyException ||
						ex is InvalidOperationException)
						args = new ConnectionFailedEventArgs(ConnectionError.PassphraseIncorrect, ex.Message);
					else if (ex is SocketException)
						args = new ConnectionFailedEventArgs(ConnectionError.NetError, ex.Message);
					else if (ex is Renci.SshNet.Common.SshAuthenticationException)
						args = new ConnectionFailedEventArgs(ConnectionError.AuthenticationError, ex.Message);
					else
						throw;

					if (Failed != null)
						Failed(this, args);
				}
			});
			dataThread.Name = "Data Thread";
			dataThread.IsBackground = true;
			dataThread.Start();
		}
	}
}
