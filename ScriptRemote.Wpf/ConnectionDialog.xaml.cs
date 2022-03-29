using MahApps.Metro.Controls;
using Renci.SshNet;
using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ScriptRemote.Wpf
{

	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : MetroWindow
	{

		public ConnectionSettings SelectedSettings
		{
			get
			{
				return new ConnectionSettings()
				{
					ConnectName = ConnectName,
					ServerAddress = ServerAddress,
					ServerPort = ServerPort,
					Username = Username,
					Password = Password,
					KeyFilePath = KeyFilePath,
					KeyFilePassphrase = KeyFilePassphrase,
				};
			}
		}

		public string ConnectName
		{ get { return connectName.Text; } }

		public string ServerAddress
		{ get { return serverAddress.Text; } }

		public int ServerPort
		{
			get
			{
				int port;
				if (int.TryParse(serverPort.Text, out port))
					return port;
				return 0;
			}
		}

		public string Username
		{ get { return username.Text; } }

		public string Password
		{ get { return password.Password; } }

		public string KeyFilePath
		{ get { return keyPath.Text; } }

		public string KeyFilePassphrase
		{ get { return keyPassphrase.Password; } }


		public ConnectionDialog()
		{
			DataContext = this;

			InitializeComponent();

			Loaded += (sender, e) => { MinHeight = ActualHeight; MaxHeight = ActualHeight; };

		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
		}

		/// <summary>
		/// 保存链接信息
		/// </summary>
		protected void OnSave()
		{
			var store = IsolatedStorageFile.GetUserStoreForAssembly();
			try
			{
				var root = new XElement(XName.Get("Settings"));
				int index = 0;
				foreach (var settings in GlobalVariable.SavedSettings)
				{
					var connection = new XElement(XName.Get("Connection"));
					connection.Add(new XElement(XName.Get("Index"), index.ToString()));
					connection.Add(new XElement(XName.Get("ConnectName"), settings.ConnectName));
					connection.Add(new XElement(XName.Get("ServerAddress"), settings.ServerAddress));
					connection.Add(new XElement(XName.Get("ServerPort"), settings.ServerPort));
					connection.Add(new XElement(XName.Get("Username"), settings.Username));
					connection.Add(new XElement(XName.Get("Password"), settings.Password));
					connection.Add(new XElement(XName.Get("KeyFilePath"), settings.KeyFilePath));

					root.Add(connection);
					index++;
				}

				var document = new XDocument();
				document.Add(root);

				using (var writer = XmlWriter.Create(store.OpenFile(CommonConst.configPath, FileMode.Create)))
				{
					document.WriteTo(writer);
				}
			}
			catch (IOException)
			{
				App.Current.DisplayError("Could not access your saved settings.", "Error");
			}
		}

		 
		private void keyPathBrowse_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog();
			dialog.Filter = "All Files|*.*";
			dialog.Title = "Private Key File";

			bool result = dialog.ShowDialog(this).GetValueOrDefault(false);
			if (result)
			{
				keyPath.Text = dialog.FileName;
			}
		}

		private void save_Click(object sender, RoutedEventArgs e)
		{
			string userName = SelectedSettings.Username;
			string address = SelectedSettings.ServerAddress;
			string conName = SelectedSettings.ConnectName;
			if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(address))
            {
				App.Current.DisplayError("Address or Username can not be empty", "Error");
				return;
            }
			if(string.IsNullOrEmpty(conName))
            {
				// 控件赋值
				connectName.Text = userName + "@" + address;
			}
			GlobalVariable.SavedSettings.Add(SelectedSettings);

			OnSave();
		}

		private void close_Click(object sender, RoutedEventArgs e)
		{
			// 保存记录
			OnSave();
			// 退出
			Close();
		}

		private void settingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var settings = e.AddedItems[0] as ConnectionSettings;	// Let an exception happen if the items are not ConnectionSettings
				connectName.Text = settings.ConnectName;
				serverAddress.Text = settings.ServerAddress;
				serverPort.Text = settings.ServerPort.ToString();
				username.Text = settings.Username;
				//password.Clear();
				password.Password = settings.Password;
				keyPath.Text = settings.KeyFilePath;
				keyPassphrase.Clear();

				if (connectName.Text == "")
					connectName.Focus();
				if (serverAddress.Text == "")
					serverAddress.Focus();
				else if (serverPort.Text == "")
					serverPort.Focus();
				else if (username.Text == "")
					username.Focus();
				else if (keyPath.Text == "")
					password.Focus();
				else
					keyPassphrase.Focus();
			}
		}

		
	}
}
