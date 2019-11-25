using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;

public enum ClientConnectionState
{
	OnConnectionRequest,
	OnRequestingChallenge,
	OnChallengeResponse,
	OnConnectionAccepted
}

public class Client
{
    public float timeStamp;
    public uint id;
    public IPEndPoint ipEndPoint;
	public ulong clientSalt;
	public ulong serverSalt;

    public Client(IPEndPoint ipEndPoint, uint id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress { get; private set; }
    public int port { get; private set; }
    public bool isServer { get; private set; }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;
	private ClientConnectionState clientConnectionState;

	private readonly Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
    private readonly Dictionary<IPEndPoint, uint> ipToId = new Dictionary<IPEndPoint, uint>();

	private ulong clientSalt;
	private ulong serverSalt;

    public uint clientID { get; private set; }
    
	protected override void Initialize()
	{
		enabled = false;
		clientID = 0;
	} 

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
    }

    public void StartClient(IPAddress ip, int port)
    {
        isServer = false;
        
        this.port = port;
        this.ipAddress = ip;
        
        connection = new UdpConnection(ip, port, this);

		clientConnectionState = ClientConnectionState.OnConnectionRequest;

		enabled = true;
    }

	void Update()
	{
		if (connection != null)
			connection.FlushReceiveData();

		if (isServer) return;

		switch (clientConnectionState)
		{
			case ClientConnectionState.OnConnectionRequest:
				Debug.Log("OnConnectionRequest");
				SendConnectionRequest();
				break;
			case ClientConnectionState.OnChallengeResponse:
				Debug.Log("OnChallengeResponse");
				SendChallengeResponse(clientSalt, serverSalt);
				break;

			default: 
				break;
		}
	}

	void SendConnectionRequest()
	{
		ConnectionRequestPacket connectionRequestPacket = new ConnectionRequestPacket();
		connectionRequestPacket.payload.clientSalt = (ulong)UnityEngine.Random.Range(0, ulong.MaxValue);

		PacketManager.Instance.SendPacket<ConnectionRequestPayload>(connectionRequestPacket);
	}

    void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding client: " + ip.Address);

            uint id = clientID;
            ipToId[ip] = clientID;
            
            clients.Add(clientID, new Client(ip, id, Time.realtimeSinceStartup));
            clientID++;
        }
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        //AddClient(ip);

		Debug.Log("OnReceiveData");

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

	public void SendToServer<T>(NetworkPacket<T> packetToSend)
	{
		PacketManager.Instance.SendPacket<T>(packetToSend);
	}

	public void SendToClient(byte[] data, IPEndPoint iPEndPoint)
	{
		connection.Send(data, iPEndPoint);
	}

	public void SendToClient<T>(NetworkPacket<T> packet, IPEndPoint iPEndPoint)
	{
		PacketManager.Instance.SendPacket<T>(packet, iPEndPoint);
	}

	public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
                connection.Send(data, iterator.Current.Value.ipEndPoint);
        }
    }

	// Called by the PacketManager
	public void OnReceivePacket(IPEndPoint iPEndPoint, PacketType packetType, Stream stream)
	{
		Debug.Log("Packet received");
		switch(packetType)
		{
			case PacketType.ConnectionRequest:
				Debug.Log("OnConnectionRequest");

				ConnectionRequestPacket connectionRequestPacket = new ConnectionRequestPacket();
				connectionRequestPacket.Deserialize(stream);
				SendChallengeRequest(iPEndPoint, connectionRequestPacket.payload);
				break;
			case PacketType.ChallengeRequest:
				Debug.Log("OnChallengeRequest");
				
				ChallengeRequestPacket challengeRequestPacket = new ChallengeRequestPacket();
				challengeRequestPacket.Deserialize(stream);
				SendChallengeResponse(iPEndPoint, challengeRequestPacket.payload);
				break;
			case PacketType.ChallengeResponse:
				Debug.Log("OnChallengeResponse");
				
				ChallengeResponsePacket challengeResponsePacket = new ChallengeResponsePacket();
				challengeResponsePacket.Deserialize(stream);
				CheckResultForConnection(iPEndPoint, challengeResponsePacket.payload);
				break;
			case PacketType.ConnectionAccepted:
				Debug.Log("OnConnectionAccepted");
				
				if (!isServer && clientConnectionState == ClientConnectionState.OnChallengeResponse)
				{
					clientConnectionState = ClientConnectionState.OnConnectionAccepted;
				}
				break;
		}
	}

	private void SendChallengeRequest(IPEndPoint iPEndPoint, ConnectionRequestPayload connectionRequestPayload)
	{
		if (isServer) 
		{
			if (ipToId.ContainsKey(iPEndPoint))
			{
				Client newClient = new Client(iPEndPoint, clientID++, DateTime.Now.Ticks);
				newClient.clientSalt = connectionRequestPayload.clientSalt;
				newClient.serverSalt = (ulong)UnityEngine.Random.Range(0, ulong.MaxValue);
				clients.Add(newClient.id, newClient);
				ipToId.Add(iPEndPoint, newClient.id);

				Debug.Log("Adding client: " + iPEndPoint);
			}
			SendChallengeRequest(clients[ipToId[iPEndPoint]]);
		}
	}

	private void SendChallengeRequest(Client client)
	{
		ChallengeRequestPacket packet = new ChallengeRequestPacket();
		packet.payload.clientID = client.id;
		packet.payload.clientSalt = client.clientSalt;
		packet.payload.serverSalt = client.serverSalt;
		SendToClient(packet, client.ipEndPoint);
	}

	private void SendChallengeResponse(IPEndPoint iPEndPoint, ChallengeRequestPayload challengeRequestPayload)
	{
		if (!isServer && clientConnectionState == ClientConnectionState.OnConnectionRequest)
		{
			clientSalt = challengeRequestPayload.clientSalt;
			serverSalt = challengeRequestPayload.serverSalt;
			clientConnectionState = ClientConnectionState.OnChallengeResponse;
			SendChallengeResponse(clientSalt, serverSalt);
		}
	}

	private void SendChallengeResponse(ulong clientSalt, ulong serverSalt)
	{
		ChallengeResponsePacket challengeResponsePacket = new ChallengeResponsePacket();
		challengeResponsePacket.payload.result = clientSalt ^ serverSalt;
		SendToServer(challengeResponsePacket);
	}

	private void CheckResultForConnection(IPEndPoint iPEndPoint, ChallengeResponsePayload challengeResponsePayload)
	{
		if (isServer)
		{
			Client client = clients[ipToId[iPEndPoint]];
			ulong result = client.clientSalt ^ client.serverSalt;
			if (challengeResponsePayload.result == result)
				SendToClient(new ConnectionAcceptedPacket(), iPEndPoint);
		}
	}
}
