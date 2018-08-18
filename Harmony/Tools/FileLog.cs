// file:	Tools\FileLog.cs
//
// summary:	Implements the file log class
/// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Harmony
{
	/// <summary>A file log.</summary>
	public static class FileLog
	{
		/// <summary>Full pathname of the log file.</summary>
		public static string logPath;
		/// <summary>The indent character.</summary>
		public static char indentChar = '\t';
		/// <summary>The indent level.</summary>
		public static int indentLevel = 0;
		static List<string> buffer = new List<string>();

		[UpgradeToLatestVersion(1)]
		static FileLog()
		{
			logPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar + "harmony.log.txt";
		}

		static string IndentString()
		{
			return new string(indentChar, indentLevel);
		}

		/// <summary>Change indent.</summary>
		/// <param name="delta">The delta.</param>
		///
		public static void ChangeIndent(int delta)
		{
			indentLevel = Math.Max(0, indentLevel + delta);
		}

		/// <summary>
		///   use this method only if you are sure that FlushBuffer will be called
		///   or else logging information is incomplete in case of a crash.
		/// </summary>
		/// <param name="str">The string.</param>
		///
		public static void LogBuffered(string str)
		{
			lock (logPath)
			{
				buffer.Add(IndentString() + str);
			}
		}

		/// <summary>Flushes the buffer.</summary>
		public static void FlushBuffer()
		{
			lock (logPath)
			{
				if (buffer.Count > 0)
				{
					using (var writer = File.AppendText(logPath))
					{
						foreach (var str in buffer)
							writer.WriteLine(str);
					}
					buffer.Clear();
				}
			}
		}

		/// <summary>
		///   this is the slower method that flushes changes directly to the file
		///   to prevent missing information in case of a cache.
		/// </summary>
		/// <param name="str">The string.</param>
		///
		public static void Log(string str)
		{
			lock (logPath)
			{
				using (var writer = File.AppendText(logPath))
				{
					writer.WriteLine(IndentString() + str);
				}
			}
		}

		/// <summary>Resets this FileLog.</summary>
		public static void Reset()
		{
			lock (logPath)
			{
				var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar + "harmony.log.txt";
				File.Delete(path);
			}
		}

		/// <summary>Logs the bytes.</summary>
		/// <param name="ptr">The pointer.</param>
		/// <param name="len">The length.</param>
		///
		public static unsafe void LogBytes(long ptr, int len)
		{
			lock (logPath)
			{
				var p = (byte*)ptr;
				var s = "";
				for (var i = 1; i <= len; i++)
				{
					if (s == "") s = "#  ";
					s = s + (*p).ToString("X2") + " ";
					if (i > 1 || len == 1)
					{
						if (i % 8 == 0 || i == len)
						{
							Log(s);
							s = "";
						}
						else if (i % 4 == 0)
							s = s + " ";
					}
					p++;
				}

				var arr = new byte[len];
				Marshal.Copy((IntPtr)ptr, arr, 0, len);
				var md5Hash = MD5.Create();
				var hash = md5Hash.ComputeHash(arr);
#pragma warning disable XS0001
				var sBuilder = new StringBuilder();
#pragma warning restore XS0001
				for (var i = 0; i < hash.Length; i++)
					sBuilder.Append(hash[i].ToString("X2"));
				Log("HASH: " + sBuilder);
			}
		}
	}
}