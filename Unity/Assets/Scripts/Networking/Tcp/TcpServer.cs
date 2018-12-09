//#define TCPSERVER_DEBUG

using System;
using System.Net;
using System.Net.Sockets;

namespace Jake.Tcp
{
	public class TcpServer : TcpSocket
	{
		private Socket server;

		public override void Connect(string ip, int port, Action onConnected)
		{
#if TCPSERVER_DEBUG
			debug("Connecting...");
#endif

			var ipAddress = default(IPAddress);
			if		(ip == "any")		ipAddress = IPAddress.Any;
			else if (ip == "loopback")	ipAddress = IPAddress.Loopback;
			else						ipAddress = IPAddress.Parse(ip);

			server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(new IPEndPoint(ipAddress, port));
			server.Listen(1);

			server.BeginAccept(OnConnected, new ConnectState() { callback = onConnected });
		}
		
		void OnConnected(IAsyncResult result)
		{
			client = server.EndAccept(result);

#if TCPSERVER_DEBUG
			debug("Connected.");
#endif

			var state = result.AsyncState as ConnectState;
			if (state.callback != null)
			{
				state.callback();
			}
		}
	}
}
