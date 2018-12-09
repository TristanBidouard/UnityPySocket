//#define TCPCLIENT_DEBUG

using System;
using System.Net;
using System.Net.Sockets;

namespace Jake.Tcp
{
	public class TcpClient : TcpSocket
	{
		public override void Connect(string ip, int port, Action onConnected)
		{
#if TCPCLIENT_DEBUG
			debug("Connecting...");
#endif
			client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var endPoint = new IPEndPoint(
				ip == "loopback" ? IPAddress.Loopback : IPAddress.Parse(ip), 
				port
			);

			client.BeginConnect(endPoint, OnConnected, new ConnectState() { callback = onConnected });
		}
		
		void OnConnected(IAsyncResult result)
		{
			client.EndConnect(result);

#if TCPCLIENT_DEBUG
			debug("Connected.");
#endif

			var callback = result.AsyncState as Action;
			if (callback != null)
			{
				callback();
			}
		}
	}
}
