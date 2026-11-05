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
		private static void Update_Prefix(PlayerVoiceChat __instance)
		{
			// Only apply when in-game (not lobby menu) and feature is enabled
			if (SemiFunc.RunIsLobbyMenu())
				return;

			if (!Settings.Instance.SettingsData.b_HearEveryone)
				return;

			// Only affect how we *play back* other players. In 0.5 inLobbyMixer on our own voice chat also
			// switches voice transmission into "target every player" mode (re-resolved every 0.5s and
			// rewriting recorder.TargetPlayers), which costs network traffic/lag.
			if (__instance.GetField<Photon.Pun.PhotonView>("photonView").IsMine)
				return;

			// Safety check
			if (PlayerController.instance == null || PlayerController.instance.playerAvatarScript == null)
				return;

			bool isDead = PlayerController.instance.playerAvatarScript.GetField<bool>("deadSet");
			bool inLobbyMixer = __instance.GetField<bool>("inLobbyMixer");

			// Set inLobbyMixer so the original Update uses lobby audio routing
			if (!isDead && !inLobbyMixer)
			{
				__instance.SetField("inLobbyMixer", true);
			}

			// Always let the original Update run so TTS, mic device polling,
			// spatial audio, and all other 4.0 logic continues to work correctly.
		}
	}
}
