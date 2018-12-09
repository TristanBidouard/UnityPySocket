using System;
using UnityEngine;

namespace Jake.Tcp
{
	public class TcpConnection : MonoBehaviour
	{
		public bool autoConnect;
		public bool isServer;
		public string ipAddress;
		public int port;
		public bool useJson;
		public string jsonFile;
		
		private TcpSocket socket;
		private bool connected;
		
		void Start()
		{
			if (autoConnect)
				Connect();
		}
		
		void OnApplicationQuit()
		{
			if (connected)
			{
				socket.Close();
			}
		}

		public byte[] SendBuffer
		{
			get
			{
				return socket.SendBuffer;
			}
		}

		public byte[] ReceiveBuffer
		{
			get
			{
				return socket.ReceiveBuffer;
			}
		}

		public void ResizeSendBuffer(float size)
		{
			socket.ResizeSendBuffer(size);
		}

		public void Connect()
		{
			Connect(null);
		}

		public void Connect(Action onStarted)
		{
			if (useJson)
				LoadJson();
			
			socket = isServer ? new TcpServer() : new TcpClient() as TcpSocket;
			socket.debug = print;
			socket.Connect(ipAddress, port, (() => { connected = true; }) + onStarted);
		}

		public void Send(int size, Action onSent)
		{
			socket.Send(size, onSent);
		}
		
		public void Receive(Action<byte[], int> onReceived)
		{
			socket.Receive(onReceived);
		}
		
		public void LoadJson()
		{
			var json = Project.LoadJson(jsonFile);

			ipAddress	= json["ip"].str;
			port		= Mathf.RoundToInt(json["port"].n);
		}
	}
}
