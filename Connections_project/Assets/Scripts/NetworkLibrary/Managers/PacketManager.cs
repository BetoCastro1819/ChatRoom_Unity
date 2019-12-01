using System.Collections.Generic;
using System.IO;
using System.Net;
using System;
using UnityEngine;

public class PacketManager : MonoBehaviourSingleton<PacketManager>, IReceiveData
{
	[SerializeField] bool enablePacketLossSimulation = false;
	[SerializeField] int porcentageOfPacketLossSimulation = 25;
	[SerializeField] float resendPacketRate = 1.0f;

	Dictionary<uint, Action<uint, ushort, Stream>> onPacketReceived = new Dictionary<uint, Action<uint, ushort, Stream>>();
	Crc32 crc32 = new Crc32();

	Dictionary<uint, byte[]> packetsPendingToBeAcked = new Dictionary<uint, byte[]>();
	Queue<uint> sequenceNumbersReceived = new Queue<uint>();
	const uint numberOfBitsInAck = 32;

	uint localSequence = 0;
	uint remoteSequence = 0;
	int ackBitfields = 0;

	float packetRateTimer = 0;

	protected override void Initialize()
	{
		NetworkManager.Instance.OnReceiveEvent += OnReceiveData;
	}

	private void Update()
	{
		if ((packetRateTimer += Time.deltaTime) < resendPacketRate) return;
		packetRateTimer = 0;

		if (packetsPendingToBeAcked.Count <= 0) 
		{
			//Debug.Log("No packets pending to resend");
			return;
		}

		using (var pendingPacket = packetsPendingToBeAcked.GetEnumerator())
		{
			while (pendingPacket.MoveNext())
			{
				UnityEngine.Debug.Log("Sending pending packet with ID " + pendingPacket.Current.Key);
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

		//UnityEngine.Debug.Log("Adding packet to pendign list: ID " + localSequence);
		//packetsPendingToBeAcked.Add(localSequence, packetData);

		SendPacketData(packetData);

		localSequence++;
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

			userPacketHeader.packetTypeID = packetToSerialize.userPacketTypeID;
			userPacketHeader.packetID = localSequence;
			userPacketHeader.senderID = NetworkManager.Instance.clientID;
			userPacketHeader.objectID = objectID;
			userPacketHeader.isReliable = isReliable;

			userPacketHeader.Serialize(memoryStream);


			ReliablePacketHeader reliablePacketHeader = new ReliablePacketHeader();

			reliablePacketHeader.sequence = localSequence;
			reliablePacketHeader.ack = remoteSequence;

			UnityEngine.Debug.Log("Setting up ackbits for packet Nro." + localSequence);
			ackBitfields = 0;
			for (int i = 0; i <= numberOfBitsInAck; i++)
			{
				if (sequenceNumbersReceived.Contains((uint)(remoteSequence - i)))
				{
					//UnityEngine.Debug.Log("Sequence number is contained: " + (int)(expectedSequence - i));
					ackBitfields |= 1 << i;
				}
			}
			UnityEngine.Debug.Log("Ackbits to send: " + Convert.ToString(ackBitfields, 2));
			reliablePacketHeader.ackBitfield = ackBitfields;

			reliablePacketHeader.Serialize(memoryStream);

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

		if (isReliable)
		{
			UnityEngine.Debug.Log("Adding packet to pending list: ID " + localSequence);
			if (!packetsPendingToBeAcked.ContainsKey(localSequence))
				packetsPendingToBeAcked.Add(localSequence, memoryStream.ToArray());
		}

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

			ReliablePacketHeader reliablePacketHeader = new ReliablePacketHeader();
			reliablePacketHeader.Deserialize(memoryStream);
			ProcessReliablePacketReceived(reliablePacketHeader);

			//if (userPacketHeader.isReliable)
			//{
			//}	

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
		if (enablePacketLossSimulation && UnityEngine.Random.Range(0, 100) < porcentageOfPacketLossSimulation) 
		{
			UnityEngine.Debug.Log("Packet lost simulation");
			return;
		}

		UnityEngine.Debug.Log("Reliable packet received");
		UnityEngine.Debug.Log("Remote sequence: ID " + remoteSequence);
		UnityEngine.Debug.Log("Sequence received: ID " + reliablePacketHeader.sequence);
		UnityEngine.Debug.Log("Latest packet that the other machine received: ID " + reliablePacketHeader.ack);

		if (reliablePacketHeader.sequence > remoteSequence)
			remoteSequence = reliablePacketHeader.sequence;

		if (!sequenceNumbersReceived.Contains(reliablePacketHeader.sequence))
			sequenceNumbersReceived.Enqueue(reliablePacketHeader.sequence);

		if (sequenceNumbersReceived.Count > numberOfBitsInAck)
			sequenceNumbersReceived.Dequeue();

		int ackBits = reliablePacketHeader.ackBitfield;
		UnityEngine.Debug.Log("Ackbits received: " + Convert.ToString(ackBits, 2));
		for (int i = 0; i < numberOfBitsInAck; i++)
		{
			if ((ackBits & (1 << i)) != 0)
			{
				int packetSequenceToAck = (int)(reliablePacketHeader.ack - i);

				UnityEngine.Debug.Log("Bit at position " + i + " is set.");
				UnityEngine.Debug.Log("Acknowledging packet sequence " + packetSequenceToAck);

				if (packetsPendingToBeAcked.ContainsKey((uint)packetSequenceToAck))
				{
					UnityEngine.Debug.Log("Removin packet Nro." + packetSequenceToAck + " from pending list");
					packetsPendingToBeAcked.Remove((uint)packetSequenceToAck);
				}
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
