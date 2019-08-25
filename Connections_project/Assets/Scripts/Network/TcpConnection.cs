using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class TcpConnection
{
	private struct DataReceived
	{
		public byte[] data;
		public IPEndPoint ipEndPoint;
	}

	private readonly TcpClient tcpClient = null;
	private readonly TcpListener myList = null;
	private Socket serverSocket = null;
	private Socket clientSocket = null;

	private IReceiveData receiver = null;
	private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

	object handler = new object();

	public void StartServer(IPAddress ip, int port, IReceiveData receiver = null)
	{
		try
		{
			serverSocket = new Socket(
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp
			);

			IPEndPoint ipEndPoint = new IPEndPoint(ip, port);

			serverSocket.Bind(ipEndPoint);
			serverSocket.Listen(4);
			serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
		}
		catch (SocketException e)
		{
			UnityEngine.Debug.Log("Error: " + e.StackTrace);
		}
	}

	public void StartClient()
	{

	}

	private void OnAccept(IAsyncResult ar)
	{
		try
		{
			clientSocket = serverSocket.EndAccept(ar);
			serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

			DataReceived dataReceived = dataReceivedQueue.Dequeue();
			clientSocket.BeginReceive(
				dataReceived.data,
				0,
				dataReceived.data.Length,
				SocketFlags.None,
				new AsyncCallback(OnRecieve),
				clientSocket
			);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.Log("Error: " + e.Message);
		}
	}

	private void OnRecieve(IAsyncResult ar)
	{
		try 
		{
			DataReceived dataReceived = new DataReceived();
			clientSocket.EndReceive(ar);
			
			lock (handler)
			{
				dataReceivedQueue.Enqueue(dataReceived);
			}

			clientSocket.BeginReceive(
				dataReceived.data,
				0,
				dataReceived.data.Length,
				SocketFlags.None,
				new AsyncCallback(OnRecieve),
				clientSocket
			);
		}
		catch(SocketException e)
		{
			UnityEngine.Debug.LogError("TCP_Connection: " + e.Message);
		}
	}
}
