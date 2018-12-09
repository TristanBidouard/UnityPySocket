//#define CYCLE_DEBUG
using UnityEngine;

namespace Jake.RL
{
	using Serialization;
	using Tcp;
	using Threading;

	public class Cycle : MonoBehaviour
	{
		public TcpConnection	tcp;
		public DataHandler		dataHandler;

		private bool wait;

		public bool Wait
		{
			get
			{
				return wait;
			}

			set
			{
				wait = value;
				if (!wait)
				{
					Next();
				}
			}
		}

#if CYCLE_DEBUG
		private int cycleCount;
		private float startTime;
#endif

#if CYCLE_DEBUG
		void OnGUI()
		{
			if (cycleCount > 0)
			{
				var runtime = Time.time - startTime;
				GUILayout.Label("Cycles: " + cycleCount + "c");
				GUILayout.Label("Time: " + runtime + "s");
				GUILayout.Label("Cycles per second: " + (cycleCount / runtime) + "s");
				GUILayout.Label("Seconds per cycle: " + (runtime / cycleCount) + "c");
			}
		}
#endif
		/// <summary>
		/// Begin cycle
		/// </summary>
		public void Begin()
		{
#if CYCLE_DEBUG
			cycleCount = 0;
			startTime = Time.time;
#endif
			Next();
		}
		
		private void Next()
		{
#if CYCLE_DEBUG
			cycleCount++;
#endif
			// serialize
			var numBytes = 0;
			var serialized = false;
			while (!serialized)
			{
				try
				{
					var sendBuffer = tcp.SendBuffer;
					numBytes = dataHandler.Serialize(ref sendBuffer, 4);
					serialized = true;
				}
				catch (SerializerException)
				{
					tcp.ResizeSendBuffer(2);
				}
			}
			
			// send
			tcp.Send(numBytes, OnSent);
		}
		
		private void OnSent()
		{
			// receive
			Dispatcher.AddJob(() =>
			{
				tcp.Receive(OnReceived);
			}, 
			false);
		}

		private void OnReceived(byte[] buffer, int numBytes)
		{
			Dispatcher.AddJob(() =>
			{
				// deserialize
				dataHandler.Deserialize(tcp.ReceiveBuffer);

				// recurse
				if (!Wait)
					Next();
			}, 
			false);
		}
	}
}
