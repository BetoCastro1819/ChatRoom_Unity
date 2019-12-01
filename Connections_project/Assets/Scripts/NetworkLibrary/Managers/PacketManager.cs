using System.Collections.Generic;
using System.IO;
using System.Net;
using System;

public class PacketManager : MonoBehaviourSingleton<PacketManager>, IReceiveData
{
	Dictionary<uint, Action<uint, ushort, Stream>> onPacketReceived = new Dictionary<uint, Action<uint, ushort, Stream>>();
	Crc32 crc32 = new Crc32();

	Dictionary<uint, byte[]> packetsPendingToBeAcked = new Dictionary<uint, byte[]>();
	Queue<int> sequenceNumbersReceived = new Queue<int>();
	const uint numberOfBitsInAck = 32;

	uint localSequence = 0;
	uint expectedSequence = 0;
	int ackBitfields = 0;

	protected override void Initialize()
	{
		NetworkManager.Instance.OnReceiveEvent += OnReceiveData;
	}

	private void FixedUpdate()
	{
		if (NetworkManager.Instance.isServer) return;
		if (packetsPendingToBeAcked.Count <= 0) return;

		using (var pendingPacket = packetsPendingToBeAcked.GetEnumerator())
		{
			while (pendingPacket.MoveNext())
			{
				UnityEngine.Debug.Log("Sending pending packet");
				SendPacketData(pendingPacket.Current.Value);
			}
		}
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

		UnityEngine.Debug.Log("Adding packet to pendign list: ID " + localSequence);
		packetsPendingToBeAcked.Add(localSequence, packetData);

		SendPacketData(packetData);
	}

	public void SendPacket<T>(NetworkPacket<T> packetToSend, uint objectID = 0)
	{
		byte[] packetData = SerializePacket(packetToSend, false, objectID);
		SendPacketData(packetData);
	}

	private void SendPacketData(byte[] data)
	{
		if (NetworkManager.Instance.isServer)
			NetworkManager.Instance.Broadcast(data);
		else
			NetworkManager.Instance.SendToServer(data);
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

			localSequence++;
			userPacketHeader.packetTypeID = packetToSerialize.userPacketTypeID;
			userPacketHeader.packetID = localSequence;
			userPacketHeader.senderID = NetworkManager.Instance.clientID;
			userPacketHeader.objectID = objectID;
			userPacketHeader.isReliable = isReliable;

			userPacketHeader.Serialize(memoryStream);

			if (isReliable)
			{
				ReliablePacketHeader reliablePacketHeader = new ReliablePacketHeader();

				reliablePacketHeader.sequence = localSequence;
				reliablePacketHeader.ack = expectedSequence;

				UnityEngine.Debug.Log("Setting up ackbits for packet Nro." + userPacketHeader.packetID);
				ackBitfields = 0;
				for (int i = 0; i <= numberOfBitsInAck; i++)
				{
					if (sequenceNumbersReceived.Contains((int)(expectedSequence - i)))
					{
						UnityEngine.Debug.Log("Sequence number is contained: " + (int)(expectedSequence - i));
						ackBitfields |= 1 << i;
					}
				}
				UnityEngine.Debug.Log("Ackbits to send: " + Convert.ToString(ackBitfields, 2));
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

				ProcessReliablePacketReceived(reliablePacketHeader);
			}	

			InvokeCallback(userPacketHeader, memoryStream);
		}
		else
		{
			NetworkManager.Instance.OnReceivePacket(iPEndPoint, (PacketType)packetHeader.packetTypeID, memoryStream);
		}
		memoryStream.Close();
	}

	private void ProcessReliablePacketReceived(ReliablePacketHeader reliablePacketHeader)
	{
		UnityEngine.Debug.Log("Reliable packet received");

		if (reliablePacketHeader.sequence > expectedSequence)
			expectedSequence = reliablePacketHeader.sequence;

		int ackBits = reliablePacketHeader.ackBitfield;
		UnityEngine.Debug.Log("Ackbits received: " + Convert.ToString(ackBits, 2));


		if (!sequenceNumbersReceived.Contains((int)reliablePacketHeader.sequence))
			sequenceNumbersReceived.Enqueue((int)reliablePacketHeader.sequence);

		for (int i = 0; i < numberOfBitsInAck; i++)
		{
			if ((ackBits & (1 << i)) != 0)
			{
				int packetSequenceToAck = (int)(reliablePacketHeader.sequence - i);
				UnityEngine.Debug.Log("Bit at position " + i + " is set.");
				UnityEngine.Debug.Log("Acknowledging packet sequence " + packetSequenceToAck);

				if (!sequenceNumbersReceived.Contains(packetSequenceToAck))
					sequenceNumbersReceived.Enqueue(packetSequenceToAck);

				if (packetsPendingToBeAcked.ContainsKey((uint)packetSequenceToAck))
				{
					UnityEngine.Debug.Log("Removing packet from pending list: ID " + packetSequenceToAck);
					packetsPendingToBeAcked.Remove((uint)packetSequenceToAck);
					UnityEngine.Debug.Log("Number of pending packets: " + packetsPendingToBeAcked.Count);
				}
			}

			if (sequenceNumbersReceived.Count >= numberOfBitsInAck)
			{
				sequenceNumbersReceived.Dequeue();
			}
		}
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
