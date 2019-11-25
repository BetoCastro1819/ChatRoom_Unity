using System.IO;

public struct UserPacketPayloadData { }

public class UserPacket : NetworkPacket<UserPacketPayloadData>
{
	public UserPacket(ushort userPacketTypeID) : base(PacketType.User)
	{
		this.userPacketTypeID = userPacketTypeID;
	}

	protected override void OnSerialize(Stream stream) { }
	protected override void OnDeserialize(Stream stream) { }
}