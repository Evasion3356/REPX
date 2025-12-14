using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Data;
using UnityEngine;

namespace REPX.Patches.Game.Player
{
	// Token: 0x02000010 RID: 16
	[HarmonyPatch(typeof(PlayerController))]
	internal class PlayerControllerPatch
	{
		// Token: 0x0600006D RID: 109 RVA: 0x0000613C File Offset: 0x0000433C
		[HarmonyPatch("FixedUpdate")]
		[HarmonyPrefix]
		private static void FixedUpdate_Prefix(PlayerController __instance)
		{
			bool flag = Settings.Instance.SettingsData.b_InfiniteStamina && __instance.EnergyCurrent <= 1f;
			if (flag)
			{
				__instance.EnergyCurrent = 1f;
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00006180 File Offset: 0x00004380
		[HarmonyPatch("ForceImpulse")]
		[HarmonyPrefix]
		private static bool ForceImpulse_Prefix(Vector3 force)
		{
			bool b_AntiKnockBack = Settings.Instance.SettingsData.b_AntiKnockBack;
			return !b_AntiKnockBack;
		}
	}
}
