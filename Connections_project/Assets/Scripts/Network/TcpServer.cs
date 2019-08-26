using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class StateObject
{
	public Socket workSocket = null;
	public const int BufferSize = 1024;
	public byte[] buffer = new byte[BufferSize];
	public StringBuilder sb = new StringBuilder();
}

public class TcpServer
{
	private struct DataReceived
	{
		public byte[] data;
		public IPEndPoint ipEndPoint;
	}

	private IReceiveData receiver = null;
	private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

	object handler = new object();

	public static ManualResetEvent allDone = new ManualResetEvent(false);


	public void StartServer(IPAddress ip, int port, IReceiveData receiver = null)
	{
		UnityEngine.Debug.Log("Starting server");
		IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
		Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


		try
		{
			listener.Bind(ipEndPoint);
			listener.Listen(4);

			// Set the vent to nonsignaled state
			allDone.Reset();

			// Start an asynchronous socket to listen for connections
			UnityEngine.Debug.Log("Waiting for a connection...");
			listener.BeginAccept(new AsyncCallback(OnAccept), listener);

			// Wait until a connection is made before continuing
			allDone.WaitOne();

			//while (true)
			//{
			//}
		}
		catch (SocketException e)
		{
			UnityEngine.Debug.Log("Error: " + e.StackTrace);
		}
	}

	private void OnAccept(IAsyncResult ar)
	{
		// Signal the main thread to continue
		allDone.Set();

		// Get the socket that handles the client request
		Socket listener = (Socket)ar.AsyncState;
		Socket handler = listener.EndAccept(ar);

		// Create the state object
		StateObject state = new StateObject();
		state.workSocket = handler;
		handler.BeginReceive(
			state.buffer, 
			0, 
			StateObject.BufferSize, 
			0, 
			new AsyncCallback(OnRecieve), 
			state
		);
	}

	private static void OnRecieve(IAsyncResult ar)
	{
		String content = String.Empty;

		// Retrieve the state object and the handler socket
		// from the asynchronous state object
		StateObject state = (StateObject)ar.AsyncState;
		Socket handler = state.workSocket;

		// Read data from the client socket
		int bytesRead = handler.EndReceive(ar);

		if (bytesRead > 0)
		{
			// There might be more data, so store the data received so far
			state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0 , bytesRead));

			if (content.IndexOf("<EOF>") > -1)
			{
				// All data has been read from the client
				String bytesLengthMsg = "Read " + content.Length + " bytes from socket.";
				UnityEngine.Debug.Log(bytesLengthMsg);
				
				String dataMsg = "Data: " + content;
				UnityEngine.Debug.Log(dataMsg);

				// Echo data back to the client
				Send(handler, content);
			}
		}
	}

	private static void Send(Socket handler, String data)
	{
		byte[] byteData = Encoding.ASCII.GetBytes(data);

		// Begin sending the data to the remote device
		handler.BeginSend(byteData, 0 , byteData.Length, 0, new AsyncCallback(SendCallback), handler);
	}

	private static void SendCallback(IAsyncResult ar)
	{
		try 
		{
			Socket handler = (Socket)ar.AsyncState;

			int bytesStent = handler.EndSend(ar);
			String debugMsg = "Sent " + bytesStent + " bytes to client";
			UnityEngine.Debug.Log(debugMsg);

			handler.Shutdown(SocketShutdown.Both);
			handler.Close();
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError("TCP Server: " + e.Message);
		}
	}
}
