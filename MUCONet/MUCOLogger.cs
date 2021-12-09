using System;
using System.Globalization;

namespace PhenomenalViborg.MUCONet
{
	/// <summary>
	/// Struct containing convenient data about a log message.
	/// </summary>
	public struct MUCOLogMessage
	{
		/// <summary>
		/// Enum containing all supported log verbosity levels.
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
			return $"[{TimeStamp.ToString(@"HH:mm:ss.fff")}] [{LogLevel}] MUCONet: {Message}";
		}
	}

	/// <summary>
	/// The only logger used in the MUCONet libary, client applications can hook custom handlers with the MUCOLogger.LogEvent.
	/// </summary>
	public static class MUCOLogger
    {
		public delegate void LogDelegate(MUCOLogMessage message);
		public static event LogDelegate LogEvent;

		public static MUCOLogMessage.MUCOLogLevel LogLevel = MUCOLogMessage.MUCOLogLevel.Trace;

		/// <summary>
		/// A log level describing events showing step by step execution of your code that can be ignored during the standard operation, but may be useful during extended debugging sessions.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Trace(string message)
		{
			if ((int)LogLevel >= (int)MUCOLogMessage.MUCOLogLevel.Trace)
			{
				LogEvent?.Invoke(new MUCOLogMessage(MUCOLogMessage.MUCOLogLevel.Trace, message));
			}
		}

		/// <summary>
		/// A log level used for events considered to be useful during software debugging when more granular information is needed.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Debug(string message)
		{
			if ((int)LogLevel >= (int)MUCOLogMessage.MUCOLogLevel.Debug)
			{
				LogEvent?.Invoke(new MUCOLogMessage(MUCOLogMessage.MUCOLogLevel.Debug, message));
			}
		}

		/// <summary>
		/// An event happened, the event is purely informative and can be ignored during normal operations.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Info(string message)
		{
			if ((int)LogLevel >= (int)MUCOLogMessage.MUCOLogLevel.Info)
			{
				LogEvent?.Invoke(new MUCOLogMessage(MUCOLogMessage.MUCOLogLevel.Info, message));
			}
		}

		/// <summary>
		/// Unexpected behavior happened inside the application, but it is continuing its work and the key business features are operating as expected.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Warn(string message)
		{
			if ((int)LogLevel >= (int)MUCOLogMessage.MUCOLogLevel.Warn)
			{
				LogEvent?.Invoke(new MUCOLogMessage(MUCOLogMessage.MUCOLogLevel.Warn, message));
			}
		}

		/// <summary>
		/// One or more functionalities are not working, preventing some functionalities from working correctly.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Error(string message)
        {
			if ((int)LogLevel >= (int)MUCOLogMessage.MUCOLogLevel.Error)
			{
				LogEvent?.Invoke(new MUCOLogMessage(MUCOLogMessage.MUCOLogLevel.Error, message));
			}
		}

		/// <summary>
		/// One or more key business functionalities are not working and the whole system doesn’t fulfill the business functionalities.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Fatal(string message)
		{
			if ((int)LogLevel >= (int)MUCOLogMessage.MUCOLogLevel.Fatal)
			{
				LogEvent?.Invoke(new MUCOLogMessage(MUCOLogMessage.MUCOLogLevel.Fatal, message));
			}
		}
	}
}