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

            harmony.Patch(AccessTools.Method(_menu, nameof(CharacterCustomization.performHoverAction), new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(PerformHoverAction)));
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

            ___leftSelectionButtons.Add(new ClickableTextureComponent("fashion_sense", new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 280 - 48 + IClickableMenu.borderWidth, ___leftSelectionButtons.First(b => b.name == "Acc").bounds.Y, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 516,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            ___labels.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 280 - 48 + IClickableMenu.borderWidth + 64 + 8, ___accLabel.bounds.Y, 1, 1), _helper.Translation.Get("ui.fashion_sense.title")));
            ___rightSelectionButtons.Add(new ClickableTextureComponent("fashion_sense", new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 280 - 48 + IClickableMenu.borderWidth + 200, ___leftSelectionButtons.First(b => b.name == "Acc").bounds.Y, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 517,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
        }

        private static void SelectionClickPostfix(CharacterCustomization __instance, string name, int change)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            switch (name)
            {
                case "fashion_sense":
                    {
                        List<HairContentPack> hairModels = FashionSense.textureManager.GetAllAppearanceModels<HairContentPack>();
                        var currentCustomHair = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID]);

                        int current_index = -1;
                        if (currentCustomHair != null)
                        {
                            current_index = hairModels.IndexOf(currentCustomHair);
                        }
                        current_index += change;
                        if (current_index >= hairModels.Count)
                        {
                            current_index = -1;
                        }
                        else if (current_index < -1)
                        {
                            current_index = hairModels.Count() - 1;
                        }

                        Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID] = current_index == -1 ? "None" : hairModels[current_index].Id;
                        FashionSense.ResetAnimationModDataFields(Game1.player, 0, AnimationModel.Type.Idle, Game1.player.facingDirection);
                        Game1.playSound("grassyStep");
                        break;
                    }
            }
        }

        private static void PerformHoverAction(CharacterCustomization __instance, List<ClickableComponent> ___labels, ref string ___hoverText, int x, int y)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            foreach (ClickableComponent label in ___labels)
            {
                if (label.name == HandMirrorMenu.GetColorPickerLabel(true, true) && label.containsPoint(x, y))
                {
                    ___hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.hair_color_info");
                }
            }
        }

        private static void DrawPostfix(CharacterCustomization __instance, List<ClickableComponent> ___labels, string ___hoverText, SpriteBatch b)
        {
            if (__instance.source != Source.NewGame)
            {
                return;
            }

            // Get the custom hair object, if it exists
            var currentCustomHair = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID]);

            // Draw labels
            foreach (ClickableComponent c in ___labels)
            {
                if (!c.visible)
                {
                    continue;
                }
                string sub = "";
                float offset = 0f;
                float subYOffset = 0f;
                Color color = Game1.textColor;
                if (c.name == _helper.Translation.Get("ui.fashion_sense.title"))
                {
                    offset = Game1.smallFont.MeasureString(c.name).X / 2f - 20;
                    if (!c.name.Contains("Color"))
                    {
                        sub = "None";
                        if (currentCustomHair != null)
                        {
                            sub = currentCustomHair.Name;
                        }
                    }
                }
                else if (c.name == HandMirrorMenu.GetColorPickerLabel(false, true) || c.name == HandMirrorMenu.GetColorPickerLabel(true, true))
                {
                    var name = HandMirrorMenu.GetColorPickerLabel(false, true);
                    if (currentCustomHair != null && currentCustomHair.GetHairFromFacingDirection(Game1.player.facingDirection) is HairModel model && model != null && model.DisableGrayscale)
                    {
                        name = HandMirrorMenu.GetColorPickerLabel(true, true);
                    }

                    var measuredStringSize = Game1.smallFont.MeasureString(name);
                    c.bounds.Width = (int)measuredStringSize.X;
                    c.bounds.Height = (int)measuredStringSize.Y;
                    c.name = name;
                }
                else
                {
                    color = Game1.textColor;
                }

                if (sub.Length > 0)
                {
                    Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2(((float)(c.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f) + offset, (float)(c.bounds.Y + 32) + subYOffset), color);
                }
            }
        }
    }
}
