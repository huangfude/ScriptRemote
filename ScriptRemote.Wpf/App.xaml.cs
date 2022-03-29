using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace ScriptRemote.Wpf
{

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
		private static readonly object _writeLock = new object();

		public static new App Current
		{ get { return Application.Current as App; } }

		public App()
		{
			Dispatcher.UnhandledException += Dispatcher_UnhandledException;
		}

		private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			string message = $"{e.Exception.Message}\n\n{e.Exception.StackTrace}";
			MessageBox.Show(MainWindow, message, "An unhandled exception has occurred", MessageBoxButton.OK, MessageBoxImage.Error);
			Shutdown(1);
		}

		public void DisplayError(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		/// <summary>
		/// 保存信息
		/// </summary>
		public void OnSave()
		{
			var store = IsolatedStorageFile.GetUserStoreForAssembly();
			try
			{
				var document = new XDocument();
				document.Declaration = new XDeclaration("1.0", "utf-8", null);

				var root = new XElement(XName.Get("Settings"));
				document.Add(root);
				foreach (var settings in GlobalVariable.SavedSettings)
				{
					var connection = new XElement(XName.Get("Connection"));
					root.Add(connection);

					connection.Add(new XElement(XName.Get("ConnectName"), settings.ConnectName));
					connection.Add(new XElement(XName.Get("ServerAddress"), settings.ServerAddress));
					connection.Add(new XElement(XName.Get("ServerPort"), settings.ServerPort));
					connection.Add(new XElement(XName.Get("Username"), settings.Username));
					connection.Add(new XElement(XName.Get("Password"), settings.Password));
					connection.Add(new XElement(XName.Get("KeyFilePath"), settings.KeyFilePath));
				}


				lock (_writeLock)
				{
					if(store.FileExists(CommonConst.configPath))
                    {
						store.DeleteFile(CommonConst.configPath);
					}

					using (var isoStream = new IsolatedStorageFileStream(CommonConst.configPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, store))
					{
						using (var writer = XmlWriter.Create(isoStream))
						{
							document.WriteTo(writer);
						}
					}
				}
			}
			catch (IOException)
			{
				DisplayError("Could not access your saved settings.", "Error");
			}
		}

	}
}
