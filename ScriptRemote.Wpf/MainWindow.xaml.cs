using MahApps.Metro.Controls;
using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
		public Connection Connection
		{ get; private set; }

		public bool? Ok
		{ get; private set; }

		public MainWindow()
        {
            DataContext = this;

            InitializeComponent();

			settingsList.ItemsSource = GlobalVariable.SavedSettings;

			var store = IsolatedStorageFile.GetUserStoreForAssembly();
			if (store.FileExists(CommonConst.configPath))
			{
				try
				{
					using (var reader = new StreamReader(store.OpenFile(CommonConst.configPath, FileMode.Open)))
					{
						var document = XDocument.Load(reader);

						foreach (var settingsElement in document.XPathSelectElements("/Settings/Connection"))
						{
							var settings = new ConnectionSettings();

							string rawConnectName = settingsElement.XPathSelectElement("ConnectName")?.Value;
							if (rawConnectName != null)
								settings.ConnectName = rawConnectName;

							string rawServerAddress = settingsElement.XPathSelectElement("ServerAddress")?.Value;
							if (rawServerAddress != null)
								settings.ServerAddress = rawServerAddress;

							string rawServerPort = settingsElement.XPathSelectElement("ServerPort")?.Value;
							if (rawServerPort != null)
							{
								int serverPort = 0;
								int.TryParse(rawServerPort, out serverPort);
								settings.ServerPort = serverPort;
							}

							string rawUsername = settingsElement.XPathSelectElement("Username")?.Value;
							if (rawUsername != null)
								settings.Username = rawUsername;

							string rawPassword = settingsElement.XPathSelectElement("Password")?.Value;
							if (rawPassword != null)
								settings.Password = rawPassword;

							string rawKeyFilePath = settingsElement.XPathSelectElement("KeyFilePath")?.Value;
							if (rawKeyFilePath != null)
								settings.KeyFilePath = rawKeyFilePath;

							GlobalVariable.SavedSettings.Add(settings);
						}
					}

					if (GlobalVariable.SavedSettings.Count > 0)
						settingsList.SelectedIndex = 0;
				}
				catch (Exception ex) when (ex is IOException || ex is XmlException)
				{
					MessageBox.Show(this, "Could not access your saved settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void settingsListItem_Delete(object sender, RoutedEventArgs e)
		{
			GlobalVariable.SavedSettings.Remove((sender as MenuItem).Tag as ConnectionSettings);
		}

        private async void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			// 鼠标左键双击
			if (e.ChangedButton == MouseButton.Left)
			{
				IsEnabled = false;
				try
				{
					ConnectionSettings SelectedSettings = (sender as Button).Tag as ConnectionSettings;
					Connection = await App.Current.MakeConnectionAsync(SelectedSettings, App.DefaultTerminalCols, App.DefaultTerminalRows);
					Ok = true;
					Close();
				}
				catch (ConnectException ex)
				{
					IsEnabled = true;
				}
			}
		}
    }
}
