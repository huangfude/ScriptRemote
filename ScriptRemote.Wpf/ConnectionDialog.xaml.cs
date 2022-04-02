using MahApps.Metro.Controls;
using Renci.SshNet;
using ScriptRemote.Core.Common;
using ScriptRemote.Core.Utils;
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
					Id = Id,
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

		public int Id 
		{
			get
			{
				int _id;
				if (int.TryParse(id.Text, out _id))
					return _id;
				return 0; 
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

		public int Sort
		{
			get
			{
				int _sort;
				if (int.TryParse(sort.Text, out _sort))
					return _sort;
				return 0;
			}
		}

		public ConnectionDialog()
		{
			InitializeComponent();

			//Loaded += (sender, e) => { MinHeight = ActualHeight; MaxHeight = ActualHeight; };
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
			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(address))
            {
				App.Current.DisplayError("Address or Username can not be empty", "Error");
				return;
            }
			if(string.IsNullOrEmpty(conName))
            {
				// 控件赋值
				connectName.Text = userName + "@" + address;
			}

			List<SettingMacros> macroList = new List<SettingMacros>();
			// 查找控件
			List<TextBox> patternList = ControlUtil.GetChildObjectList<TextBox>(macroStackPanel, "pattern");
			List<TextBox> commandList = ControlUtil.GetChildObjectList<TextBox>(macroStackPanel, "command");
            // 遍历
            for (int i = 0; i < commandList.Count; i++)
            {
				SettingMacros macro = new SettingMacros();
				macro.Pattern = patternList[i].Text;
				macro.Command = commandList[i].Text;
				macroList.Add(macro);
			}

			if (SelectedSettings.Id > 0)
            {
				SettingsUtil.OnUpdate(SelectedSettings);
				long Id = SelectedSettings.Id;
				// 先删除
				MacrosUtil.OnDeleteBySettingId(Id);
				// 再保存
				MacrosUtil.OnSaveList(macroList, Id);
			} 
			else
            {
				long Id = SettingsUtil.OnSave(SelectedSettings);
				// 保存
				MacrosUtil.OnSaveList(macroList, Id);
			}

			MainWindow mainwin = (MainWindow)Application.Current.MainWindow;
			mainwin.settingsList.ItemsSource = SettingsUtil.List();

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

			//隐藏
			id.Text = settings.Id.ToString();
			sort.Text = settings.Sort.ToString();

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
			
			// id
			long settingId = Convert.ToInt64(settings.Id);
			List<SettingMacros> settingMacroList = MacrosUtil.List(settingId);
			// 遍历
			foreach (SettingMacros settingMacro in settingMacroList)
            {
				MacroControl macroControl = new MacroControl();
				// 显示值
				macroControl.pattern.Text = settingMacro.Pattern;
				macroControl.command.Text = settingMacro.Command;
				macroStackPanel.Children.Add(macroControl);
			}
		}

        private void macroAdd_Click(object sender, RoutedEventArgs e)
        {
			macroStackPanel.Children.Add(new MacroControl());
		}

       
    }
}
