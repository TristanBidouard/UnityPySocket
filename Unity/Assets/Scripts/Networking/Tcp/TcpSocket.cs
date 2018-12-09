//#define TCPSOCKET_DEBUG

using System;
using System.Net.Sockets;

namespace Jake.Tcp
{
	public abstract class TcpSocket : ProtocolSocket
	{
		public Action<object> debug;

		private SendState		sendState		= new SendState		();
		private ReceiveState	receiveState	= new ReceiveState	();
		
		public override byte[] SendBuffer
		{
			get
			{
				return sendState.buffer;
			}
		}

		public override byte[] ReceiveBuffer
		{
			get
			{
				return receiveState.buffer;
			}
		}

		public override void ResizeSendBuffer(float size)
		{
			sendState.buffer = new byte[(int)(sendState.buffer.Length * size)];
#if TCPSOCKET_DEBUG
			debug("Send buffer resized to " + sendState.buffer.Length.ToString() + " bytes.");
#endif
		}

		public override void ResizeReceiveBuffer(float size)
		{
			receiveState.buffer = new byte[(int)(receiveState.buffer.Length * size)];
#if TCPSOCKET_DEBUG
			debug("Receive buffer resized to " + receiveState.buffer.Length.ToString() + " bytes.");
#endif
		}

		public override void Send(int size, Action onSent)
		{
			// prepend size
			var head = BitConverter.GetBytes(size);
			Buffer.BlockCopy(head, 0, sendState.buffer, 0, 4);

			// resete send state
			sendState.bufferSize	= 4 + size;
			sendState.numBytesSent	= 0;
			sendState.callback		= onSent;
#if TCPSOCKET_DEBUG
			debug("Sending " + sendState.bufferSize.ToString() + " bytes...");
#endif
			// send
			client.BeginSend(sendState.buffer, 0, sendState.bufferSize, SocketFlags.None, OnSent, null);
		}

		public override void Receive(Action<byte[], int> onReceived)
		{
			receiveState.bufferSize			= 0;
			receiveState.numBytesReceived	= 0;
			receiveState.callback			= onReceived;
#if TCPSOCKET_DEBUG
			debug("Receiving...");
#endif
			client.BeginReceive(receiveState.buffer, 0, 4, SocketFlags.None, OnReceived, null);
		}

		public override void Close()
		{
#if TCPSOCKET_DEBUG
			debug("Closing...");
#endif
			client.Close();
#if TCPSOCKET_DEBUG
			debug("Closed.");
#endif
		}

		private void OnSent(IAsyncResult result)
		{
			var fragmentSize = client.EndSend(result);

			// calculate number of bytes sent
			sendState.numBytesSent += fragmentSize;
#if TCPSOCKET_DEBUG
			debug(sendState.numBytesSent.ToString() + " out of " + sendState.bufferSize.ToString() + " bytes sent.");
#endif
			// if more bytes need to be sent
			if (sendState.numBytesSent < sendState.bufferSize)
			{
				// continue sending
				client.BeginSend(sendState.buffer, sendState.numBytesSent, sendState.bufferSize - sendState.numBytesSent, SocketFlags.None, OnSent, null);
			}
			else
			{
				// stop sending
				if (sendState.callback != null)
				{
					sendState.callback();
				}
			}
		}

		private void OnReceived(IAsyncResult result)
		{
			var fragmentSize = client.EndReceive(result);

			// first fragment
			if (receiveState.bufferSize == 0)
			{
				// resize buffer
				receiveState.bufferSize = BitConverter.ToInt32(receiveState.buffer, 0);
				while (receiveState.buffer.Length < receiveState.bufferSize)
				{
					ResizeReceiveBuffer(2);
				}
#if TCPSOCKET_DEBUG
				debug("Receiving " + receiveState.bufferSize.ToString() + " bytes...");
#endif
			}
			else
			{
				// acknowledge bytes received
				receiveState.numBytesReceived += fragmentSize;
#if TCPSOCKET_DEBUG
				debug(receiveState.numBytesReceived.ToString() + " out of " + receiveState.bufferSize.ToString() + " bytes received.");
#endif
			}

			// if more byets need to be received
			if (receiveState.numBytesReceived < receiveState.bufferSize)
			{
				// continue receiving
				client.BeginReceive(receiveState.buffer, receiveState.numBytesReceived, receiveState.bufferSize - receiveState.numBytesReceived, SocketFlags.None, OnReceived, null);
			}
			else
			{
				// stop receiving
				if (receiveState.callback != null)
				{
					receiveState.callback(receiveState.buffer, receiveState.bufferSize);
				}
			}
		}
	}
}
