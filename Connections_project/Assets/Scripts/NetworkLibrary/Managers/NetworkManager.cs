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
    public int TimeOut = 30;

    public IPAddress ipAddress { get; private set; }
    public int port { get; private set; }
    public bool isServer { get; private set; }

    public Action<byte[], IPEndPoint> OnReceiveEvent;

	private readonly Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
	private readonly Dictionary<IPEndPoint, uint> ipToId = new Dictionary<IPEndPoint, uint>();

	private UdpConnection connection;
	private ClientConnectionState clientConnectionState;

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
		enabled = true;
    }

    public void StartClient(IPAddress ip, int port)
    {
        isServer = false;
        
        this.port = port;
        this.ipAddress = ip;
        
        connection = new UdpConnection(ip, port, this);
		SetClientConnectionStateTo(ClientConnectionState.OnConnectionRequest);

		enabled = true;
    }

	private void Update()
	{
		if (connection != null)
			connection.FlushReceiveData();

		if (!isServer)
			HandleConnectionHandshake();
	}
	
	void HandleConnectionHandshake()
	{
		switch (clientConnectionState)
		{
			case ClientConnectionState.OnConnectionRequest:
				SendConnectionRequest();
				break;
			case ClientConnectionState.OnChallengeResponse:
				SendChallengeResponse(clientSalt, serverSalt);
				break;
		}
	}

	private void SendConnectionRequest()
	{
		Debug.Log("Sending connection request");

		ConnectionRequestPacket connectionRequestPacket = new ConnectionRequestPacket();
		connectionRequestPacket.payload.clientSalt = (ulong)UnityEngine.Random.Range(0, ulong.MaxValue);

		SendToServer(connectionRequestPacket);
	}

	private void SendChallengeResponse(ulong clientSalt, ulong serverSalt)
	{
		Debug.Log("Sending challenge response");

		ChallengeResponsePacket challengeResponsePacket = new ChallengeResponsePacket();
		challengeResponsePacket.payload.result = clientSalt ^ serverSalt;
		
		SendToServer(challengeResponsePacket);
	}

	// Called by the PacketManager every time it receives a packet
	public void OnReceivePacket(IPEndPoint iPEndPoint, PacketType packetType, Stream stream)
	{
		switch (packetType)
		{
			case PacketType.ConnectionRequest:
				OnConnectionRequest(iPEndPoint, stream);
				break;

			case PacketType.ChallengeRequest:
				OnChallengeRequest(iPEndPoint, stream);
				break;

			case PacketType.ChallengeResponse:
				OnChallengeResponse(iPEndPoint, stream);
				break;

			case PacketType.ConnectionAccepted:
				OnConnectionAccepted();
				break;
		}
	}

	private void OnConnectionRequest(IPEndPoint iPEndPoint, Stream stream)
	{
		Debug.Log("OnConnectionRequest");
		ConnectionRequestPacket connectionRequestPacket = new ConnectionRequestPacket();
		connectionRequestPacket.Deserialize(stream);
		SendChallengeRequest(iPEndPoint, connectionRequestPacket.payload);
	}

	private void SendChallengeRequest(IPEndPoint iPEndPoint, ConnectionRequestPayload connectionRequestPayload)
	{
		if (isServer && !ipToId.ContainsKey(iPEndPoint))
		{
			AddClient(iPEndPoint, connectionRequestPayload.clientSalt);
			Debug.Log("Adding client: " + iPEndPoint);
		}
		SendChallengeRequest(clients[ipToId[iPEndPoint]]);
	}

	private void AddClient(IPEndPoint iPEndPoint, ulong clientSalt)
	{
		Client newClient = new Client(iPEndPoint, clientID++, DateTime.Now.Ticks);
		newClient.clientSalt = clientSalt;
		newClient.serverSalt = (ulong)UnityEngine.Random.Range(0, ulong.MaxValue);
		clients.Add(newClient.id, newClient);
		ipToId.Add(iPEndPoint, newClient.id);
	}

	private void SendChallengeRequest(Client client)
	{

		ChallengeRequestPacket packet = new ChallengeRequestPacket();
		packet.payload.clientID = client.id;
		packet.payload.clientSalt = client.clientSalt;
		packet.payload.serverSalt = client.serverSalt;
		SendToClient(packet, client.ipEndPoint);
	}

	private void OnChallengeRequest(IPEndPoint iPEndPoint, Stream stream)
	{
		Debug.Log("OnChallengeRequest");
		ChallengeRequestPacket challengeRequestPacket = new ChallengeRequestPacket();
		challengeRequestPacket.Deserialize(stream);
		StoreClientAndServerSalt(challengeRequestPacket.payload);
		SetClientConnectionStateTo(ClientConnectionState.OnChallengeResponse);
	}

	private void StoreClientAndServerSalt(ChallengeRequestPayload challengeRequestPayload)
	{
		if (!isServer && clientConnectionState == ClientConnectionState.OnConnectionRequest)
		{
			clientSalt = challengeRequestPayload.clientSalt;
			serverSalt = challengeRequestPayload.serverSalt;
		}
	}

	private void OnChallengeResponse(IPEndPoint iPEndPoint, Stream stream)
	{
		Debug.Log("OnChallengeResponse");
		ChallengeResponsePacket challengeResponsePacket = new ChallengeResponsePacket();
		challengeResponsePacket.Deserialize(stream);
		CheckResultForConnection(iPEndPoint, challengeResponsePacket.payload);
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

	private void OnConnectionAccepted()
	{
		if (!isServer && clientConnectionState == ClientConnectionState.OnChallengeResponse)
		{
			Debug.Log("OnConnectionAccepted");
			clientConnectionState = ClientConnectionState.OnConnectionAccepted;
		}
	}

	private void SetClientConnectionStateTo(ClientConnectionState state)
	{
		clientConnectionState = state;
	}

    private void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

	public void SendToServer<T>(NetworkPacket<T> packetToSend)
	{
		PacketManager.Instance.SendPacket(packetToSend);
	}

	public void SendToClient(byte[] data, IPEndPoint iPEndPoint)
	{
		connection.Send(data, iPEndPoint);
	}

	public void SendToClient<T>(NetworkPacket<T> packet, IPEndPoint iPEndPoint)
	{
		PacketManager.Instance.SendPacketToClient<T>(packet, iPEndPoint);
	}

	public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
                connection.Send(data, iterator.Current.Value.ipEndPoint);
        }
    }
}
