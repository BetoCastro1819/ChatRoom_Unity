using System.IO;

public class ReliablePacketHeader : ISerialzablePacket
{
	public uint sequence;
	public uint ack;
	public int ackBitfield;

	public void Serialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(sequence);
		binaryWriter.Write(ack);
		binaryWriter.Write(ackBitfield);
	}

	public void Deserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		sequence = binaryReader.ReadUInt32();
		ack = binaryReader.ReadUInt32();
		ackBitfield = binaryReader.ReadInt32();
	}
}
