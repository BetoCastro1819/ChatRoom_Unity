using System.IO;

public enum UserPacketType
{
	Text_Message
}

public class UserPacketHeader : ISerialzablePacket
{
	public ushort packetTypeID;
	public uint packetID;
	public uint senderID;
	public uint objectID;

	public void Serialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);

		binaryWriter.Write(packetTypeID);
		binaryWriter.Write(packetID);
		binaryWriter.Write(senderID);
		binaryWriter.Write(objectID);
	}

	public void Deserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);

		packetTypeID = binaryReader.ReadUInt16();
		packetID = binaryReader.ReadUInt32();
		senderID = binaryReader.ReadUInt32();
		objectID = binaryReader.ReadUInt32();
	}
}

public struct UserPacketPayloadData 
{

}

public class UserPacket : NetworkPacket<UserPacketPayloadData>
{
	public UserPacket(ushort userPacketTypeID) : base(PacketType.User)
	{
		this.userPacketTypeID = userPacketTypeID;
	}

	protected override void OnSerialize(Stream stream) { }
	protected override void OnDeserialize(Stream stream) { }
}
