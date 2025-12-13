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

		[HarmonyPatch("FixedUpdate")]
		[HarmonyPostfix]
		private static void FixedUpdate_Postfix(PlayerAvatar __instance)
		{
			bool flag = __instance.IsLocalPlayer();
			if (flag)
			{
				bool b_FakePing = Settings.Instance.SettingsData.b_FakePing;
				if (b_FakePing)
				{
					__instance.SetField("playerPing", Settings.Instance.SettingsData.i_FakePingNum);
				}
			}
		}

		[HarmonyPatch("OnPhotonSerializeView")]
		[HarmonyPrefix]
		private static bool OnPhotonSerializeView_Prefix(PlayerAvatar __instance, PhotonStream stream)
		{
			bool flag = !__instance.IsLocalPlayer();
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				bool isWriting = stream.IsWriting;
				if (isWriting)
				{
					stream.SendNext(__instance.GetField<bool>("isCrouching"));
					stream.SendNext(__instance.GetField<bool>("isSprinting"));
					stream.SendNext(__instance.GetField<bool>("isCrawling"));
					stream.SendNext(__instance.GetField<bool>("isSliding"));
					stream.SendNext(__instance.GetField<bool>("isMoving"));
					stream.SendNext(__instance.GetField<bool>("isGrounded"));
					stream.SendNext(__instance.GetField<bool>("Interact"));
					stream.SendNext(__instance.GetField<Vector3>("InputDirection"));
					stream.SendNext(PlayerController.instance.VelocityRelative);
					stream.SendNext(__instance.GetField<Vector3>("rbVelocityRaw"));
					bool flag3 = !Settings.Instance.SettingsData.b_Invisibility;
					if (flag3)
					{
						stream.SendNext(PlayerController.instance.transform.position);
						stream.SendNext(PlayerController.instance.transform.rotation);
						stream.SendNext(__instance.GetField<Vector3>("localCameraPosition"));
						stream.SendNext(__instance.GetField<Quaternion>("localCameraRotation"));
					}
					else
					{
						stream.SendNext(new Vector3(0f, 100000f, 0f));
						stream.SendNext(default(Quaternion));
						stream.SendNext(new Vector3(0f, 100000f, 0f));
						stream.SendNext(default(Quaternion));
					}
					stream.SendNext(PlayerController.instance.CollisionGrounded.physRiding);
					stream.SendNext(PlayerController.instance.CollisionGrounded.physRidingID);
					stream.SendNext(PlayerController.instance.CollisionGrounded.physRidingPosition);
					stream.SendNext(__instance.flashlightLightAim.clientAimPoint);
					stream.SendNext(__instance.GetField<int>("playerPing"));
					flag2 = false;
				}
				else
				{
					flag2 = true;
				}
			}
			return flag2;
		}

		[HarmonyPatch("AddToStatsManager")]
		[HarmonyPrefix]
		private static bool AddToStatsManager_Prefix(PlayerAvatar __instance)
		{
			bool flag = !Settings.Instance.SettingsData.b_Spoofing;
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				bool field = __instance.GetField<bool>("isLocal");
				string text = ((!field) ? SemiFunc.PlayerGetName(__instance) : Settings.Instance.SettingsData.s_SpoofedName);
				string text2 = Settings.Instance.SettingsData.s_SpoofedSteamId;
				bool flag3 = GameManager.Multiplayer() && GameManager.instance.localTest;
				if (flag3)
				{
					int num = 0;
					Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
					for (int i = 0; i < playerList.Length; i++)
					{
						bool isLocal = playerList[i].IsLocal;
						if (isLocal)
						{
							text = text + " " + num.ToString();
							text2 += num.ToString();
						}
						num++;
					}
				}
				bool flag4 = GameManager.Multiplayer();
				if (flag4)
				{
					bool isMine = __instance.photonView.IsMine;
					if (isMine)
					{
						__instance.photonView.RPC("AddToStatsManagerRPC", RpcTarget.MasterClient, new object[] { text, text2 });
						return false;
					}
				}
				else
				{
					__instance.AddToStatsManagerRPC(text, text2);
				}
				flag2 = false;
			}
			return flag2;
		}

		[HarmonyPatch("PlayerDeath")]
		[HarmonyPrefix]
		private static bool PlayerDeath_Prefix(PlayerAvatar __instance)
		{
			bool flag = Settings.Instance.SettingsData.b_GodMode && __instance.IsLocalPlayer();
			return !flag;
		}

		[HarmonyPatch("PlayerDeathRPC")]
		[HarmonyPrefix]
		private static bool PlayerDeathRPC_Prefix(PlayerAvatar __instance)
		{
			bool flag = Settings.Instance.SettingsData.b_GodMode && __instance.IsLocalPlayer();
			bool flag3;
			if (flag)
			{
				bool flag2 = SemiFunc.IsMultiplayer();
				if (flag2)
				{
					__instance.photonView.RPC("ReviveRPC", RpcTarget.MasterClient, new object[] { false });
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

		private static bool _posNeedsToUpdate;
	}
}
