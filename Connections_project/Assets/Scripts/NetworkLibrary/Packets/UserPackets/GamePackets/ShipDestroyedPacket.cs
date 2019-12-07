using System.IO;

public struct ShipDestroyedPacketData { }

public class ShipDestroyedPacket : UserPacket<ShipDestroyedPacket>
{
	public ShipDestroyedPacket() : base((ushort)UserPacketType.ShipDestroyed) { }

	protected override void OnSerialize(Stream stream) { }
	protected override void OnDeserialize(Stream stream) { }
}
