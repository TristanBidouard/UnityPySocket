using Matt.Json;
using System;
using UnityEngine;

namespace Jake.RL
{
	using Json;
	using Serialization;

	public abstract class DataHandler : MonoBehaviour, ISerializable, IJsonizable
	{
		public virtual byte[] Serialize()
		{
			throw new NotImplementedException();
		}

		public virtual int Serialize(ref byte[] bytes, int offset)
		{
			throw new NotImplementedException();
		}

		public virtual void Deserialize(byte[] bytes)
		{
			throw new NotImplementedException();
		}

		public virtual JSONObject Jsonize()
		{
			throw new NotImplementedException();
		}

		public virtual void Dejsonize(JSONObject json)
		{
			throw new NotImplementedException();
		}
	}
}
