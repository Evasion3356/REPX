using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using REPX.Data;
using REPX.Extensions;

namespace REPX.Patches.Game.Player
{
	[HarmonyPatch(typeof(PlayerVoiceChat))]
	internal class PlayerVoiceChatPatch
	{
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		private static bool Update_Prefix(PlayerVoiceChat __instance)
		{
			// Check if we're in lobby - if so, always execute original method
			bool isInLobby = SemiFunc.RunIsLobbyMenu();
			if (isInLobby)
			{
				return true; // Execute original Update method for normal lobby voice chat
			}

			// We're in-game now, check if "Hear Everyone" is enabled
			bool b_HearEveryone = Settings.Instance.SettingsData.b_HearEveryone;
			if (!b_HearEveryone)
			{
				return true; // Execute original Update method normally
			}

			// Safety check - make sure PlayerController and avatar exist
			if (PlayerController.instance == null || PlayerController.instance.playerAvatarScript == null)
			{
				return true; // Execute original if not ready
			}

			// In-game with "Hear Everyone" enabled - spoof as if we're in lobby
			// Set inLobbyMixer to true to enable lobby voice chat behavior
			bool field = __instance.GetField<bool>("inLobbyMixer");
			bool field2 = PlayerController.instance.playerAvatarScript.GetField<bool>("deadSet");
			
			// Only set inLobbyMixer if player is not dead and not already set
			if (!field2 && !field)
			{
				__instance.SetField("inLobbyMixer", true);
			}
			
			// Skip original Update to maintain our lobby spoofing
			return false;
		}
	}
}
