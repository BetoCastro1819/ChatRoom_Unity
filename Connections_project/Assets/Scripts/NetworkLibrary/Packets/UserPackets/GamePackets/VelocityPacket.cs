using System.IO;
using UnityEngine;

//public struct VelocityPacketData
//{
//	public ushort enitityID;
//	public Vector3 velocity;
//}

public class VelocityPacket : UserPacket<Vector3>
{
	public VelocityPacket() : base((ushort)UserPacketType.Velocity) { }

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
