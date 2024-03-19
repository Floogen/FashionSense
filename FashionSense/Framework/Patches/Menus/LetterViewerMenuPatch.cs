using FashionSense.Framework.Patches.ShopLocations;
using FashionSense.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

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

            harmony.Patch(AccessTools.Method(_menu, nameof(LetterViewerMenu.receiveLeftClick), new[] { typeof(int), typeof(int), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(ReceiveLeftClickPrefix)));
        }

        private static void LetterViewerMenuPostfix(LetterViewerMenu __instance, string mail, string mailTitle, bool fromCollection = false)
        {
            if (mailTitle.Equals(ModDataKeys.LETTER_HAND_MIRROR, StringComparison.OrdinalIgnoreCase) && fromCollection is false)
            {
                __instance.itemsToGrab.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + __instance.width / 2 - 48, __instance.yPositionOnScreen + __instance.height - 32 - 96, 96, 96), ShopBuilderPatch.GetHandMirrorTool())
                {
                    myID = 104,
                    leftNeighborID = 101,
                    rightNeighborID = 102
                });

                _monitor.Log("Added the Hand Mirror to the LetterViewerMenu", LogLevel.Trace);
            }
        }

        [HarmonyPriority(Priority.High)]
        private static bool ReceiveLeftClickPrefix(LetterViewerMenu __instance, int x, int y, bool playSound = true)
        {
            if (__instance is not null && __instance.ShouldShowInteractable())
            {
                foreach (ClickableComponent c in __instance.itemsToGrab)
                {
                    if (c.containsPoint(x, y) && c.item is not null && c.item.modData.ContainsKey(ModDataKeys.HAND_MIRROR_FLAG))
                    {
                        Game1.playSound("coin");
                        Game1.player.addItemToInventory(c.item);
                        c.item = null;
                    }
                }
            }

            return true;
        }
    }
}
