using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Phenomenal.MUCONet
{
	/// <summary>
	/// MUCOConstants holds shared configuration variables that won't change at runtime.
	/// </summary>
	public static class MUCOConstants
	{
		// The size of all receive buffers in bytes.
		public const int RECEIVE_BUFFER_SIZE = 4096;
	}

	/// <summary>
	/// MUCOServerInfo is used for storeing information about a remote client.
	/// </summary>
	public struct MUCOServerInfo
	{
	}

	/// <summary>
	/// MUCOClientInfo is used for storeing information about a remote client.
	/// </summary>
	public struct MUCOClientInfo
	{
		public Socket socket;
	}

	/// <summary>
	/// MUCOServer handles all "low-level" socket communication with the clients.
	/// </summary>
	public class MUCOServer
	{
		public List<MUCOClientInfo> ClientInfo { get; private set; } = new List<MUCOClientInfo>();

		private byte[] m_ReceiveBuffer = new byte[MUCOConstants.RECEIVE_BUFFER_SIZE];
		private Socket m_LocalSocket;

		/// <summary>
		/// MUCOServer::Start is responsible for starting the server.
		/// </summary>
		public void Start()
		{
			try
			{
				// The AddressFamily enum specifies the addressing scheme that an instance of the Socket class can use. AddressFamily.InterNetwork represents address for IP version 4 (IPv4).
				// The SocketType enum specifies the type of socket that an instance of the Socket class represents. SocketType.Steam is a reliable, two-way, connection-based byte stream.
				// The ProtocolType enum specifies the protocols that the Socket class supports. ProtocolType.TCP represents Transmission Control Protocol(TCP)
				m_LocalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// IPAddress.Any Provides an IP address that indicates that the server must listen for client activity on all network interfaces.
				IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);

				// Socket::Bind associates the Socket with the local endpoint. 
				m_LocalSocket.Bind(ipEndPoint);

				// Socket::Listen places the Socket in a listening state. The backlog parameter specifies the maximum length of the pending connections queue.
				m_LocalSocket.Listen(4);

				// Begin an asynchronous operation to accept an incoming connection attempt.
				m_LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] Failed to create and/or configure the Socket: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOServer::AcceptCallback is an asynchronous callback used for handling incoming connection.
		/// </summary>
		private void AcceptCallback(IAsyncResult asyncResult)
		{
			try
			{
				Socket clientSocket = m_LocalSocket.EndAccept(asyncResult);

				// Begin an asynchronously operation to accept another incoming connection attempt.
				m_LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

				Console.WriteLine($"Incoming connection from {clientSocket.RemoteEndPoint}...");

				// Begin an asynchronously operation to receive incoming data from clientSocket. Incoming data will be stored in m_ReceiveBuffer 
				clientSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] An error occurred trying to handle an incomming connection: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOServer::ReceiveCallback is an asynchronous callback used for handling incoming data.
		/// </summary>
		private void ReceiveCallback(IAsyncResult asyncResult)
		{
			try
			{
				// Grab the socket from the AsyncState, this is the "Object" parameter, the last parameter, passed into the Socket::BeginReceive method.
				Socket clientSocket = (Socket)asyncResult.AsyncState;
				clientSocket.EndReceive(asyncResult);

				// TODO: Handle Data - If command id is LOGIN, register client to array of clients, etc.
				Console.WriteLine("ReceiveCallback");

				// TMP: Echo
				List<byte> message = new List<byte>();
				message.AddRange(Encoding.UTF8.GetBytes("Test message"));
				byte[] byteArray = message.ToArray();
				clientSocket.BeginSend(byteArray, 0, message.Count, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);

				// Begin an asynchronously operation to receive incoming data from clientSocket. Incoming data will be stored in m_ReceiveBuffer 
				clientSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] An error occurred trying to handle incoming data: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOServer::SendCallback is an asynchronous callback used when sending data.
		/// </summary>
		private void SendCallback(IAsyncResult asyncResult)
		{
			try
			{
				Socket client = (Socket)asyncResult.AsyncState;
				client.EndSend(asyncResult);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] An error occurred when sending data: {exception.Message}");
			}
		}

		/// <summary>
		/// The MUCOServer::Stop is responsible for stopping the server, as well as freeing all runtime server data and disconnecting all connected clients.
		/// </summary>
		public void Stop()
		{
			// TODO: Impleme
		}
	}

	/// <summary>
	/// MUCOClient handles all "low-level" socket communication with the server.
	/// </summary>
	public class MUCOClient
	{
		public MUCOServerInfo ServerInfo { get; private set; }

		private byte[] m_ReceiveBuffer = new byte[MUCOConstants.RECEIVE_BUFFER_SIZE];
		private Socket m_LocalSocket;
		private bool b = false;

		/// <summary>
		/// MUCOClient::Start is responsible for starting the server.
		/// </summary>
		public void Connect()
		{
			try
			{
				// The AddressFamily enum specifies the addressing scheme that an instance of the Socket class can use. AddressFamily.InterNetwork represents address for IP version 4 (IPv4).
				// The SocketType enum specifies the type of socket that an instance of the Socket class represents. SocketType.Steam is a reliable, two-way, connection-based byte stream.
				// The ProtocolType enum specifies the protocols that the Socket class supports. ProtocolType.TCP represents Transmission Control Protocol(TCP)
				m_LocalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// Configure endpoint
				// TODO: the ip and port should not be const.
				IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
				IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

				Console.WriteLine($"[MUCONet|TRACE] Connecting to server {ipEndPoint}");

				// Begins an asynchronous request for a remote host connection.
				m_LocalSocket.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallback), null);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] Failed to create and/or configure the Socket: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOClient::ConnectCallback is an asynchronous callback used for handling an incoming connection from the remote host.
		/// </summary>
		private void ConnectCallback(IAsyncResult asyncResult)
		{
			try
			{
				m_LocalSocket.EndConnect(asyncResult);

				if (b == false)
				{
					b = true;
					//Start listening to the data asynchronously
					m_LocalSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
					Console.WriteLine("First time connect");
				}

				// At this point we should have an established connection with the server.
				// So let's try to send a message...
				List<byte> message = new List<byte>();
				message.AddRange(Encoding.UTF8.GetBytes("Message from client!"));
				byte[] byteArray = message.ToArray();
				m_LocalSocket.BeginSend(byteArray, 0, message.Count, SocketFlags.None, new AsyncCallback(SendCallback), null);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] Failed to accept request from remote host: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOClient::SendCallback is an asynchronous callback used when sending data.
		/// </summary>
		private void SendCallback(IAsyncResult asyncResult)
		{
			try
			{
				m_LocalSocket.EndSend(asyncResult);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] An error occurred when sending data: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOClient::RecieveCallback is an asynchronous callback used for handling incoming data.
		/// </summary>
		private void RecieveCallback(IAsyncResult asyncResult)
		{
			try
			{
				m_LocalSocket.EndReceive(asyncResult);

				Console.WriteLine("MUCOClient::RecieveCallback");

				m_LocalSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[MUCONet|ERROR] An error occurred while receiving data: {exception.Message}");
			}
		}
	}
}