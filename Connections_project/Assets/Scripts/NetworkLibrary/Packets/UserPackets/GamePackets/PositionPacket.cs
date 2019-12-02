using System.IO;
using UnityEngine;

public struct PositionPacketData
{
	public uint sequence;
	public Vector3 position;
}

public class PositionPacket : UserPacket<PositionPacketData>
{
	public PositionPacket() : base((ushort)UserPacketType.Position) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);

		binaryWriter.Write(payload.sequence);
		binaryWriter.Write(payload.position.x);
		binaryWriter.Write(payload.position.y);
		binaryWriter.Write(payload.position.z);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);

		payload.sequence = binaryReader.ReadUInt32();
		payload.position.x = binaryReader.ReadSingle();
		payload.position.y = binaryReader.ReadSingle();
		payload.position.z = binaryReader.ReadSingle();
	}
}
