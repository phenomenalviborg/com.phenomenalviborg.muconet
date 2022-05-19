using System;
using System.Collections.Generic;

namespace PhenomenalViborg.MUCONet
{
	/// <summary>
	/// Enum containing all internal packets identifiers used to specifiy server packages.
	/// The value range of internal packets identifiers range from 32768 to 65536.
	/// </summary>
	public enum MUCOInternalServerPacketIdentifiers : int
	{
		Welcome = 32768,
	}

	/// <summary>
	/// Enum containing all internal packets identifiers used to specifiy client packages.
	/// The value range of internal packets identifiers range from 32768 to 65536.
	/// </summary>
	public enum MUCOInternalClientPacketIdentifiers : int
	{
		WelcomeRecived = 32768,
	}

	public class MUCOPacket : IDisposable
	{
		private List<byte> m_Data;
		private int m_ReadOffset;

		/// <summary>
		/// Constructs a new empty MUCOPacket (without an id).
		/// </summary>
		/// <param name="data">The initial packet data.</param>
		public MUCOPacket()
		{
			m_Data = new List<byte>();
			m_ReadOffset = 0;
		}

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
		/// Inserts the packet size at the start of the packet data.
		/// </summary>
		public void WriteLength()
		{
			m_Data.InsertRange(0, BitConverter.GetBytes(GetSize() + sizeof(int)));
		}

		/// <summary>
		/// Gets the unread length of the buffer.
		/// </summary>
		/// <returns>The size of the remaining (unread) part of the buffer</returns>
		public int UnreadLength()
		{
			return GetSize() - m_ReadOffset;
		}

		/// <summary>
		/// Sets the read offset to the specified offset.
		/// </summary>
		/// <param name="readOffset">The new read offset.</param>
		public void SetReadOffset(int readOffset)
		{
			m_ReadOffset = readOffset;
		}

		/// <summary>
		/// Returns the current read offset.
		/// </summary>
		public int GetReadOffset()
		{
			return m_ReadOffset;
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
		/// Resets the packet to allow it to be reused.
		/// </summary>
		/// <param name="_shouldReset"></param>
		public void Reset()
		{
			m_Data.Clear();
			m_ReadOffset = 0;
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
		/// Writes a string to the packet data.
		/// </summary>
		/// <param name="value">The string to add.</param>
		public void WriteString(string value)
		{
			WriteInt(value.Length); // Add the length of the string to the packet
			m_Data.AddRange(System.Text.Encoding.ASCII.GetBytes(value));
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

		/// <summary>
		/// Reads a string from the packet data. 
		/// </summary>
		/// <param name="moveReadOffset">Whether or not to move the buffer's read position offset.</param>
		/// <returns>The requested string.</returns>
		public string ReadString(bool moveReadPos = true)
		{
			try
			{
				int length = ReadInt(); // Get the length of the string
				string value = System.Text.Encoding.ASCII.GetString(m_Data.ToArray(), m_ReadOffset, length);
				if (moveReadPos && value.Length > 0)
				{
					// If moveReadPos is true string is not empty
					m_ReadOffset += length; // Increase readPos by the length of the string
				}
				return value; // Return the string
			}
			catch
			{
				throw new Exception("Could not read value of type 'string'!");
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
}