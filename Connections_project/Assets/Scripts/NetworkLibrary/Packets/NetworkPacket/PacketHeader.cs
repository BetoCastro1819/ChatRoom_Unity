using System.IO;

public class PacketHeader : ISerialzablePacket
{
	public uint packetTypeID;

	public void Serialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(packetTypeID);
	}

	public void Deserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		packetTypeID = binaryReader.ReadUInt32();
	}
}
