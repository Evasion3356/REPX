using System;
using System.Runtime.CompilerServices;

namespace REPX.Modules
{
	// Token: 0x02000016 RID: 22
	internal static class ExtendedPlayerDataExtension
	{
		// Token: 0x06000081 RID: 129 RVA: 0x00006590 File Offset: 0x00004790
		internal static ExtendedPlayerData TryExtendedData(this PlayerAvatar pa)
		{
			ExtendedPlayerData extendedPlayerData;
			bool flag = ExtendedPlayerData._extendedPlayerData.TryGetValue(pa, out extendedPlayerData);
			ExtendedPlayerData extendedPlayerData2;
			if (flag)
			{
				extendedPlayerData2 = extendedPlayerData;
			}
			else
			{
				extendedPlayerData2 = null;
			}
			return extendedPlayerData2;
		}

		// Token: 0x06000082 RID: 130 RVA: 0x000065BC File Offset: 0x000047BC
		internal static ExtendedPlayerData ExtendedData(this PlayerAvatar pa)
		{
			ExtendedPlayerData extendedPlayerData;
			bool flag = ExtendedPlayerData._extendedPlayerData.TryGetValue(pa, out extendedPlayerData);
			if (flag)
			{
				return extendedPlayerData;
			}
			throw new Exception();
		}
	}
}
