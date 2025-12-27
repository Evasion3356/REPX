using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using REPX.Data;
using REPX.Extensions;
using REPX.Helpers;
using REPX.Modules;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPX.Patches.Game.Player
{
	[HarmonyPatch(typeof(PlayerAvatar))]
	internal class PlayerAvatarPatch
	{
		// Cache structure for SemiFunc values (updated once per level)
		internal class SemiFuncCache
		{
			public bool isMainMenu;
			public bool runIsLobbyMenu;
			public bool runIsLobby;
			public bool runIsShop;
			public bool isMultiplayer;
			public bool runIsLevel;
		}

		// Static cache instance accessible by CheatGUI
		internal static SemiFuncCache Cache { get; private set; } = new SemiFuncCache();

		[HarmonyPatch("Awake")]
		[HarmonyPostfix]
		private static void Start_Postfix(PlayerAvatar __instance)
		{
			__instance.gameObject.AddComponent<ExtendedPlayerData>();
			
			// Update SemiFunc cache when player entity is created (once per level)
			UpdateSemiFuncCache();
		}

		private static void UpdateSemiFuncCache()
		{
			Cache.isMainMenu = SemiFunc.IsMainMenu();
			Cache.runIsLobbyMenu = SemiFunc.RunIsLobbyMenu();
			Cache.runIsLobby = SemiFunc.RunIsLobby();
			Cache.runIsShop = SemiFunc.RunIsShop();
			Cache.isMultiplayer = SemiFunc.IsMultiplayer();
			Cache.runIsLevel = SemiFunc.RunIsLevel();
			Log.LogInfo("SemiFunc Cache Updated: " +
				$"isMainMenu={Cache.isMainMenu}, " +
				$"runIsLobbyMenu={Cache.runIsLobbyMenu}, " +
				$"runIsLobby={Cache.runIsLobby}, " +
				$"runIsShop={Cache.runIsShop}, " +
				$"isMultiplayer={Cache.isMultiplayer}, " +
				$"runIsLevel={Cache.runIsLevel}");
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
				bool flag2 = Cache.isMultiplayer;
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
