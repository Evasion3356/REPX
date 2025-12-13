using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Helpers;

namespace REPX.Patches.Game.Player
{
	// Token: 0x0200000E RID: 14
	[HarmonyPatch(typeof(PhysGrabObject))]
	internal class PhysGrabObjectPatch
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00005C10 File Offset: 0x00003E10
		[HarmonyPatch("Awake")]
		[HarmonyPostfix]
		private static void Awake_Postfix(PhysGrabObject __instance)
		{
			MonoHelper.CatchedPhysGrabObjects.Add(__instance);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00005C1F File Offset: 0x00003E1F
		[HarmonyPatch("OnDestroy")]
		[HarmonyPostfix]
		private static void OnDestroy_Postfix(PhysGrabObject __instance)
		{
			MonoHelper.CatchedPhysGrabObjects.Remove(__instance);
		}
	}
}
