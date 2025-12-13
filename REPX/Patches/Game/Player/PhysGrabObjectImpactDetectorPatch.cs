using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Data;
using REPX.Extensions;

namespace REPX.Patches.Game.Player
{
	// Token: 0x0200000D RID: 13
	[HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
	internal class PhysGrabObjectImpactDetectorPatch
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00005BAC File Offset: 0x00003DAC
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		private static void Update_Prefix(PhysGrabObjectImpactDetector __instance)
		{
			bool flag = Settings.Instance.SettingsData.b_IndestructibleObjects && __instance.isValuable;
			if (flag)
			{
				__instance.SetField("indestructibleSpawnTimer", 0.1f);
				PhysGrabObject field = __instance.GetField<PhysGrabObject>("physGrabObject");
				if (field != null)
				{
					field.OverrideIndestructible(0.1f);
				}
			}
		}
	}
}
