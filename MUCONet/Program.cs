using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace Phenomenal.MUCONet
{
    #region Logging

    /// <summary>
    /// MUCOLogLevel in an enum containing all supported log verbosity levels.
    /// </summary>
    public enum MUCOLogLevel
	{
		Trace = 0,
		Debug = 1,
		Info = 2,
		Warn = 3,
		Error = 4,
		Fatal = 5
	}

	/// <summary>
	/// MUCOLogMessage is a struct containing convenient data about a log message.
	/// </summary>
	public struct MUCOLogMessage
	{
		public DateTime TimeStamp;
		public MUCOLogLevel LogLevel;
		public string Message;

		public MUCOLogMessage(MUCOLogLevel logLevel, string message)
		{
			TimeStamp = DateTime.Now;
			LogLevel = logLevel;
			Message = message;
		}

		public override string ToString()
        {
			return $"[{TimeStamp.ToString("HH:mm:ss.fff", new CultureInfo("es-ES", false))}] [{LogLevel}] MUCONet: {Message}";
        }
	}

	/// <summary>
	/// MUCOLogger is the only logger used in the MUCONet libary, client applications can hook custom handlers with the MUCOLogger.LogEvent.
	/// </summary>
	public static class MUCOLogger
    {
		public delegate void LogDelegate(MUCOLogMessage message);
		public static event LogDelegate LogEvent;

		/// <summary>
		/// A log level describing events showing step by step execution of your code that can be ignored during the standard operation, but may be useful during extended debugging sessions.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Trace(string message)
		{
			LogEvent?.Invoke(new MUCOLogMessage(MUCOLogLevel.Trace, message));
		}

		/// <summary>
		/// A log level used for events considered to be useful during software debugging when more granular information is needed.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Debug(string message)
		{
			LogEvent?.Invoke(new MUCOLogMessage(MUCOLogLevel.Debug, message));
		}

		/// <summary>
		/// An event happened, the event is purely informative and can be ignored during normal operations.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Info(string message)
		{
			LogEvent?.Invoke(new MUCOLogMessage(MUCOLogLevel.Info, message));
		}

		/// <summary>
		/// Unexpected behavior happened inside the application, but it is continuing its work and the key business features are operating as expected.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Warn(string message)
		{
			LogEvent?.Invoke(new MUCOLogMessage(MUCOLogLevel.Warn, message));
		}

		/// <summary>
		/// One or more functionalities are not working, preventing some functionalities from working correctly.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Error(string message)
        {
			LogEvent?.Invoke(new MUCOLogMessage(MUCOLogLevel.Error, message));
		}

		/// <summary>
		/// One or more key business functionalities are not working and the whole system doesn’t fulfill the business functionalities.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Fatal(string message)
		{
			LogEvent?.Invoke(new MUCOLogMessage(MUCOLogLevel.Fatal, message));
		}
	}

    #endregion

    #region Packets
    public class MUCOPacket : IDisposable
    {
        private List<byte> m_Data;
        private int m_ReadOffset;

		/// <summary>
		/// Constructs a MUCOPacket and adds the specified int identifier field to the start of the packet data.
		/// This will primarily be used when creating outgoing packets.
		/// </summary>
		/// <param name="data">The initial packet data.</param>
        public MUCOPacket(int id)
        {
            m_Data = new List<byte>();
            m_ReadOffset = 0;

            WriteInt(id);
        }

		/// <summary>
		/// Constructs a MUCOPacket from a byte array.
		/// This will primarily be used when reciving an incoming packet.
		/// </summary>
		/// <param name="data">The initial packet data.</param>
        public MUCOPacket(byte[] data)
        {
            m_Data = new List<byte>();
            m_ReadOffset = 0;

			WriteBytes(data);
        }

		/// <summary>
		/// Gets the size of the packet data.
		/// </summary>
		/// <returns>The size of the packet data in bytes.</returns>
		public int GetSize()
		{
			return m_Data.Count;
		}

		/// <summary>
		/// Gets the packet data as a byte array.
		/// </summary>
		/// <returns>The packet data as a byte array.</returns>
		public byte[] ToArray()
		{
			return m_Data.ToArray();
		}

		/// <summary>
		/// Writes a byte array to the packet data.
		/// </summary>
		/// <param name="value">The byte array to add.</param>
		public void WriteBytes(byte[] value)
		{
			m_Data.AddRange(value);
		}

	    /// <summary>
		/// Writes an int to the packet data.
		/// </summary>
		/// <param name="value">The int to add.</param>
		public void WriteInt(int value)
		{
			m_Data.AddRange(BitConverter.GetBytes(value));
		}

	    /// <summary>
		/// Writes a float to the packet data.
		/// </summary>
		/// <param name="value">The float to add.</param>
		public void WriteFloat(float value)
		{
			m_Data.AddRange(BitConverter.GetBytes(value));
		}

		/// <summary>
		/// Reads a byte array from the packet data. 
		/// </summary>
		/// <param name="length">The length of the byte array.</param>
		/// <param name="moveReadOffset">Whether or not to move the buffer's read position offset.</param>
		/// <returns>The requested byte array, or null if an error occurred.</returns>
		public byte[] ReadBytes(int length, bool moveReadOffset = true)
		{
			if (m_Data.Count >= m_ReadOffset + length)
            {
				byte[] value = m_Data.GetRange(m_ReadOffset, length).ToArray();
				if (moveReadOffset)
                {
					m_ReadOffset += length;
                }
				return value;
            }
			else
            {
				MUCOLogger.Error("Could not read value of type 'byte[]', value was out of range.");
				return null;
            }
		}

		/// <summary>
		/// Reads an int from the packet data. 
		/// </summary>
		/// <param name="moveReadOffset">Whether or not to move the buffer's read position offset.</param>
		/// <returns>The requested int, or 0 if an error occurred.</returns>
		public int ReadInt(bool moveReadOffset = true)
		{
			if (m_Data.Count >= m_ReadOffset + sizeof(int))
			{
				int value = BitConverter.ToInt32(m_Data.ToArray(), m_ReadOffset);
				if (moveReadOffset)
				{
					m_ReadOffset += sizeof(int);
				}
				return value;
			}
			else
			{
				MUCOLogger.Error("Could not read value of type 'int', value was out of range.");
				return 0;
			}
		}

		/// <summary>
		/// Reads a float from the packet data. 
		/// </summary>
		/// <param name="moveReadOffset">Whether or not to move the buffer's read position offset.</param>
		/// <returns>The requested float, or 0.0f if an error occurred.</returns>
		public float ReadFloat(bool moveReadOffset = true)
		{
			if (m_Data.Count >= m_ReadOffset + sizeof(float))
			{
				float value = BitConverter.ToSingle(m_Data.ToArray(), m_ReadOffset);
				if (moveReadOffset)
				{
					m_ReadOffset += sizeof(float);
				}
				return value;
			}
			else
			{
				MUCOLogger.Error("Could not read value of type 'float', value was out of range.");
				return 0.0f;
			}
		}

		// Implement the IDisposable
		private bool m_Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
					m_Data = null;
					m_ReadOffset = 0;
                }

				m_Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    #endregion


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
		public int UniqueIdentifier;
		public Socket RemoteSocket;
	}

	/// <summary>
	/// MUCOServer handles all "low-level" socket communication with the clients.
	/// </summary>
	public class MUCOServer
	{
		public Dictionary<int, MUCOClientInfo> ClientInfo { get; private set; } = new Dictionary<int, MUCOClientInfo>();

		private byte[] m_ReceiveBuffer = new byte[MUCOConstants.RECEIVE_BUFFER_SIZE];
		private Socket m_LocalSocket = null;
		private int m_PlayerIDCounter = 0;

		/// <summary>
		/// MUCOServer::Start is responsible for starting the server.
		/// </summary>
		public void Start()
		{
			MUCOLogger.Info("Starting server...");

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
		/// MUCOServer::AcceptCallback is an asynchronous callback used for handling incoming connection.
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

				// Begin an asynchronously operation to receive incoming data from clientSocket. Incoming data will be stored in m_ReceiveBuffer 
				clientSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientInfo);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An error occurred trying to handle an incomming connection: {exception.Message}");
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
				MUCOClientInfo clientInfo = (MUCOClientInfo)asyncResult.AsyncState;
				clientInfo.RemoteSocket.EndReceive(asyncResult);

				// TODO: Handle Data - If command id is LOGIN, register client to array of clients, etc.
				MUCOLogger.Debug("ReceiveCallback");

				// TMP: Echo
				/*List<byte> message = new List<byte>();
				message.AddRange(Encoding.UTF8.GetBytes("Test message"));
				byte[] byteArray = message.ToArray();
				clientSocket.BeginSend(byteArray, 0, message.Count, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);*/

				// Begin an asynchronously operation to receive incoming data from clientSocket. Incoming data will be stored in m_ReceiveBuffer 
				clientInfo.RemoteSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientInfo);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An error occurred trying to handle incoming data: {exception.Message}");
			}
		}

		/// <summary>
		/// MUCOServer::SendCallback is an asynchronous callback used when sending data.
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
		/// The MUCOServer::Stop is responsible for stopping the server, as well as freeing all runtime server data and disconnecting all connected clients.
		/// </summary>
		public void Stop()
		{
			MUCOLogger.Info("Shutting down server...");
			throw new NotImplementedException();
		}

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

		private void SendPacket(MUCOClientInfo receiver, MUCOPacket packet, bool reliable = true)
        {
			if (reliable)
            {
				MUCOLogger.Trace($"Sending packet to client {receiver.UniqueIdentifier} ({receiver.RemoteSocket.RemoteEndPoint})");
				receiver.RemoteSocket.BeginSend(packet.ToArray(), 0, packet.GetSize(), SocketFlags.None, new AsyncCallback(SendCallback), receiver);
			}
			else
            {
				throw new NotImplementedException();
			}
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
				m_LocalSocket.EndReceive(asyncResult);

				MUCOLogger.Debug("MUCOClient::RecieveCallback");

				m_LocalSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
			}
			catch (Exception exception)
			{
				MUCOLogger.Error($"An error occurred while receiving data: {exception.Message}");
			}
		}

		public void SendPacket(MUCOPacket packet, bool reliable = true)
		{
			if (reliable)
			{
				MUCOLogger.Trace($"Sending a packet to the server.");
				m_LocalSocket.BeginSend(packet.ToArray(), 0, packet.GetSize(), SocketFlags.None, new AsyncCallback(SendCallback), null);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}