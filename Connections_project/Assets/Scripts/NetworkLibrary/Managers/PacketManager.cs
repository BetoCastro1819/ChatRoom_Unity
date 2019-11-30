using System.Collections.Generic;
using System.IO;
using System.Net;
using System;

public class PacketManager : MonoBehaviourSingleton<PacketManager>, IReceiveData
{
	Dictionary<uint, Action<uint, ushort, Stream>> onPacketReceived = new Dictionary<uint, Action<uint, ushort, Stream>>();
	Crc32 crc32 = new Crc32();

	uint localSequence = 0;
	uint remoteSequence = 0;
	uint ack = 0;
	uint ackBitfields = 0;


	protected override void Initialize()
	{
		NetworkManager.Instance.OnReceiveEvent += OnReceiveData;
	}

	public void AddListener(uint listenerID, Action<uint, ushort, Stream> callback)
	{
		if (!onPacketReceived.ContainsKey(listenerID))
			onPacketReceived.Add(listenerID, callback);
	}

	public void RemoveListener(uint listenerID)
	{
		if (onPacketReceived.ContainsKey(listenerID))
			onPacketReceived.Remove(listenerID);
	}

	public void SendReliablePacket<T>(NetworkPacket<T> packetToSend, uint objectID = 0)
	{
		byte[] packetData = SerializePacket(packetToSend, true, objectID);

		if (NetworkManager.Instance.isServer)
			NetworkManager.Instance.Broadcast(packetData);
		else
			NetworkManager.Instance.SendToServer(packetData);
	}

	public void SendPacket<T>(NetworkPacket<T> packetToSend, uint objectID = 0)
	{
		byte[] packetData = SerializePacket(packetToSend, false, objectID);

		if (NetworkManager.Instance.isServer)
			NetworkManager.Instance.Broadcast(packetData);
		else
			NetworkManager.Instance.SendToServer(packetData);
	}

	public void SendPacketToClient<T>(NetworkPacket<T> packetToSend, IPEndPoint iPEndPoint, bool isReliable = false)
	{
		byte[] packetData = SerializePacket(packetToSend, isReliable);
		NetworkManager.Instance.SendToClient(packetData, iPEndPoint);
	}

	public byte[] SerializePacket<T>(NetworkPacket<T> packetToSerialize, bool isReliable = false, uint objectID = 0)
	{
		MemoryStream memoryStream = new MemoryStream();
		PacketHeader packetHeader = new PacketHeader();

		packetHeader.packetTypeID = (uint)packetToSerialize.type;
		packetHeader.Serialize(memoryStream);

		if (packetToSerialize.type == PacketType.User)
		{
			UserPacketHeader userPacketHeader = new UserPacketHeader();

			userPacketHeader.packetTypeID = packetToSerialize.userPacketTypeID;
			userPacketHeader.packetID = localSequence++;
			userPacketHeader.senderID = NetworkManager.Instance.clientID;
			userPacketHeader.objectID = objectID;
			userPacketHeader.isReliable = isReliable;

			userPacketHeader.Serialize(memoryStream);

			if (isReliable)
			{
				ReliablePacketHeader reliablePacketHeader = new ReliablePacketHeader();

				reliablePacketHeader.sequence = localSequence;
				reliablePacketHeader.ack = ack;
				reliablePacketHeader.ackBitfield = ackBitfields;

				reliablePacketHeader.Serialize(memoryStream);
			}
		}
		packetToSerialize.Serialize(memoryStream);

		PacketWithCrc packetWithCrc = new PacketWithCrc();
		packetWithCrc.crc = crc32.CalculateCrc(memoryStream.ToArray());
		packetWithCrc.byteLength = memoryStream.ToArray().Length;
		packetWithCrc.data = memoryStream.ToArray();	
		memoryStream.Close();

		memoryStream = new MemoryStream();
		packetWithCrc.Serialize(memoryStream);
		memoryStream.Close();

		return memoryStream.ToArray();
	}

	public void OnReceiveData(byte[] data, IPEndPoint iPEndPoint)
	{
		MemoryStream memoryStream = new MemoryStream(data);
		PacketWithCrc packetWithCrc = new PacketWithCrc();
		packetWithCrc.Deserialize(memoryStream);
		memoryStream.Close();

		if (crc32.IsDataCorrupted(packetWithCrc.data, packetWithCrc.crc))
		{
			UnityEngine.Debug.LogError("Received corrupted data from " + iPEndPoint);
			return;
		} 
		memoryStream = new MemoryStream(packetWithCrc.data);
		PacketHeader packetHeader = new PacketHeader();
		packetHeader.Deserialize(memoryStream);

		if ((PacketType)packetHeader.packetTypeID == PacketType.User)
		{
			UserPacketHeader userPacketHeader = new UserPacketHeader();
			userPacketHeader.Deserialize(memoryStream);

			if (userPacketHeader.isReliable)
			{
				ReliablePacketHeader reliablePacketHeader = new ReliablePacketHeader();
				reliablePacketHeader.Deserialize(memoryStream);
			}	

			InvokeCallback(userPacketHeader, memoryStream);
		}
		else
		{
			NetworkManager.Instance.OnReceivePacket(iPEndPoint, (PacketType)packetHeader.packetTypeID, memoryStream);
		}
		memoryStream.Close();
	}

	public void InvokeCallback(UserPacketHeader packetHeader, Stream stream)
	{
		if (onPacketReceived.ContainsKey(packetHeader.objectID))
		{
			onPacketReceived[packetHeader.objectID].Invoke(
				packetHeader.packetID, 
				packetHeader.packetTypeID, 
				stream
			);
		}
	}
}
