using System.IO;

public struct ConnectionAcceptedPayload { }

public class ConnectionAcceptedPacket : NetworkPacket<ConnectionAcceptedPayload>
{
	public ConnectionAcceptedPacket() : base(PacketType.ConnectionAccepted) { }
	protected override void OnSerialize(Stream stream) { }
	protected override void OnDeserialize(Stream stream) { }
}
