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

		

	}
}
