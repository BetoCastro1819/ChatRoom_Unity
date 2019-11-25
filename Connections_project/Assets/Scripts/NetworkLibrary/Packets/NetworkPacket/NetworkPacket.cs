using System.Net;
using System.IO;

public enum PacketType
{
	ConnectionRequest,
	ChallengeRequest,
	ChallengeResponse,
	ConnectionAccepted,
	User
}

public abstract class NetworkPacket<T> : ISerialzablePacket
{
    public PacketType type;
	public ushort userPacketTypeID;
	public T payload;

	public NetworkPacket(PacketType type)
	{
		this.type = type;
	}

	public virtual void Serialize(Stream stream)
	{
		OnSerialize(stream);
	}

	public virtual void Deserialize(Stream stream)
	{
		OnDeserialize(stream);
	}

	protected abstract void OnSerialize(Stream stream);
	protected abstract void OnDeserialize(Stream stream);
}