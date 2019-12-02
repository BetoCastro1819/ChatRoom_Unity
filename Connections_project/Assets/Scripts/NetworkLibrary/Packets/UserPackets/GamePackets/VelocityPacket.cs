using System.IO;
using UnityEngine;

public struct VelocityPacketData
{
	public uint sequence;
	public Vector3 velocity;
}

public class VelocityPacket : UserPacket<VelocityPacketData>
{
	public VelocityPacket() : base((ushort)UserPacketType.Velocity) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);

		binaryWriter.Write(payload.sequence);
		binaryWriter.Write(payload.velocity.x);
		binaryWriter.Write(payload.velocity.y);
		binaryWriter.Write(payload.velocity.z);
	}
	
	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		
		payload.sequence = binaryReader.ReadUInt32();
		payload.velocity.x = binaryReader.ReadSingle();
		payload.velocity.y = binaryReader.ReadSingle();
		payload.velocity.z = binaryReader.ReadSingle();
	}
}
