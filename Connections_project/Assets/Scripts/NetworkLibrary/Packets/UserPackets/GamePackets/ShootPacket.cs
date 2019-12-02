using System.IO;

public class ShootPacket : UserPacket<int>
{
	public ShootPacket() : base((ushort)UserPacketType.Shoot) { }

	protected override void OnSerialize(Stream stream) { }
	protected override void OnDeserialize(Stream stream) { }
}