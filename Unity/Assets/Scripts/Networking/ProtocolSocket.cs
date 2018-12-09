using System;
using System.Net.Sockets;

namespace Jake
{
	public abstract class ProtocolSocket
	{
		protected Socket client;

		public abstract byte[] SendBuffer		{ get; }
		public abstract byte[] ReceiveBuffer	{ get; }

		public abstract void ResizeSendBuffer		(float size);
		public abstract void ResizeReceiveBuffer	(float size);

		public abstract void Connect	(string ip, int port, Action onConnected);
		public abstract void Send		(int size, Action onSent);
		public abstract void Receive	(Action<byte[], int> onReceive);
		public abstract void Close		();
	}
}
