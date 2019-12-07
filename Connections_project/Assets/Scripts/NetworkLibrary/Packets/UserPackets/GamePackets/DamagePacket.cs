using System.IO;

public struct DamagePacketData
{
	public uint damage;
}

public class DamagePacket : UserPacket<DamagePacketData>
{
	public DamagePacket() : base((ushort)UserPacketType.Damage) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload.damage);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload.damage = binaryReader.ReadUInt32();
	}
}
