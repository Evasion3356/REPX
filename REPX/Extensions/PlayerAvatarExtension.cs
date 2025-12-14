using System;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

namespace REPX.Extensions
{
	internal static class PlayerAvatarExtension
	{
		internal static bool IsLocalPlayer(this PlayerAvatar pa)
		{
			return pa.GetField<bool>("isLocal");
		}

		internal static bool IsDead(this PlayerAvatar pa)
		{
			return pa.GetField<bool>("deadSet");
		}

		internal static string GetPlayerName(this PlayerAvatar pa)
		{
			return pa.GetField<string>("playerName");
		}

		internal static string GetSteamId(this PlayerAvatar pa)
		{
			return pa.GetField<string>("steamID");
		}

		internal static void TeleportExploit(this PlayerAvatar pa, Vector3 pos, Quaternion? rot = null, RpcTarget target = RpcTarget.All)
		{
			Quaternion quaternion = rot.GetValueOrDefault();
			if (rot == null)
			{
				quaternion = pa.transform.rotation;
				rot = new Quaternion?(quaternion);
			}
			Quaternion value = rot.Value;
			bool flag = SemiFunc.IsMultiplayer();
			if (flag)
			{
				pa.photonView.RPC("SpawnRPC", target, new object[] { pos, value });
			}
			else
			{
				pa.Spawn(pos, value);
			}
		}

		internal static void OutroExploit(this PlayerAvatar pa)
		{
			bool flag = SemiFunc.IsMultiplayer();
			if (flag)
			{
				pa.photonView.RPC("OutroStartRPC", pa.photonView.Owner, Array.Empty<object>());
			}
			else
			{
				pa.OutroStartRPC();
			}
		}

		internal static void SetDisabledExploit(this PlayerAvatar pa)
		{
			bool flag = SemiFunc.IsMultiplayer();
			if (flag)
			{
				pa.photonView.RPC("SetDisabledRPC", RpcTarget.All, Array.Empty<object>());
			}
			else
			{
				pa.SetField("isDisabled", true);
			}
		}

		internal static void SetNameExploit(this PlayerAvatar pa, string name = null, string steamId = null)
		{
			if (name == null)
			{
				name = pa.GetPlayerName();
			}
			if (steamId == null)
			{
				steamId = pa.GetSteamId();
			}
			pa.photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, new object[] { name, steamId });
		}
	}
}
