﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Terminal
{
	public class TitleChangeEventArgs : EventArgs
	{
		public string Title
		{ get; }

		public TitleChangeEventArgs(string title)
		{
			Title = title;
		}
	}

	public interface ITerminalHandler
	{
		TerminalBase Terminal
		{ get; }

		TerminalFont DefaultFont
		{ get; set; }
	}
}
