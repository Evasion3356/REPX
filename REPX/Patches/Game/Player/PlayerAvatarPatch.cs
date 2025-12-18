using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using REPX.Data;
using REPX.Extensions;
using REPX.Modules;
using UnityEngine;

namespace REPX.Patches.Game.Player
{
	[HarmonyPatch(typeof(PlayerAvatar))]
	internal class PlayerAvatarPatch
	{
		[HarmonyPatch("Awake")]
		[HarmonyPostfix]
		private static void Start_Postfix(PlayerAvatar __instance)
		{
			__instance.gameObject.AddComponent<ExtendedPlayerData>();
		}

		[HarmonyPatch("PlayerDeath")]
		[HarmonyPrefix]
		private static bool PlayerDeath_Prefix(PlayerAvatar __instance, int enemyIndex)
		{
			bool flag = Settings.Instance.SettingsData.b_GodMode && __instance.IsLocalPlayer();
			return !flag;
		}

		[HarmonyPatch("PlayerDeathRPC")]
		[HarmonyPrefix]
		private static bool PlayerDeathRPC_Prefix(PlayerAvatar __instance, int enemyIndex, PhotonMessageInfo _info)
		{
			bool flag = Settings.Instance.SettingsData.b_GodMode && __instance.IsLocalPlayer();
			bool flag3;
			if (flag)
			{
				bool flag2 = SemiFunc.IsMultiplayer();
				if (flag2)
				{
					//__instance.photonView.RPC("ReviveRPC", RpcTarget.MasterClient, new object[] { false });
					__instance.playerHealth.GetPhotonView("photonView").RPC("HealOtherRPC", RpcTarget.MasterClient, new object[]
					{
						__instance.playerHealth.GetField<int>("health") - 1,
						false
					});
				}
				flag3 = false;
			}
			else
			{
				flag3 = true;
			}
			return flag3;
		}
	}
}
