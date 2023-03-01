using FashionSense.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using static StardewValley.Menus.CharacterCustomization;

namespace FashionSense.Framework.Patches.Menus
{
    internal class CharacterCustomizationPatch : PatchTemplate
    {
        private readonly Type _menu = typeof(CharacterCustomization);

        internal CharacterCustomizationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_menu, "setUpPositions", null), postfix: new HarmonyMethod(GetType(), nameof(SetUpPositionsPostfix)));
            harmony.Patch(AccessTools.Method(_menu, "selectionClick", new[] { typeof(string), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(SelectionClickPostfix)));

            harmony.Patch(AccessTools.Method(_menu, nameof(CharacterCustomization.performHoverAction), new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(PerformHoverActionPostfix)));
            harmony.Patch(AccessTools.Method(_menu, nameof(CharacterCustomization.draw), new[] { typeof(SpriteBatch) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPostfix)));
        }

        private static void SetUpPositionsPostfix(CharacterCustomization __instance, List<ClickableComponent> ___labels, ClickableComponent ___accLabel, List<ClickableComponent> ___leftSelectionButtons, List<ClickableComponent> ___rightSelectionButtons)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            // Assign the required modData keys, if required
            if (!Game1.player.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID))
            {
                Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID] = null;
            }

            ___leftSelectionButtons.Add(new ClickableTextureComponent("start_with_hand_mirror", new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 280 - 48 + IClickableMenu.borderWidth, ___leftSelectionButtons.First(b => b.name == "Acc").bounds.Y, 36, 36), null, _helper.Translation.Get("ui.fashion_sense.start_with_hand_mirror"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4f)
            {
                myID = 516,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
        }

        private static void SelectionClickPostfix(CharacterCustomization __instance, string name, int change, List<ClickableComponent> ___leftSelectionButtons)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            switch (name)
            {
                case "start_with_hand_mirror":
                    {
                        var button = ___leftSelectionButtons.First(c => c.name == "start_with_hand_mirror") as ClickableTextureComponent;

                        Game1.playSound("drumkit6");
                        button.sourceRect.X = ((button.sourceRect.X == 227) ? 236 : 227);
                        if (!Game1.player.modData.ContainsKey(ModDataKeys.STARTS_WITH_HAND_MIRROR) || !bool.Parse(Game1.player.modData[ModDataKeys.STARTS_WITH_HAND_MIRROR]))
                        {
                            Game1.player.modData[ModDataKeys.STARTS_WITH_HAND_MIRROR] = true.ToString();
                        }
                        else
                        {
                            Game1.player.modData[ModDataKeys.STARTS_WITH_HAND_MIRROR] = false.ToString();
                        }
                        break;
                    }
            }
        }

        private static void PerformHoverActionPostfix(CharacterCustomization __instance, List<ClickableComponent> ___leftSelectionButtons, ref string ___hoverText, int x, int y)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            var button = ___leftSelectionButtons.FirstOrDefault(c => c.name == "start_with_hand_mirror") as ClickableTextureComponent;
            if (button is not null && button.containsPoint(x, y))
            {
                ___hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.start_with_hand_mirror.description");
            }
        }

        private static void DrawPostfix(CharacterCustomization __instance, List<ClickableComponent> ___leftSelectionButtons, string ___hoverText, SpriteBatch b)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            var button = ___leftSelectionButtons.First(c => c.name == "start_with_hand_mirror") as ClickableTextureComponent;
            Utility.drawTextWithShadow(b, button.hoverText, Game1.smallFont, new Vector2(button.bounds.X + button.bounds.Width + 8, button.bounds.Y + 8), Game1.textColor);

            if (___hoverText == FashionSense.modHelper.Translation.Get("ui.fashion_sense.start_with_hand_mirror.description"))
            {
                IClickableMenu.drawHoverText(b, Game1.parseText(___hoverText, Game1.smallFont, 256), Game1.smallFont, 0, 0, -1);
            }
        }
    }
}
