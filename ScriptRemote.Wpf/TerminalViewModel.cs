
using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using ScriptRemote.Core.Terminal;
using ScriptRemote.Terminal.Controls;
using Point = ScriptRemote.Core.Terminal.Point;
using ScriptRemote.Core.Common;

namespace ScriptRemote.Wpf
{
	public class ErrorEventArgs : EventArgs
	{
		public string Message
		{ get; }

		public ErrorEventArgs(string message)
		{
			Message = message;
		}
	}

	interface IWindowStreamNotifier : IStreamNotifier
	{
		void Start();
		void Stop();
	}

	class ShellStreamNotifier : IWindowStreamNotifier
	{
		readonly TerminalControl terminal;
		readonly ShellStream stream;
		readonly SemaphoreSlim received = new SemaphoreSlim(0);
		bool stopped;

		public Stream Stream
		{ get { return stream; } }

		public event EventHandler<DataAvailableEventArgs> DataAvailable;

		public ShellStreamNotifier(TerminalControl terminal, ShellStream stream)
		{
			this.terminal = terminal;
			this.stream = stream;
		}

		public void Start()
		{
			stopped = false;
			stream.DataReceived += Stream_DataReceived;
			Task.Run(() =>
			{
				while (true)
				{
					received.Wait();
					if (stopped)
						break;

					while (stream.DataAvailable)
					{
						terminal.BeginChange();
						if (DataAvailable != null)
						{
							bool done = false;
							Task.Run(() =>
							{
								while (!done)
								{
									Thread.Sleep(50);
									if (done)
										break;
									terminal.CycleChange();
								}
							});
							DataAvailable(this, new DataAvailableEventArgs(0));
							done = true;
						}
						else
							throw new InvalidOperationException("Data was received but no one was listening.");
						terminal.EndChange();
					}
				}
			});
			received.Release();
		}

		public void Stop()
		{
			stopped = true;
			stream.Close();
			received.Release();
		}

		private void Stream_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
		{
			received.Release();
		}
	}

	class SavingShellStreamNotifier : IWindowStreamNotifier
	{
		readonly TerminalControl terminal;
		readonly ShellStream input;
		readonly Stream output;
		readonly ProxyStream middle;
		readonly ConcurrentQueue<byte[]> outputQueue = new ConcurrentQueue<byte[]>();
		readonly EventWaitHandle outputWait = new EventWaitHandle(false, EventResetMode.AutoReset);
		readonly Task outputTask;

		public Stream Stream
		{ get { return middle; } }

		public event EventHandler<DataAvailableEventArgs> DataAvailable;

		public SavingShellStreamNotifier(TerminalControl terminal, ShellStream stream, Stream output)
		{
			this.terminal = terminal;
			input = stream;
			this.output = output;
			middle = new ProxyStream(input);
			stream.DataReceived += Stream_DataReceived;
			outputTask = Task.Run(() =>
			{
				do
				{
					outputWait.WaitOne();
					if (outputQueue.IsEmpty)
						break;

					byte[] result;
					while (!outputQueue.TryDequeue(out result)) ;

					output.Write(result, 0, result.Length);
				} while (true);
			});
		}

		public void Start()
		{
			if (input.DataAvailable && DataAvailable != null)
				Stream_DataReceived(this, null);
		}

		public void Stop()
		{
			output.Close();
		}

		private void Stream_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
		{
			terminal.BeginChange();
			if (DataAvailable != null)
			{
				DataAvailable(this, new DataAvailableEventArgs(e.Data.Length));
				outputQueue.Enqueue(middle.GetCopy());
				outputWait.Set();
			}
			else
				throw new InvalidOperationException("Data was received but no one was listening.");
			terminal.EndChange();
		}
	}

	class LoadingShellStreamNotifier : IWindowStreamNotifier
	{
		TerminalControl terminal;

		public Stream Stream
		{ get; }

		public event EventHandler<DataAvailableEventArgs> DataAvailable;

		public LoadingShellStreamNotifier(TerminalControl terminal, Stream input)
		{
			this.terminal = terminal;
			Stream = new ProxyStream(input, false, true);
		}

		public void Start()
		{
			if (DataAvailable != null)
			{
				Stream_DataReceived(this, null);
			}
		}

		public void Stop()
		{
			Stream.Close();
		}

		private void Stream_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
		{
			terminal.BeginChange();
			if (DataAvailable != null)
				DataAvailable(this, new DataAvailableEventArgs(e.Data.Length));
			else
				throw new InvalidOperationException("Data was received but no one was listening.");
			terminal.EndChange();
		}
	}

	class TerminalViewModel : INotifyPropertyChanged
	{
		ConnectionSettings settings;
		ShellStream stream;

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<ErrorEventArgs> Error;

		protected void notifyPropertyChanged([CallerMemberName] string memberName = null)
		{
			App.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
			}));
		}

		string title;
		public string Title
		{
			get { return title; }
			set
			{
				title = value;
				notifyPropertyChanged();
			}
		}

		XtermTerminal terminal;
		public XtermTerminal Terminal
		{
			get { return terminal; }
			private set
			{
				terminal = value;
				notifyPropertyChanged();
			}
		}


		public void Connect(IStreamNotifier notifier, ShellStream stream, ConnectionSettings settings)
		{
			this.settings = settings;
			this.stream = stream;
			var terminal = new XtermTerminal(notifier)
			{
				Size = new Point(CommonConst.DefaultTerminalCols, CommonConst.DefaultTerminalRows),
				DefaultFont = new TerminalFont()
				{
					Foreground = TerminalColors.GetBasicColor(7)
				}
			};
			terminal.StreamException += Terminal_StreamException;
			// 监听变更名称事件
			terminal.TitleChanged += (sender, e) =>
			{
				Title = e.Title;
			};
			// 监听文本变更事件
			terminal.TextChanged += (sender, e) =>
			{
				// 去掉空格符
				string text = e.Title.Trim();
				if (!string.IsNullOrEmpty(text))
                {
					foreach (SettingMacros macro in settings.settingMacros)
                    {
						// 执行一次去除一个
						string pattern = macro.Pattern;
                        if (!string.IsNullOrEmpty(pattern) && text.IndexOf(pattern)>-1 && macro.exec)
                        {
							System.Diagnostics.Debug.WriteLine("command: " + macro.Command);
							stream.WriteLine(macro.Command);
							// 执行一次
							macro.exec = false;
						}
					}
					
				}
			};
			

			Terminal = terminal;
		}

		private void Terminal_StreamException(object sender, StreamExceptionEventArgs e)
		{
			string message;
			var ex1 = (e.Exception as Renci.SshNet.Common.SshConnectionException);
			var ex2 = (e.Exception as IOException);
			var ex3 = (e.Exception as System.Net.Sockets.SocketException);
			if (ex1 != null || ex3 != null)
				message = string.Format("Connection to the server has been lost: {0}", e.Exception.Message);
			else if (ex2 != null)
				message = string.Format("An error occurred reading from the server: {0}", e.Exception.Message);
			else
				throw new Exception("An unidentified error occurred.", e.Exception);

			Error?.Invoke(this, new ErrorEventArgs(message));
		}

		public void ChangeSize(int cols, int rows)
		{
			if(terminal != null)
            {
				if (cols != terminal.Size.Col || rows != terminal.Size.Row)
				{
					terminal.Size = new Point(cols, rows);
					//stream.Channel.SendWindowChangeRequest((uint) cols, (uint) rows, 0, 0);
				}
			}
		}
	}
}
