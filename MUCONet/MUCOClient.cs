using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Phenomenal.MUCONet
{
	/// <summary>
	/// Handles all "low-level" socket communication with the server.
	/// </summary>
	public class MUCOClient
	{
		public delegate void PacketHandler(MUCOPacket packet);

		private Dictionary<int, PacketHandler> m_PacketHandlers = new Dictionary<int, PacketHandler>();

		private byte[] m_ReceiveBuffer = new byte[MUCOConstants.RECEIVE_BUFFER_SIZE];
		private Socket m_LocalSocket;
		private MUCOPacket m_ReceiveData = new MUCOPacket();

		/// <summary>
		/// Constructs an instance of MUCOClient.
		/// </summary>
		public MUCOClient()
		{
			// Register internal packet handlers
			RegisterPacketHandler((int)MUCOInternalServerPacketIdentifiers.Welcome, HandleWelcome);
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

		private void HandleWelcome(MUCOPacket packet)
		{
			int assignedClientID = packet.ReadInt();
			MUCOLogger.Info($"Welcome, {assignedClientID}");

			MUCOPacket welcomeRecivedPacket = new MUCOPacket((int)MUCOInternalClientPacketIdentifiers.WelcomeRecived);
			welcomeRecivedPacket.WriteInt(assignedClientID);
			SendPacket(welcomeRecivedPacket);
		}

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

				MUCOLogger.Info($"Connecting to server {ipEndPoint}");

				// Begins an asynchronous request for a remote host connection.
				m_LocalSocket.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallback), null);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"Failed to create and/or configure the Socket: {exception.Message}");
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

				MUCOLogger.Info("Connection was successfully established with the server.");

				m_LocalSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"Failed to accept request from remote host: {exception.Message}");
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
				MUCOLogger.Error($"An error occurred when sending data: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOClient::RecieveCallback is an asynchronous callback used for handling incoming data.
		/// </summary>
		private void RecieveCallback(IAsyncResult asyncResult)
		{
			try
			{
				int bytesReceived = m_LocalSocket.EndReceive(asyncResult);

				MUCOLogger.Trace($"Received {bytesReceived} bytes from the server.");

				byte[] dataReceived = new byte[bytesReceived];
				Array.Copy(m_ReceiveBuffer, dataReceived, bytesReceived);


				// FIXME: Should i take care of stiching packet segment or does the standard do this for me?
				// From my tests it seems like the only reason a packet would be split is the receive buffer size?..
				m_ReceiveData = new MUCOPacket(dataReceived);
				MUCOLogger.Trace($"Received {bytesReceived} out of {m_ReceiveData.ReadInt()} bytes.");
				m_ReceiveData.SetReadOffset(0);
				HandlePacket(m_ReceiveData);

				m_LocalSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An error occurred while receiving data: {exception.Message}");
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

			MUCOLogger.Trace($"Handleing packet new packet. Packet size: {size}, packet id: {packetID}.");

			if (m_PacketHandlers.ContainsKey(packetID))
			{
				m_PacketHandlers[packetID](packet);
			}
			else
			{
				MUCOLogger.Warn($"Failed to find package handler for packet with identifier: {packetID}");
			}
		}

		/// <summary>
		/// Sends a packet to the server.
		/// </summary>
		/// <param name="packet">The packet to send.</param>
		/// <param name="reliable">Reliable packets are sent using TCP, non-reliable packets use UDP.</param>
		public void SendPacket(MUCOPacket packet, bool reliable = true)
		{

			if (reliable)
			{
				MUCOLogger.Trace($"Sending a packet to the server.");
				packet.WriteLength();
				m_LocalSocket.BeginSend(packet.ToArray(), 0, packet.GetSize(), SocketFlags.None, new AsyncCallback(SendCallback), null);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}