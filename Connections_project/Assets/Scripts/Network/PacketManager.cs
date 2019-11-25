using System.Collections.Generic;
using System.IO;
using System.Net;
using System;
using UnityEngine;

public class PacketManager : MonoBehaviourSingleton<PacketManager>, IReceiveData
{
	Dictionary<uint, Action<uint, ushort, Stream>> onPacketReceived = new Dictionary<uint, Action<uint, ushort, Stream>>();
	uint currentPacketID = 0;

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

	public void SendPacket<T>(NetworkPacket<T> packetToSend, uint objectID = 0)
	{
		Debug.Log(gameObject + "Sending packet");

		byte[] packetData = SerializePacket(packetToSend, objectID);

		if (NetworkManager.Instance.isServer)
			NetworkManager.Instance.Broadcast(packetData);
		else
			NetworkManager.Instance.SendToServer(packetData);
	}

	public void SendPacket<T>(NetworkPacket<T> packetToSend, IPEndPoint iPEndPoint)
	{
		byte[] packetData = SerializePacket(packetToSend);
		NetworkManager.Instance.SendToClient(packetData, iPEndPoint);
	}

	public byte[] SerializePacket<T>(NetworkPacket<T> packetToSerialize, uint objectID = 0) // ReliablePacketHeader packetHeader = null
	{
		MemoryStream memoryStream = new MemoryStream();
		PacketHeader packetHeader = new PacketHeader();

		packetHeader.packetTypeID = (uint)packetToSerialize.type;
		packetHeader.Serialize(memoryStream);

		if (packetToSerialize.type == PacketType.User)
		{
			UserPacketHeader userPacketHeader = new UserPacketHeader();

			userPacketHeader.packetTypeID = packetToSerialize.userPacketTypeID;
			userPacketHeader.packetID = currentPacketID++;
			userPacketHeader.senderID = NetworkManager.Instance.clientID;
			userPacketHeader.objectID = objectID;

			userPacketHeader.Serialize(memoryStream);
		}

		packetToSerialize.Serialize(memoryStream);

		memoryStream.Close();

		return memoryStream.ToArray();
	}

	public void OnReceiveData(byte[] data, IPEndPoint iPEndPoint)
	{
		PacketHeader packetHeader = new PacketHeader();
		MemoryStream memoryStream = new MemoryStream(data);

		packetHeader.Deserialize(memoryStream);

		if ((PacketType)packetHeader.packetTypeID == PacketType.User)
		{
			UserPacketHeader userPacketHeader = new UserPacketHeader();
			userPacketHeader.Deserialize(memoryStream);
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
		Debug.Log("InvokeCallback");
		
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
