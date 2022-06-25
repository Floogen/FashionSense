using HarmonyLib;
using FashionSense.Framework.Models;
using FashionSense.Framework.UI;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static StardewValley.Menus.CharacterCustomization;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Patches.ShopLocations;

namespace FashionSense.Framework.Patches.Menus
{
    internal class LetterViewerMenuPatch : PatchTemplate
    {
        private readonly Type _menu = typeof(LetterViewerMenu);

        internal LetterViewerMenuPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Constructor(_menu, new[] { typeof(string), typeof(string), typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(LetterViewerMenuPostfix)));
        }

        private static void LetterViewerMenuPostfix(LetterViewerMenu __instance, string mail, string mailTitle, bool fromCollection = false)
        {
            if (mailTitle.Equals(ModDataKeys.LETTER_HAND_MIRROR, StringComparison.OrdinalIgnoreCase) && fromCollection is false)
            {
                __instance.itemsToGrab.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + __instance.width / 2 - 48, __instance.yPositionOnScreen + __instance.height - 32 - 96, 96, 96), SeedShopPatch.GetHandMirrorTool())
                {
                    myID = 104,
                    leftNeighborID = 101,
                    rightNeighborID = 102
                });
            }
        }
    }
}
