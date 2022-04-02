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
using System.Windows.Input;
using System.Windows.Media;
using ScriptRemote.Core.Utils;

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

			// 初始化创建settings表
			SettingsUtil.OnCreate();
			// 初始化创建macros表
			MacrosUtil.OnCreate();

			// 读取数据
			settingsList.ItemsSource = SettingsUtil.List();
			settingsList.PreviewMouseMove += ListBox_PreviewMouseMove;

			Style itemContainerStyle = new Style(typeof(ListBoxItem));
			itemContainerStyle.Setters.Add(new Setter(AllowDropProperty, true));
			itemContainerStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonDown)));
			itemContainerStyle.Setters.Add(new EventSetter(DropEvent, new DragEventHandler(ListBoxItem_Drop)));
			// 铺满
			itemContainerStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
			itemContainerStyle.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
			settingsList.ItemContainerStyle = itemContainerStyle;
			// 默认选中第一个
			settingsList.SelectedIndex = 0;
			
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
				var lbi = ControlUtil.FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
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

		/// <summary>
		/// 连接
		/// </summary>
		/// <param name="settings"></param>
		private async void SelectedConnect(ConnectionSettings settings)
		{
			// 添加TabItem
			TabItem tabItem = new TabItem();
			tabItem.Header = settings.ConnectName;

			// 添加TabItem的内容
			TerminalTabControl terminalTab = new TerminalTabControl();
			tabItem.Content = terminalTab;
			// 设置选中
			tabItem.IsSelected = true;

			tabItem.IsVisibleChanged += terminalTab.this_IsVisibleChanged;
			tabItem.GotKeyboardFocus += terminalTab.OnKeyboardFocus;

			tabControl.SizeChanged += terminalTab.this_SizeChanged;
			//添加到TabControl
			tabControl.Items.Add(tabItem);

			try
			{
				// 执行的macro
				settings.settingMacros = MacrosUtil.List(settings.Id);

				// 连接
				Connection connection = await MakeConnectionAsync(settings, CommonConst.DefaultTerminalCols, CommonConst.DefaultTerminalRows);
				terminalTab.Connect(connection.Stream, settings);

				// 重置大小
				terminalTab.TerminalChangSize(tabControl.ActualWidth, tabControl.ActualHeight);
				tabItem.Focus();

			}
			catch (ConnectException ex)
			{
				App.Current.DisplayError(ex.Message, "Could not connect");
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


		private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (e.Timestamp - lastClickTimestamp < 500)
			{
				// 鼠标左键双击
				TextBlock textBlock = sender as TextBlock;
				ConnectionSettings selectedSettings = textBlock.Tag as ConnectionSettings;
				// 连接
				SelectedConnect(selectedSettings);
			}

			lastClickTimestamp = e.Timestamp;
		}

		private void settingsListItem_Connect(object sender, RoutedEventArgs e)
		{
			ConnectionSettings settings = (sender as MenuItem).Tag as ConnectionSettings;
			// 连接
			SelectedConnect(settings);
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
			// 屏幕整体宽度
			double x = SystemParameters.PrimaryScreenWidth;
			// 屏幕整体高度
			double y = SystemParameters.PrimaryScreenHeight;

			Width = x * 0.8;
			Height = y * 0.8;

			Left = 100;
			Top = 100;
		}
    }
}
