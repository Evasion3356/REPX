using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using REPX.Data;
using REPX.Extensions;
using REPX.Helpers;
using REPX.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

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

		// Stores a direct reference to the closest extraction point (excluding index 0)
		internal static ExtractionPoint ClosestExtractionPoint { get; private set; } = null;

		// Flag to track if we've already calculated for this level
		private static bool hasCalculatedForCurrentLevel = false;

		[HarmonyPatch("Awake")]
		[HarmonyPostfix]
		private static void Start_Postfix(PlayerAvatar __instance)
		{
			__instance.gameObject.AddComponent<ExtendedPlayerData>();
			
			// Update SemiFunc cache when player entity is created (once per level)
			UpdateSemiFuncCache();

			// Reset the calculation flag when a new level loads
			hasCalculatedForCurrentLevel = false;

			// Start a coroutine to wait for level generation
			if (Cache.runIsLevel && !Cache.runIsShop)
			{
				__instance.StartCoroutine(WaitForLevelGenerationAndCalculate());
			}
		}

		private static IEnumerator WaitForLevelGenerationAndCalculate()
		{
			// Wait until the level is actually generated (without spamming logs)
			while (LevelGenerator.Instance == null || !LevelGenerator.Instance.Generated)
			{
				yield return new WaitForSeconds(0.5f);
			}

			// Additional small delay to ensure everything is initialized
			yield return new WaitForSeconds(0.5f);

			// Only calculate once per level
			if (!hasCalculatedForCurrentLevel)
			{
				ClosestExtractionPoint = CalculateClosestExtractionPointToTruck();
				if (ClosestExtractionPoint != null)
				{
					//Log.LogInfo(string.Format("Closest Extraction Point reference stored: {0}", ClosestExtractionPoint.name));
				}
				hasCalculatedForCurrentLevel = true;
			}
		}

		private static void UpdateSemiFuncCache()
		{
			Cache.isMainMenu = SemiFunc.IsMainMenu();
			Cache.runIsLobbyMenu = SemiFunc.RunIsLobbyMenu();
			Cache.runIsLobby = SemiFunc.RunIsLobby();
			Cache.runIsShop = SemiFunc.RunIsShop();
			Cache.isMultiplayer = SemiFunc.IsMultiplayer();
			Cache.runIsLevel = SemiFunc.RunIsLevel();
			/*Log.LogInfo("SemiFunc Cache Updated: " +
				$"isMainMenu={Cache.isMainMenu}, " +
				$"runIsLobbyMenu={Cache.runIsLobbyMenu}, " +
				$"runIsLobby={Cache.runIsLobby}, " +
				$"runIsShop={Cache.runIsShop}, " +
				$"isMultiplayer={Cache.isMultiplayer}, " +
				$"runIsLevel={Cache.runIsLevel}");*/

			// Don't calculate here - it's too early
			// Reset the reference when not in a level
			if (!Cache.runIsLevel || Cache.runIsShop)
			{
				ClosestExtractionPoint = null;
				hasCalculatedForCurrentLevel = false;
			}
		}

		/// <summary>
		/// Calculates which extraction point (excluding index 0) has the shortest NavMesh path to the truck.
		/// Returns a direct reference to the closest ExtractionPoint, or null if none found.
		/// </summary>
		private static ExtractionPoint CalculateClosestExtractionPointToTruck()
		{
			if (LevelGenerator.Instance == null || !LevelGenerator.Instance.Generated)
			{
				Log.LogWarning("Level not generated yet");
				return null;
			}

			// Find the truck position using the same approach as b_truckESP
			var levelPathTruck = LevelGenerator.Instance.LevelPathTruck;
			if (levelPathTruck == null)
			{
				Log.LogWarning("Truck not found in level");
				return null;
			}

			Vector3 truckPosition = levelPathTruck.transform.position;

			// Get extraction points using the same approach as b_extractionESP
			if (RoundDirector.instance == null)
			{
				Log.LogWarning("RoundDirector instance not found");
				return null;
			}

			List<GameObject> extractionPointList = RoundDirector.instance.GetField<List<GameObject>>("extractionPointList");
			if (extractionPointList == null || extractionPointList.Count <= 1)
			{
				Log.LogWarning("Not enough extraction points (need at least 2)");
				return null;
			}

			NavMeshPath path = new NavMeshPath();
			ExtractionPoint closestExtractionPoint = null;
			float shortestDistance = float.MaxValue;

			// Start from index 1 to skip the first extraction point (index 0)
			for (int i = 1; i < extractionPointList.Count; i++)
			{
				GameObject extractionPointObj = extractionPointList[i];
				
				if (extractionPointObj == null)
				{
					continue;
				}

				ExtractionPoint ep = extractionPointObj.GetComponent<ExtractionPoint>();
				if (ep == null)
				{
					continue;
				}

				Vector3 extractionPosition = extractionPointObj.transform.position;

				// Calculate NavMesh path from extraction point to truck
				if (NavMesh.CalculatePath(extractionPosition, truckPosition, NavMesh.AllAreas, path))
				{
					if (path.status != NavMeshPathStatus.PathInvalid)
					{
						// Calculate total path length by summing distances between corners
						float pathLength = 0f;
						for (int j = 0; j < path.corners.Length - 1; j++)
						{
							pathLength += Vector3.Distance(path.corners[j], path.corners[j + 1]);
						}

						// Update closest if this path is shorter
						if (pathLength < shortestDistance)
						{
							shortestDistance = pathLength;
							closestExtractionPoint = ep;
						}

						//Log.LogInfo(string.Format("Extraction Point {0}: Path distance to truck = {1:F2} units", i, pathLength));
					}
					else
					{
						Log.LogError(string.Format("Extraction Point {0}: NavMesh path is invalid", i));
					}
				}
				else
				{
					Log.LogError(string.Format("Extraction Point {0}: Could not calculate path to truck because of missing NavMesh. (Custom map?)", i));
				}
			}

			return closestExtractionPoint;
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
