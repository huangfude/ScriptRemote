﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Wpf
{
	internal static class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		[DllImport("user32.dll")]
		public static extern bool GetClientRect(IntPtr hwnd, out RECT rect);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
	}
}
