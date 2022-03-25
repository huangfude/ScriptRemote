using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Terminal
{
	public class DataAvailableEventArgs : EventArgs
	{
		public int ChunkSize
		{ get; }

		public DataAvailableEventArgs(int chunkSize)
		{
			ChunkSize = chunkSize;
		}
	}

	public interface IStreamNotifier
	{
		Stream Stream
		{ get; }

		event EventHandler<DataAvailableEventArgs> DataAvailable;
	}
}
