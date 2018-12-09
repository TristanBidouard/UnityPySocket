using System;

namespace Jake.Tcp
{
	public class SendState
	{
		public byte[]	buffer;
		public int		bufferSize;
		public int		numBytesSent;
		public Action	callback;

		public SendState()
		{
			buffer = new byte[1024];
		}
	}
}
