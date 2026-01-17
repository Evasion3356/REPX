using HarmonyLib;
using REPX.Data;
using REPX.Helpers;

namespace REPX.Patches.Game
{
	[HarmonyPatch(typeof(DataDirector))]
	internal class DataDirectorPatch
    {
		[HarmonyPatch("SaveDeleteCheck")]
		[HarmonyPrefix]
		public static bool SaveDeleteCheckPatch(bool _leaveGame)
		{
			return Settings.Instance.SettingsData.b_deleteSaves;
		}
	}
}
