using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Photon.Pun;
using REPX.Data;
using REPX.Extensions;
using UnityEngine;

namespace REPX.Patches.Game.Player
{
	// Token: 0x02000012 RID: 18
	[HarmonyPatch(typeof(PlayerTumble))]
	internal class PlayerTumblePatch
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00006240 File Offset: 0x00004440
		[HarmonyPatch("TumbleRequest")]
		[HarmonyPrefix]
		private static bool TumbleRequest_Prefix(PlayerTumble __instance, bool _isTumbling, bool _playerInput)
		{
			Log(__instance, "TumbleRequest(tumbling=" + _isTumbling + ", input=" + _playerInput + ")");
			bool flag = !__instance.playerAvatar.IsLocalPlayer();
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				if (Settings.Instance.SettingsData.b_AntiTumble)
				{
					if (!_isTumbling && _playerInput)
					{
						// Player is breaking out: the fall check in PlayerAvatar.Update would otherwise re-tumble
						// them next frame (fallDistance is still above its threshold), so clear it and hold off
						// involuntary tumbles briefly.
						_antiTumbleGraceUntil = Time.unscaledTime + AntiTumbleGraceSeconds;
						try
						{
							PlayerController.instance.GetField<object>("CollisionController").SetField("fallDistance", 0f);
						}
						catch (Exception e) { Debug.Log("[REPX/Tumble] fallDistance reset failed: " + e.Message); }
					}
					else if (_isTumbling && !_playerInput && Time.unscaledTime < _antiTumbleGraceUntil)
					{
						return false;
					}
				}
				bool flag3 = Settings.Instance.SettingsData.b_NoTumble && !_playerInput;
				if (flag3)
				{
					flag2 = false;
				}
				else
				{
					PlayerTumblePatch._byLocalPlayer = true;
					flag2 = true;
				}
			}
			return flag2;
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00006290 File Offset: 0x00004490
		[HarmonyPatch("TumbleSetRPC")]
		[HarmonyPrefix]
		private static bool TumbleSetRPC_Prefix(PlayerTumble __instance, bool _isTumbling, bool _playerInput, int _stateRevision, PhotonMessageInfo _info)
		{
			Log(__instance, "TumbleSetRPC(tumbling=" + _isTumbling + ", input=" + _playerInput + ", rev=" + _stateRevision + ", byLocal=" + _byLocalPlayer + ")");
			bool flag = !__instance.playerAvatar.IsLocalPlayer();
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				bool flag3 = Settings.Instance.SettingsData.b_NoTumble && !PlayerTumblePatch._byLocalPlayer;
				if (flag3)
				{
					// Keep the local revision in step with the master so later tumble requests aren't rejected as stale.
					__instance.SetField("tumbleStateRevision", _stateRevision);
					bool flag4 = SemiFunc.IsMultiplayer();
					if (flag4)
					{
						PlayerTumblePatch.ReSyncTumbleState(__instance);
					}
					PlayerTumblePatch._byLocalPlayer = false;
					flag2 = false;
				}
				else
				{
					PlayerTumblePatch._byLocalPlayer = false;
					flag2 = true;
				}
			}
			return flag2;
		}

		// Knockback applied while tumbling goes through TumbleForce (PlayerController.ForceImpulse only
		// covers the non-tumbling case), so Anti-Knock Back has to cancel it here too.
		[HarmonyPatch("TumbleForce")]
		[HarmonyPrefix]
		private static bool TumbleForce_Prefix(PlayerTumble __instance, Vector3 _force)
		{
			if (!Settings.Instance.SettingsData.b_AntiKnockBack)
				return true;
			return !__instance.playerAvatar.IsLocalPlayer();
		}

		// Diagnostics for tumble state changes on the local player.
		private static void Log(PlayerTumble t, string what)
		{
			try
			{
				if (t.playerAvatar == null || !t.playerAvatar.IsLocalPlayer()) return;
				Debug.Log("[REPX/Tumble] " + what
					+ " | isTumbling=" + t.GetField<bool>("isTumbling")
					+ " rev=" + t.GetField<int>("tumbleStateRevision")
					+ " override=" + t.GetField<bool>("tumbleOverride")
					+ " overrideTimer=" + t.GetField<float>("tumbleOverrideTimer")
					+ " stunTimer=" + t.GetField<float>("tumbleStunTimer")
					+ " reqTimer=" + t.GetField<float>("tumbleRequestTimer")
					+ " master=" + SemiFunc.IsMasterClientOrSingleplayer()
					+ " noTumble=" + Settings.Instance.SettingsData.b_NoTumble);
			}
			catch (Exception e) { Debug.Log("[REPX/Tumble] log failed: " + e.Message); }
		}

		[HarmonyPatch("TumbleRequestRPC")]
		[HarmonyPrefix]
		private static void TumbleRequestRPC_Log(PlayerTumble __instance, bool _isTumbling, bool _playerInput, int _stateRevision)
			=> Log(__instance, "TumbleRequestRPC(tumbling=" + _isTumbling + ", input=" + _playerInput + ", rev=" + _stateRevision + ")");

		[HarmonyPatch("TumbleSet")]
		[HarmonyPrefix]
		private static void TumbleSet_Log(PlayerTumble __instance, bool _isTumbling, bool _playerInput)
			=> Log(__instance, "TumbleSet(tumbling=" + _isTumbling + ", input=" + _playerInput + ")");

		// Token: 0x06000075 RID: 117 RVA: 0x000062FC File Offset: 0x000044FC
		private static void ReSyncTumbleState(PlayerTumble __instance)
		{
			__instance.GetField<PhotonView>("photonView").RPC("TumbleSetRPC", RpcTarget.MasterClient, new object[] { false, true, __instance.GetField<int>("tumbleStateRevision") });
		}

		// Token: 0x0400002A RID: 42
		private static bool _byLocalPlayer;

		private static float _antiTumbleGraceUntil;

		private const float AntiTumbleGraceSeconds = 1.5f;
	}
}
