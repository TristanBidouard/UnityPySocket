using System;

namespace Jake.Tcp
{
	public class ReceiveState
	{
		public byte[]				buffer;
		public int					bufferSize;
		public int					numBytesReceived;
		public Action<byte[], int>	callback;

		public ReceiveState()
		{
			buffer = new byte[1024];
		}
	}
}
