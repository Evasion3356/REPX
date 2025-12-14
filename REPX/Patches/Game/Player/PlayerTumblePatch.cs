using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Photon.Pun;
using REPX.Data;
using REPX.Extensions;

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
			bool flag = !__instance.playerAvatar.IsLocalPlayer();
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
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
		private static bool TumbleSetRPC_Prefix(PlayerTumble __instance, bool _isTumbling, bool _playerInput, PhotonMessageInfo _info)
		{
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

		// Token: 0x06000075 RID: 117 RVA: 0x000062FC File Offset: 0x000044FC
		private static void ReSyncTumbleState(PlayerTumble __instance)
		{
			__instance.GetField<PhotonView>("photonView").RPC("TumbleSetRPC", RpcTarget.MasterClient, new object[] { false, true });
		}

		// Token: 0x0400002A RID: 42
		private static bool _byLocalPlayer;
	}
}
