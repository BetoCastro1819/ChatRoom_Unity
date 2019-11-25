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

public class PacketHeader : ISerialzablePacket
{
	public uint packetTypeID;

	public void Serialize(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(packetTypeID);
	}

	public void Deserialize(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		packetTypeID = binaryReader.ReadUInt32();
	}
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