using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Data;
using REPX.Extensions;

namespace REPX.Patches.Game.Player
{
	// Token: 0x02000011 RID: 17
	[HarmonyPatch(typeof(PlayerHealth))]
	internal class PlayerHealthPatch
	{
		// Token: 0x06000070 RID: 112 RVA: 0x000061B4 File Offset: 0x000043B4
		[HarmonyPatch("Death")]
		[HarmonyPrefix]
		private static bool Death_Prefix(PlayerHealth __instance)
		{
			bool flag = Settings.Instance.SettingsData.b_GodMode && __instance.GetField<PlayerAvatar>("playerAvatar").IsLocalPlayer();
			return !flag;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000061F4 File Offset: 0x000043F4
		[HarmonyPatch("Hurt")]
		[HarmonyPrefix]
		private static bool Hurt_Prefix(PlayerHealth __instance)
		{
			bool flag = Settings.Instance.SettingsData.b_GodMode && __instance.GetField<PlayerAvatar>("playerAvatar").IsLocalPlayer();
			return !flag;
		}
	}
}
