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
using ScriptRemote.Core.Utils;
using System.Collections.ObjectModel;

namespace ScriptRemote.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

		int lastClickTimestamp = 1000;

		private Point _dragStartPoint;

		public MainWindow()
        {
            InitializeComponent();

			// 判断数据库文件是否存在
			if (File.Exists(GlobalVariable.dbpath))
            {
				settingsList.ItemsSource = SettingsUtil.List();

				settingsList.PreviewMouseMove += ListBox_PreviewMouseMove;

				Style itemContainerStyle = new Style(typeof(ListBoxItem));
				itemContainerStyle.Setters.Add(new Setter(AllowDropProperty, true));
				itemContainerStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonDown)));
				itemContainerStyle.Setters.Add(new EventSetter(DropEvent, new DragEventHandler(ListBoxItem_Drop)));
				settingsList.ItemContainerStyle = itemContainerStyle;
				settingsList.SelectedIndex = 0;
			} 
			else
            {
				// 创建
				SettingsUtil.OnCreate();
			}
			
		}
		private T FindVisualParent<T>(DependencyObject child)
			where T : DependencyObject
		{
			var parentObject = VisualTreeHelper.GetParent(child);
			if (parentObject == null)
				return null;
			T parent = parentObject as T;
			if (parent != null)
				return parent;
			return FindVisualParent<T>(parentObject);
		}

		private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			Point point = e.GetPosition(null);
			Vector diff = _dragStartPoint - point;
			if (e.LeftButton == MouseButtonState.Pressed &&
				(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
			{
				var lb = sender as ListBox;
				var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
				if (lbi != null)
				{
					DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
				}
			}
		}
		private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_dragStartPoint = e.GetPosition(null);
		}

		private void ListBoxItem_Drop(object sender, DragEventArgs e)
		{
			if (sender is ListBoxItem)
			{
				var source = e.Data.GetData(typeof(ConnectionSettings)) as ConnectionSettings;
				var target = ((ListBoxItem)(sender)).DataContext as ConnectionSettings;

				//int sourceIndex = settingsList.Items.IndexOf(source);
				//int targetIndex = settingsList.Items.IndexOf(target);

				// 互换
				SettingsUtil.OnUpdateSort(source.Id, target.Sort);
				SettingsUtil.OnUpdateSort(target.Id, source.Sort);

				settingsList.ItemsSource = SettingsUtil.List();
			}
		}


		internal async Task<Connection> MakeConnectionAsync(ConnectionSettings settings, int terminalCols, int terminalRows)
		{
			using (var doneEvent = new System.Threading.ManualResetEvent(false))
			{
				var connection = new Connection();
				string error = null;
				connection.Connected += (_sender, _e) =>
				{
					Dispatcher.Invoke(() =>
					{
						doneEvent.Set();
					});
				};
				connection.Failed += (_sender, _e) =>
				{
					Dispatcher.Invoke(() =>
					{
						error = _e.Message;
						doneEvent.Set();
					});
				};

				connection.Connect(settings, terminalCols, terminalRows);

				// 等待执行事件
				await Task.Run(new Action(() => doneEvent.WaitOne()));
				if (error != null)
					throw new ConnectException(error);
				return connection;
			}
		}

		private void settingsListItem_Edit(object sender, RoutedEventArgs e)
		{
			ConnectionSettings settings = (sender as MenuItem).Tag as ConnectionSettings;
			var dialog = new ConnectionDialog();
			dialog.SettingsEdit(settings);
			dialog.ShowDialog();
		}

		private void settingsListItem_Delete(object sender, RoutedEventArgs e)
		{
			ConnectionSettings settings = (sender as MenuItem).Tag as ConnectionSettings;
			SettingsUtil.OnDelete(settings.Id);
			settingsList.ItemsSource = SettingsUtil.List();
		}

		

		private async void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (e.Timestamp - lastClickTimestamp < 500)
			{
				// 鼠标左键双击
				TextBlock textBlock = sender as TextBlock;

				ConnectionSettings SelectedSettings = textBlock.Tag as ConnectionSettings;

				try
				{
					Connection connection = await MakeConnectionAsync(SelectedSettings, CommonConst.DefaultTerminalCols, CommonConst.DefaultTerminalRows);
					
					// 添加TabItem
					TabItem tabItem = new TabItem();
					tabItem.Header = textBlock.Text;

					// 添加TabItem的内容
					TerminalTabControl terminalTab = new TerminalTabControl();
					terminalTab.Height = tabControl.ActualHeight;
					terminalTab.Width = tabControl.ActualWidth;
					// 连接
					terminalTab.Connect(connection.Stream, connection.Settings);

					tabItem.Content = terminalTab;
					// 设置选中
					tabItem.IsSelected = true;
					tabItem.IsVisibleChanged += terminalTab.this_IsVisibleChanged;

					tabControl.SizeChanged += terminalTab.this_SizeChanged;

					//添加到TabControl
					tabControl.Items.Add(tabItem);
				}
				catch (ConnectException ex)
				{
					App.Current.DisplayError(ex.Message, "Could not connect");
				}
			}

			lastClickTimestamp = e.Timestamp;
		}

		private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
			App.Current.Shutdown();
        }

        private void MenuItem_Click_New(object sender, RoutedEventArgs e)
        {
			var dialog = new ConnectionDialog();
			dialog.ShowDialog();
		}

        
    }
}
