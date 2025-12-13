using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Data;
using REPX.Extensions;
using UnityEngine;

namespace REPX.Patches.Game.Player
{
	// Token: 0x02000013 RID: 19
	[HarmonyPatch(typeof(PlayerVisionTarget))]
	internal class PlayerVisionTargetPatch
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00006338 File Offset: 0x00004538
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		private static bool Update_Prefix(PlayerVisionTarget __instance)
		{
			bool flag = __instance.GetField<PlayerAvatar>("PlayerAvatar").IsLocalPlayer();
			if (flag)
			{
				bool b_Invulnerable = Settings.Instance.SettingsData.b_Invulnerable;
				if (b_Invulnerable)
				{
					__instance.SetField("TargetPosition", -100000f);
					__instance.SetField("TargetRotation", default(Quaternion));
					__instance.VisionTransform.localPosition = new Vector3(0f, -100000f, 0f);
					__instance.VisionTransform.rotation = default(Quaternion);
					return false;
				}
			}
			return true;
		}
	}
}
