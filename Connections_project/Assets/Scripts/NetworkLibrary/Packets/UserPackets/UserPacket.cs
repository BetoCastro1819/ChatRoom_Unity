using System.IO;

public class UserPacket<T> : NetworkPacket<T>
{
	public UserPacket(ushort userPacketTypeID) : base(PacketType.User)
	{
		this.userPacketTypeID = userPacketTypeID;
	}

	protected override void OnSerialize(Stream stream) { }
	protected override void OnDeserialize(Stream stream) { }
}