using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPX.Helpers
{
	internal static class Log
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000083 RID: 131 RVA: 0x000065E7 File Offset: 0x000047E7
		private static string LogFilePath
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath, "REPX.log");
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00006608 File Offset: 0x00004808
		internal static void Clear()
		{
			object @lock = Log._lock;
			lock (@lock)
			{
				File.WriteAllText(Log.LogFilePath, string.Empty);
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00006658 File Offset: 0x00004858
		internal static void LogInfo(string info, string tag = "Log")
		{
			object @lock = Log._lock;
			lock (@lock)
			{
				string text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				string text2 = string.Concat(new string[]
				{
					"[",
					text,
					"][",
					tag,
					"]: ",
					info,
					Environment.NewLine
				});
				File.AppendAllText(Log.LogFilePath, text2);
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000066F0 File Offset: 0x000048F0
		internal static void LogWarning(string info)
		{
			Log.LogInfo(info, "Warning");
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000066FE File Offset: 0x000048FE
		internal static void LogError(string info)
		{
			Log.LogInfo(info, "Error");
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000670C File Offset: 0x0000490C
		internal static void LogError(Exception ex)
		{
			Log.LogInfo(ex.ToString(), "Error");
		}

		// Token: 0x0400002E RID: 46
		private static readonly object _lock = new object();

		// Token: 0x0400002F RID: 47
		private const string LogFileName = "REPX.log";
	}
}
