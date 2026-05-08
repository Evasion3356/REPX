using HarmonyLib;
using REPX.Data;

namespace REPX.Patches.Game
{
    [HarmonyPatch(typeof(MenuElementCosmeticButton))]
    internal class MenuElementCosmeticButtonPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start_Postfix(MenuElementCosmeticButton __instance)
        {
            if (!Settings.Instance.SettingsData.b_UnlockAllCosmetics)
                return;

            MenuButton menuButton = Traverse.Create(__instance).Field("menuButton").GetValue<MenuButton>();
            if (menuButton == null)
                return;

            Traverse menuButtonTraverse = Traverse.Create(menuButton);
            if (!menuButtonTraverse.Field("disabled").GetValue<bool>())
                return;

            menuButtonTraverse.Field("disabled").SetValue(false);

            bool hasBeenEquipped = Traverse.Create(__instance).Field("hasBeenEquipped").GetValue<bool>();
            if (!hasBeenEquipped)
            {
                Traverse.Create(__instance).Field("hasBeenEquipped").SetValue(true);
            }

            Traverse.Create(__instance).Method("UpdateIcon", new object[] { true }).GetValue();
        }
    }
}
