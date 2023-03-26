using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using static StardewValley.Menus.LoadGameMenu;

namespace FashionSense.Framework.Patches.Menus
{
    internal class SaveFileSlotPatch : PatchTemplate
    {
        private readonly Type _menu = typeof(SaveFileSlot);

        internal SaveFileSlotPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Constructor(_menu, new[] { typeof(LoadGameMenu), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(SaveFileSlotPostfix)));
        }

        private static void SaveFileSlotPostfix(SaveFileSlot __instance, LoadGameMenu menu, Farmer farmer)
        {
            // Handle the loading cached accessories
            FashionSense.LoadCachedAccessories(farmer);
        }
    }
}
