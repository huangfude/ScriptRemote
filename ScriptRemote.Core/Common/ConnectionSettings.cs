using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRemote.Core.Common
{
	public class ConnectionSettings
	{
		public string Index
		{ get; set; }

		public string ConnectName
		{ get; set; }

		public string ServerAddress
		{ get; set; }

		public int ServerPort
		{ get; set; }

		public string Username
		{ get; set; }

		public string Password
		{ get; set; }

		public string KeyFilePath
		{ get; set; }

		public string KeyFilePassphrase
		{ get; set; }
	}


	[Serializable]
	public class ConnectException : Exception
	{
		public ConnectException() { }
		public ConnectException(string message) : base(message) { }
		public ConnectException(string message, Exception inner) : base(message, inner) { }
		protected ConnectException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}

}
