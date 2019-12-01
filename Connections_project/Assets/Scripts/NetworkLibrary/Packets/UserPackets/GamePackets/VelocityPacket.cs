using System.IO;
using UnityEngine;

public class VelocityPacket : NetworkPacket<Vector3>
{
	public VelocityPacket() : base(PacketType.User) { }

	protected override void OnSerialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(payload.x);
		binaryWriter.Write(payload.y);
		binaryWriter.Write(payload.z);
	}
	
	protected override void OnDeserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		payload.x = binaryReader.ReadSingle();
		payload.y = binaryReader.ReadSingle();
		payload.z = binaryReader.ReadSingle();
	}
}
