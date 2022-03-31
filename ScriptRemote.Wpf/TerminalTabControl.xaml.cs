using Renci.SshNet;
using ScriptRemote.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScriptRemote.Wpf
{
    /// <summary>
    /// Interaction logic for TerminalTabControl.xaml
    /// </summary>
    public partial class TerminalTabControl : UserControl
    {
		TerminalViewModel viewModel;
		IWindowStreamNotifier notifier;

		bool initialSized = false;
		public TerminalTabControl()
		{
			InitializeComponent();

			viewModel = new TerminalViewModel();
			DataContext = viewModel;

			viewModel.Error += ViewModel_Error;

			//SizeChanged += this_SizeChanged;

			//Loaded += this_Loaded;

		}

		private void ViewModel_Error(object sender, ErrorEventArgs e)
		{
			MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public void Connect(ShellStream stream, ConnectionSettings settings)
		{
			notifier?.Stop();
			notifier = new ShellStreamNotifier(terminalControl, stream);
			//notifier = new LoadingShellStreamNotifier(terminalControl, File.OpenRead("input.log"));
			//notifier = new SavingShellStreamNotifier(terminalControl, stream, File.OpenWrite("input.log"));
			viewModel.Connect(notifier, stream, settings);
			notifier.Start();

			// 默认名称
			//viewModel.Title = settings.Username + "@" + settings.ServerAddress;

			terminalControl.Terminal = viewModel.Terminal;
		}


		public void this_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
			// 不可见的时候
			if((bool)e.NewValue == false)
            {
				notifier?.Stop();
			}
		}

		public void this_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			/*
			if (!initialSized)
			{
				initialSized = true;
				return;
			}
			*/

			double width = ActualWidth;
			double height = ActualHeight;

			if (typeof(TabControl).FullName.Equals(sender.GetType().FullName))
            {
				TabControl tabControl = sender as TabControl;
				width = tabControl.ActualWidth;
				height = tabControl.ActualHeight;
				
				// 控件大小
				Width = width;
				Height = height;
			}


			//var transformer = PresentationSource.FromVisual(terminalControl).CompositionTarget.TransformToDevice;
			//var stops = transformer.Transform(new System.Windows.Point(terminalControl.CharWidth, terminalControl.CharHeight));
			double horizontalStop = terminalControl.CharWidth;
			double verticalStop = terminalControl.CharHeight;


			int newCols = (int)((width - SystemParameters.ScrollWidth + 1f) / horizontalStop);
			int newRows = (int)((height + 1f) / verticalStop);

			viewModel.ChangeSize(newCols, newRows);
		}

		public void this_Loaded(object sender, RoutedEventArgs e)
		{
			var content = terminalGrid;

			InvalidateMeasure();
			var contentDesired = terminalControl.TerminalSize;

			Width = contentDesired.Width + ActualWidth - content.ActualWidth;
			Height = contentDesired.Height + ActualHeight - content.ActualHeight;
		}


		public void OnKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (e.NewFocus == this)
			{
				terminalControl.Focus();
				Keyboard.Focus(terminalControl);
			}
		}
	}
}
