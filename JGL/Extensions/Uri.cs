using System;
using System.IO;
using System.Net.Sockets;
using JGL.Debugging;

namespace JGL.Extensions
{
	/// <summary>
	/// Extension method class for <see cref="System.Uri"/>
	/// </summary>
	/// <remarks>
	///	- TODO:
	///		- Tracing (sort out overall tracing approach (source names, how many sources, etc) within JGL project)
	/// </remarks>
	public static class Uri_Ext
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Open the <paramref name="uri"/> and return a <see cref="System.Stream"/> representing it
		/// </summary>
		/// <param name="uri"><see cref="System.Uri"/> to open</param>
		public static Stream Open(this Uri uri, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.None)
		{
			Stream s = null;
			if (uri.IsFile)
			{
				s = File.Open(uri.LocalPath, mode, access, share);
			}
			else if (uri.IsLoopback || uri.IsUnc)
			{
				Socket sock = new Socket(new SocketInformation() { Options = SocketInformationOptions.NonBlocking });
				sock.Connect(uri.Host, uri.Port);
				s = new NetworkStream(sock, access);
			}
			else
				throw new ArgumentException(string.Format("Unknown URI scheme \"{0}\"", uri.Scheme), "uri");
			return s;
		}
	}
}

