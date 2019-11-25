using System.IO;

public struct ChallengeResponsePayload
{
	public ulong result;
}

public class ChallengeResponsePacket : NetworkPacket<ChallengeResponsePayload>
{
	public ChallengeResponsePacket() : base(PacketType.ChallengeResponse) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload.result);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload.result = binaryReader.ReadUInt64();
	}
}
