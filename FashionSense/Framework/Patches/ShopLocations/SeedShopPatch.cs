using HarmonyLib;
using FashionSense.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace FashionSense.Framework.Patches.ShopLocations
{
    internal class SeedShopPatch : PatchTemplate
    {
        private readonly Type _object = typeof(SeedShop);

        internal SeedShopPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(SeedShop.shopStock), null), postfix: new HarmonyMethod(GetType(), nameof(AddStockPostfix)));
        }

        internal static GenericTool GetHandMirrorTool()
        {
            var handMirror = new GenericTool(_helper.Translation.Get("tools.name.hand_mirror"), _helper.Translation.Get("tools.description.hand_mirror"), -1, 6, 6);
            handMirror.modData[ModDataKeys.HAND_MIRROR_FLAG] = true.ToString();

            return handMirror;
        }

        private static void AddStockPostfix(SeedShop __instance, ref Dictionary<ISalable, int[]> __result)
        {
            __result.Add(GetHandMirrorTool(), new int[2] { 1500, 1 });
        }
    }
}
