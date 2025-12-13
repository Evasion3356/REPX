using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Extensions;
using UnityEngine;

namespace REPX.Patches.UI
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch(typeof(CursorManager))]
	internal class CursorManagerPatch
	{
		// Token: 0x0600005E RID: 94 RVA: 0x00005AF8 File Offset: 0x00003CF8
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		private static bool Update_Prefix(CursorManager __instance)
		{
			float field = __instance.GetField<float>("unlockTimer");
			bool flag = field > 0f;
			bool flag2;
			if (flag)
			{
				Cursor.lockState = 0;
				Cursor.visible = true;
				__instance.SetField("unlockTimer", field - Time.deltaTime);
				flag2 = false;
			}
			else
			{
				bool flag3 = field != -1234f;
				if (flag3)
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
					__instance.SetField("unlockTimer", -1234f);
				}
				flag2 = false;
			}
			return flag2;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00005B7C File Offset: 0x00003D7C
		[HarmonyPatch("Unlock")]
		[HarmonyPrefix]
		private static bool Unlock_Prefix(CursorManager __instance, float _time)
		{
			Cursor.lockState = 0;
			__instance.SetField("unlockTimer", _time);
			return false;
		}
	}
}
