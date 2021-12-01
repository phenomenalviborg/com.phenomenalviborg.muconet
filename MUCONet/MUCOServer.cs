using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Phenomenal.MUCONet
{
	/// <summary>
	/// Handles all "low-level" socket communication with the clients.
	/// </summary>
	public class MUCOServer
	{
		/// <summary>
		/// Used for storeing information about a remote client.
		/// </summary>
		public struct MUCOClientInfo
		{
			public int UniqueIdentifier;
			public Socket RemoteSocket;

			public void Disconnect()
            {
				RemoteSocket.Close();
            }

			public override string ToString()
            {
				return $"client {UniqueIdentifier} ({RemoteSocket.RemoteEndPoint})";
            }

		}

		public Dictionary<int, MUCOClientInfo> ClientInfo { get; private set; } = new Dictionary<int, MUCOClientInfo>();
		private Dictionary<int, PacketHandler> m_PacketHandlers = new Dictionary<int, PacketHandler>();

		private byte[] m_ReceiveBuffer = new byte[MUCOConstants.RECEIVE_BUFFER_SIZE];
		private Socket m_LocalSocket = null;
		private int m_PlayerIDCounter = 0;

		// User events and delegates
		public delegate void OnClientConnectedDelegate();
		public event OnClientConnectedDelegate OnClientConnectedEvent;

		public delegate void OnClienttDisconnectedDelegate();
		public event OnClienttDisconnectedDelegate OnClientDisconnectedEvent;

		/// <summary>
		/// Constructs an instance of MUCOServer.
		/// </summary>
		public MUCOServer()
		{
			RegisterPacketHandler((int)MUCOInternalClientPacketIdentifiers.WelcomeRecived, HandleWelcomeReceived);
		}

		/// <summary>
		/// Starts the server.
		/// </summary>
		/// <param name="port">The port number of the socket endpoint.</param>
		public void Start(int port)
		{
			MUCOLogger.Info("Starting server...");

			try
			{
				// The AddressFamily enum specifies the addressing scheme that an instance of the Socket class can use. AddressFamily.InterNetwork represents address for IP version 4 (IPv4).
				// The SocketType enum specifies the type of socket that an instance of the Socket class represents. SocketType.Steam is a reliable, two-way, connection-based byte stream.
				// The ProtocolType enum specifies the protocols that the Socket class supports. ProtocolType.TCP represents Transmission Control Protocol(TCP)
				m_LocalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// IPAddress.Any Provides an IP address that indicates that the server must listen for client activity on all network interfaces.
				IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);

				// Socket::Bind associates the Socket with the local endpoint. 
				m_LocalSocket.Bind(ipEndPoint);

				// Socket::Listen places the Socket in a listening state. The backlog parameter specifies the maximum length of the pending connections queue.
				m_LocalSocket.Listen(4);

				MUCOLogger.Info($"Successfully started server on port {1000}.");

				// Begin an asynchronous operation to accept an incoming connection attempt.
				m_LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"Failed to create and/or configure the Socket: {exception.Message}");
			}
		}

		/// <summary>
		/// Stops the server.
		/// Frees all runtime server data and disconnects all connected clients.
		/// </summary>
		public void Stop()
		{
			MUCOLogger.Info("Shutting down server...");
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Sends a packet to a specific client.
		/// </summary>
		/// <param name="receiver">The client to send the packet to.</param>
		/// <param name="packet">The packet to send.</param>
		/// <param name="reliable">Reliable packets are sent using TCP, non-reliable packets use UDP.</param>
		public void SendPacket(MUCOClientInfo receiver, MUCOPacket packet, bool reliable = true)
		{
			if (reliable)
			{
				MUCOLogger.Trace($"Sending packet to {receiver}.");
				packet.WriteLength();
				receiver.RemoteSocket.BeginSend(packet.ToArray(), 0, packet.GetSize(), SocketFlags.None, new AsyncCallback(SendCallback), receiver);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Sends a packet to all connected clients.
		/// </summary>
		/// <param name="packet">The packet to send.</param>
		/// <param name="reliable">Reliable packets are sent using TCP, non-reliable packets use UDP.</param>
		public void SendPacketToAll(MUCOPacket packet, bool reliable = true)
		{
			if (reliable)
			{
				foreach (KeyValuePair<int, MUCOClientInfo> keyValuePair in ClientInfo)
				{
					SendPacket(keyValuePair.Value, packet, true);
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Registers a packet handler to specifed packet identifier.
		/// </summary>
		/// <param name="packetIdentifier">The packet identifier to assign the packet handler.</param>
		/// <param name="packetHandler">The packet handler delegate.</param>
		public void RegisterPacketHandler(int packetIdentifier, PacketHandler packetHandler)
		{
			if (m_PacketHandlers.ContainsKey(packetIdentifier))
			{
				MUCOLogger.Error($"Failed to register packet handler to packet identifier: {packetIdentifier}. The specified packet identifier has already been assigned a packet handler.");
				return;
			}

			MUCOLogger.Info($"Successfully assigned a packet handler to packet identifier: {packetIdentifier}");

			m_PacketHandlers.Add(packetIdentifier, packetHandler);
		}

		/// <summary>
		/// An asynchronous callback used for handling incoming connection.
		/// </summary>
		private void AcceptCallback(IAsyncResult asyncResult)
		{
			try
			{
				Socket clientSocket = m_LocalSocket.EndAccept(asyncResult);

				// Begin an asynchronously operation to accept another incoming connection attempt.
				m_LocalSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

				MUCOLogger.Info($"Incoming connection from {clientSocket.RemoteEndPoint}...");

				m_PlayerIDCounter++;

				MUCOClientInfo clientInfo = new MUCOClientInfo();
				clientInfo.UniqueIdentifier = m_PlayerIDCounter;
				clientInfo.RemoteSocket = clientSocket;
				ClientInfo.Add(clientInfo.UniqueIdentifier, clientInfo);

				// Send welcome packet
				MUCOPacket welcomePacket = new MUCOPacket((int)MUCOInternalServerPacketIdentifiers.Welcome);
				welcomePacket.WriteInt(clientInfo.UniqueIdentifier);
				SendPacket(clientInfo, welcomePacket);

				// Begin an asynchronously operation to receive incoming data from clientSocket. Incoming data will be stored in m_ReceiveBuffer 
				clientSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientInfo);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An error occurred trying to handle an incomming connection: {exception.Message}");
			}
		}

		/// <summary>
		/// An asynchronous callback used when sending data.
		/// </summary>
		private void SendCallback(IAsyncResult asyncResult)
		{
			try
			{
				MUCOClientInfo clientInfo = (MUCOClientInfo)asyncResult.AsyncState; ;
				clientInfo.RemoteSocket.EndSend(asyncResult);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An error occurred when sending data: {exception.Message}");
			}
		}

		/// <summary>
		/// An asynchronous callback used for handling incoming data.
		/// </summary>
		private void ReceiveCallback(IAsyncResult asyncResult)
		{
			MUCOClientInfo clientInfo = (MUCOClientInfo)asyncResult.AsyncState;
		
			try
			{
				int bytesReceived = clientInfo.RemoteSocket.EndReceive(asyncResult);

				MUCOLogger.Trace($"Receiving package from {clientInfo}.");

				byte[] dataReceived = new byte[bytesReceived];
				Array.Copy(m_ReceiveBuffer, dataReceived, bytesReceived);

				// FIXME: Should i take care of stiching packet segment or does the standard do this for me?
				// From my tests it seems like the only reason a packet would be split is the receive buffer size?..
				// Reserch Nagle's algorithm, that might be causeing package splitting without the package size exceeding the receive buffer size.
				MUCOPacket packet = new MUCOPacket(dataReceived);
				int packetSize = packet.ReadInt();
				if (packetSize >= MUCOConstants.RECEIVE_BUFFER_SIZE)
				{
					// If this ever triggers, the package size should be increased or package stitching should be implemented.
					MUCOLogger.Error($"The package size was greater than the receive buffer size!");
					return;
				}
				packet.SetReadOffset(0);
				HandlePacket(packet);

				// Begin an asynchronously operation to receive incoming data from clientSocket. Incoming data will be stored in m_ReceiveBuffer 
				clientInfo.RemoteSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientInfo);
			}
			catch (SocketException exception)
            {
				if (exception.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset)
                {
					Disconnect(clientInfo);
				}
				else
				{
					MUCOLogger.Error($"A socket exception occurred trying to handle incoming data: {exception.Message}");
				}
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An exception occurred trying to handle incoming data: {exception.Message}");
			}
		}

		private void Disconnect(MUCOClientInfo clientInfo)
        {
			MUCOLogger.Info($"Disconnecting {clientInfo}");

			if (ClientInfo.ContainsKey(clientInfo.UniqueIdentifier))
            {
				clientInfo.Disconnect();
				ClientInfo.Remove(clientInfo.UniqueIdentifier);
				OnClientDisconnectedEvent?.Invoke();
			}
			else
            {
				MUCOLogger.Error($"Failed to find player the to disconnect, UniqueIdentifier: {clientInfo.UniqueIdentifier}.");
            }
		}

		/// <summary>
		/// Makes sure that the specified packet gets passed on the correct PacketHandler.
		/// </summary>
		/// <param name="packet">The packet to handle</param>
		private void HandlePacket(MUCOPacket packet)
		{
			int size = packet.ReadInt();
			int packetID = packet.ReadInt();

			if (m_PacketHandlers.ContainsKey(packetID))
			{
				m_PacketHandlers[packetID](packet);
			}
			else
			{
				MUCOLogger.Error($"Failed to find package handler for packet with identifier: {packetID}");
			}
		}

		#region Internal package handlers
		private void HandleWelcomeReceived(MUCOPacket packet)
		{
			MUCOLogger.Info("Welcome Received");
		
			OnClientConnectedEvent?.Invoke();
		}
		#endregion
	}

}