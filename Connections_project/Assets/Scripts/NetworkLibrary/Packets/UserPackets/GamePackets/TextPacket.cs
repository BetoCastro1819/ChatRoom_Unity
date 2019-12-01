using System.IO;

public class TextPacket : UserPacket<string>
{
	public TextPacket() : base((ushort)UserPacketType.Text_Message) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload = binaryReader.ReadString();
	}
}

