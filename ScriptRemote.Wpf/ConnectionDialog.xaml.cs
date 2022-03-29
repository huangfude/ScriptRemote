﻿using MahApps.Metro.Controls;
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
			App.Current.OnSave();
			// 退出
			Close();
		}

		private void close_Click(object sender, RoutedEventArgs e)
		{
			// 退出
			Close();
		}

		public void SettingsEdit(ConnectionSettings settings)
		{
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
