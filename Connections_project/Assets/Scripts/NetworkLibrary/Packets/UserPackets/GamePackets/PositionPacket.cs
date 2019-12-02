using System.IO;
using UnityEngine;

public class PositionPacket : UserPacket<Vector3>
{
	public PositionPacket() : base((ushort)UserPacketType.Position) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);

		//binaryWriter.Write(payload.enitityID);
		binaryWriter.Write(payload.x);
		binaryWriter.Write(payload.y);
		binaryWriter.Write(payload.z);
	}

	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);

		//payload.enitityID = binaryReader.ReadUInt16();
		payload.x = binaryReader.ReadSingle();
		payload.y = binaryReader.ReadSingle();
		payload.z = binaryReader.ReadSingle();
	}
}
