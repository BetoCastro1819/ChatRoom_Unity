using System.IO;

public struct ConnectionRequestPayload
{
	public ulong clientSalt;
}

public class ConnectionRequestPacket : NetworkPacket<ConnectionRequestPayload>
{
	public ConnectionRequestPacket() : base(PacketType.ConnectionRequest) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload.clientSalt);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload.clientSalt = binaryReader.ReadUInt64();
	}
}
