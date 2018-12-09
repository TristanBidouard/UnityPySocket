namespace Jake.Serialization
{
	public interface ISerializable
	{
		byte[] Serialize();
		void Deserialize(byte[] bytes);
	}
}
