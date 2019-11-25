using System.IO;

public struct ChallengeRequestPayload
{
	public uint clientID;
	public ulong clientSalt;
	public ulong serverSalt;
}

public class ChallengeRequestPacket : NetworkPacket<ChallengeRequestPayload>
{
	public ChallengeRequestPacket() : base(PacketType.ChallengeRequest) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload.clientID);
		binaryWriter.Write(payload.clientSalt);
		binaryWriter.Write(payload.serverSalt);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload.clientID = binaryReader.ReadUInt32();
		payload.clientSalt = binaryReader.ReadUInt64();
		payload.serverSalt = binaryReader.ReadUInt64();
	}
}
