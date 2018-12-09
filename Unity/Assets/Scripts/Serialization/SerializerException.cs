using System;
using UnityEngine;

namespace Jake.Serialization
{
	public class SerializerException : Exception
	{
		public SerializerException(string message, bool logError)
		{
			if (logError)
				Debug.LogError("SerializerException: " + message);
		}
	}
}
