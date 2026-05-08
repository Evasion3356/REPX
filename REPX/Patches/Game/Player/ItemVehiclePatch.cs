using HarmonyLib;
using REPX.Helpers;

namespace REPX.Patches.Game.Player
{
    [HarmonyPatch(typeof(ItemVehicle))]
    internal class ItemVehiclePatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void Awake_Postfix(ItemVehicle __instance)
        {
            MonoHelper.CatchedItemVehicles.Add(__instance);
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy_Postfix(ItemVehicle __instance)
        {
            MonoHelper.CatchedItemVehicles.Remove(__instance);
        }
    }
}
