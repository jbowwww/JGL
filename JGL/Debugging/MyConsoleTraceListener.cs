using System;
using System.Diagnostics;

namespace JGL.Debugging
{
	public class MyConsoleTraceListener : AsyncTraceListener
	{
		public MyConsoleTraceListener() : base("ConsoleListener")
		{
			TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId;
		}

		internal override System.IO.Stream OpenStream()
		{
			return Console.OpenStandardOutput();
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			LogMessage msg = (LogMessage)data;
			byte[] buf = FormatMessage(string.Concat(msg.MessageAsText, "\n"));
			Stream.Write(buf, 0, buf.Length);													// Write message buffer
			Stream.Flush();

		}
	}
}

