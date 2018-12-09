using Matt.Json;
using System.Collections.Generic;
using System.Text;

namespace Jake.RL
{
	using ArrayExtensions;
	using Serialization;

	public class SimpleDataHandler : DataHandler
	{
		public Data[] outgoingData;
		public Data[] incomingData;

		public override byte[] Serialize()
		{
			return Serializer.SerializeAll(
				outgoingData.Convert((d) => d.GetValue())
			);
		}

		public override int Serialize(ref byte[] buffer, int offset)
		{
			return Serializer.SerializeAll(
				outgoingData.Convert((d) => d.GetValue()), ref buffer, offset
			);
		}

		public override void Deserialize(byte[] bytes)
		{
			for (int i = 0; i < incomingData.Length; ++i)
			{
				var data = incomingData[i];
				var type = data.GetDataType();

				data.SetValue(
					type != null ? Serializer.Deserialize(type, bytes, i) : null
				);
			}
		}
	}
}
