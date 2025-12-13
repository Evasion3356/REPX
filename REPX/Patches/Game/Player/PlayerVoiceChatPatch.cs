using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Data;
using REPX.Extensions;

namespace REPX.Patches.Game.Player
{
	// Token: 0x02000014 RID: 20
	[HarmonyPatch(typeof(PlayerVoiceChat))]
	internal class PlayerVoiceChatPatch
	{
		// Token: 0x06000079 RID: 121 RVA: 0x000063E0 File Offset: 0x000045E0
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		private static void Update_Prefix(PlayerVoiceChat __instance)
		{
			bool flag = SemiFunc.RunIsLobbyMenu();
			if (!flag)
			{
				bool field = __instance.GetField<bool>("inLobbyMixer");
				bool field2 = PlayerController.instance.playerAvatarScript.GetField<bool>("deadSet");
				bool b_HearEveryone = Settings.Instance.SettingsData.b_HearEveryone;
				if (b_HearEveryone)
				{
					bool flag2 = !field2 && !field;
					if (flag2)
					{
						__instance.SetField("inLobbyMixer", true);
					}
				}
				else
				{
					bool flag3 = !field2 && field;
					if (flag3)
					{
						__instance.SetField("inLobbyMixer", false);
					}
				}
			}
		}
	}
}
