using System.IO;

public class PacketWithCrc : ISerialzablePacket
{
	public uint crc;
	public int byteLength;
	public byte[] data;

	public void Serialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(crc);
		binaryWriter.Write(byteLength);
		binaryWriter.Write(data);
	}

	public void Deserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		crc = binaryReader.ReadUInt32();
		byteLength = binaryReader.ReadInt32();
		data = binaryReader.ReadBytes(byteLength);
	}
}
