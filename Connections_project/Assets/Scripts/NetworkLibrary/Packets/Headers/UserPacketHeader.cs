using System.IO;

public enum UserPacketType
{
	Text_Message,
	Velocity
}

public class UserPacketHeader : ISerialzablePacket
{
	public ushort packetTypeID;
	public uint packetID;
	public uint senderID;
	public uint objectID;
	public bool isReliable;

	public void Serialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);

		binaryWriter.Write(packetTypeID);
		binaryWriter.Write(packetID);
		binaryWriter.Write(senderID);
		binaryWriter.Write(objectID);
		binaryWriter.Write(isReliable);
	}

	public void Deserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);

		packetTypeID = binaryReader.ReadUInt16();
		packetID = binaryReader.ReadUInt32();
		senderID = binaryReader.ReadUInt32();
		objectID = binaryReader.ReadUInt32();
		isReliable = binaryReader.ReadBoolean();
	}
}