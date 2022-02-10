using HarmonyLib;
using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Generic;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Models.Accessory;
using StardewValley.Tools;
using FashionSense.Framework.Models.Hat;
using FashionSense.Framework.Models.Shirt;
using FashionSense.Framework.Models.Pants;
using FashionSense.Framework.Models.Sleeves;

namespace FashionSense.Framework.Patches.Renderer
{
    internal class DrawPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(FarmerRenderer);

        internal DrawPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.ApplySleeveColor), new[] { typeof(string), typeof(Color[]), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(ApplySleeveColorPrefix)));
            harmony.Patch(AccessTools.Method(_entity, "ApplyShoeColor", new[] { typeof(string), typeof(Color[]) }), prefix: new HarmonyMethod(GetType(), nameof(ApplyShoeColorPrefix)));
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.draw), new[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));

            harmony.CreateReversePatcher(AccessTools.Method(_entity, "executeRecolorActions", new[] { typeof(Farmer) }), new HarmonyMethod(GetType(), nameof(ExecuteRecolorActionsReversePatch))).Patch();
            harmony.CreateReversePatcher(AccessTools.Method(_entity, "_SwapColor", new[] { typeof(string), typeof(Color[]), typeof(int), typeof(Color) }), new HarmonyMethod(GetType(), nameof(SwapColorReversePatch))).Patch();
            harmony.CreateReversePatcher(AccessTools.Method(_entity, nameof(FarmerRenderer.draw), new[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }), new HarmonyMethod(GetType(), nameof(DrawReversePatch))).Patch();
        }

        private static bool ApplyShoeColorPrefix(FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, string texture_name, Color[] pixels)
        {
            Farmer who = Game1.player;
            if (!who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR) || !who.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) || who.modData[ModDataKeys.CUSTOM_SHOES_ID] is null || who.modData[ModDataKeys.CUSTOM_SHOES_ID] == "None")
            {
                return true;
            }

            if (!uint.TryParse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR], out uint shoeColorValue))
            {
                shoeColorValue = Game1.player.hairstyleColor.Value.PackedValue;
                Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR] = shoeColorValue.ToString();
            }

            var shoeColor = new Color() { PackedValue = shoeColorValue };
            var darkestColor = new Color(57, 57, 57);
            var mediumColor = new Color(81, 81, 81);
            var lightColor = new Color(119, 119, 119);
            var lightestColor = new Color(158, 158, 158);

            SwapColorReversePatch(__instance, texture_name, pixels, 268, Utility.MultiplyColor(darkestColor, shoeColor));
            SwapColorReversePatch(__instance, texture_name, pixels, 269, Utility.MultiplyColor(mediumColor, shoeColor));
            SwapColorReversePatch(__instance, texture_name, pixels, 270, Utility.MultiplyColor(lightColor, shoeColor));
            SwapColorReversePatch(__instance, texture_name, pixels, 271, Utility.MultiplyColor(lightestColor, shoeColor));

            return false;
        }

        private static bool ApplySleeveColorPrefix(FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, string texture_name, Color[] pixels, Farmer who)
        {
            ShirtModel shirtModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
            {
                shirtModel = sPack.GetShirtFromFacingDirection(who.facingDirection);
            }

            if (shirtModel is null)
            {
                return true;
            }

            if (shirtModel.SleeveColors is null)
            {
                var skinTone = GetSkinTone(___farmerTextureManager, ___baseTexture, pixels, ___skin, ____sickFrame);

                who.hidden.Value = true;
                SwapColorReversePatch(__instance, texture_name, pixels, 256, skinTone.Darkest);
                SwapColorReversePatch(__instance, texture_name, pixels, 257, skinTone.Medium);
                SwapColorReversePatch(__instance, texture_name, pixels, 258, skinTone.Lightest);
            }
            else
            {
                var shirtColor = new Color() { PackedValue = who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR) ? uint.Parse(who.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR]) : who.hairstyleColor.Value.PackedValue };
                if (shirtModel.DisableGrayscale)
                {
                    shirtColor = Color.White;
                }
                else if (shirtModel.IsPrismatic)
                {
                    shirtColor = Utility.GetPrismaticColor(speedMultiplier: shirtModel.PrismaticAnimationSpeedMultiplier);
                }

                SwapColorReversePatch(__instance, texture_name, pixels, 256, shirtModel.IsMaskedColor(shirtModel.GetSleeveColor(0)) ? Utility.MultiplyColor(shirtColor, shirtModel.GetSleeveColor(0)) : shirtModel.GetSleeveColor(0));
                SwapColorReversePatch(__instance, texture_name, pixels, 257, shirtModel.IsMaskedColor(shirtModel.GetSleeveColor(1)) ? Utility.MultiplyColor(shirtColor, shirtModel.GetSleeveColor(1)) : shirtModel.GetSleeveColor(1));
                SwapColorReversePatch(__instance, texture_name, pixels, 258, shirtModel.IsMaskedColor(shirtModel.GetSleeveColor(2)) ? Utility.MultiplyColor(shirtColor, shirtModel.GetSleeveColor(2)) : shirtModel.GetSleeveColor(2));
            }

            return false;
        }

        [HarmonyAfter(new string[] { "aedenthorn.Swim" })]
        private static bool DrawPrefix(FarmerRenderer __instance, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            if (who.isFakeEventActor && Game1.eventUp)
            {
                who = Game1.player;
            }

            if (GetCurrentlyEquippedModels(who, facingDirection).Any(m => m is not null))
            {
                // Draw with modified SpriteSortMode method for UI to handle clipping issue
                if (FarmerRenderer.isDrawingForUI)
                {
                    b.End();
                    b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                    HandleConditionalDraw(__instance, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);

                    b.End();
                    b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                }
                else
                {
                    // Utilize standard SpriteSortMode if not using the UI
                    HandleConditionalDraw(__instance, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
                }

                return false;
            }

            return true;
        }

        private static void HandleConditionalDraw(FarmerRenderer __instance, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            ShirtContentPack shirtPack = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID))
            {
                shirtPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]);
            }

            SleevesContentPack sleevesPack = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SLEEVES_ID))
            {
                sleevesPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(who.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
            }

            PantsContentPack pantsPack = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID))
            {
                pantsPack = FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(who.modData[ModDataKeys.CUSTOM_PANTS_ID]);
            }

            // Check if we need to utilize custom draw logic
            if (GetCurrentlyEquippedModels(who, facingDirection).Count() > 0 || ShouldSleevesBeHidden(who, facingDirection))
            {
                HandleCustomDraw(__instance, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }
            else
            {
                DrawReversePatch(__instance, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }
        }

        private static void HandleCustomDraw(FarmerRenderer __instance, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            bool sick_frame = currentFrame == 104 || currentFrame == 105;
            if (____sickFrame != sick_frame)
            {
                ____sickFrame = sick_frame;
                ____shirtDirty = true;
                ____spriteDirty = true;
            }

            // Check if one of the styles
            if (ShouldUseBaldBase(who, facingDirection))
            {
                if (!__instance.textureName.Contains("_bald"))
                {
                    __instance.textureName.Set("Characters\\Farmer\\farmer_" + (who.IsMale ? "" : "girl_") + "base" + "_bald");
                }
            }
            else if (__instance.textureName.Contains("_bald"))
            {
                __instance.textureName.Set("Characters\\Farmer\\farmer_" + (who.IsMale ? "" : "girl_") + "base");
            }

            ExecuteRecolorActionsReversePatch(__instance, who);
            var baseTexture = _helper.Reflection.GetField<Texture2D>(__instance, "baseTexture").GetValue();

            position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));
            ___rotationAdjustment = Vector2.Zero;
            ___positionOffset.Y = animationFrame.positionOffset * 4;
            ___positionOffset.X = animationFrame.xOffset * 4;
            if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming)
            {
                sourceRect.Height /= 2;
                sourceRect.Height -= (int)who.yOffset / 4;
                position.Y += 64f;
            }
            if (facingDirection == 3 || facingDirection == 1)
            {
                facingDirection = ((!animationFrame.flip) ? 1 : 3);
            }

            // Get the pants model, if applicable
            PantsModel pantsModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(who.modData[ModDataKeys.CUSTOM_PANTS_ID]) is PantsContentPack pPack && pPack != null)
            {
                pantsModel = pPack.GetPantsFromFacingDirection(facingDirection);
            }

            var adjustedBaseRectangle = sourceRect;
            if (pantsModel != null && pantsModel.HideLegs && !(bool)who.swimming)
            {
                switch (who.FarmerSprite.CurrentFrame)
                {
                    case 2:
                    case 16:
                    case 54:
                    case 57:
                    case 62:
                    case 66:
                    case 84:
                    case 90:
                    case 124:
                    case 125:
                        adjustedBaseRectangle.Height -= 6;
                        break;
                    case 6:
                    case 7:
                    case 9:
                    case 19:
                    case 21:
                    case 30:
                    case 31:
                    case 32:
                    case 33:
                    case 43:
                    case 45:
                    case 55:
                    case 59:
                    case 61:
                    case 64:
                    case 68:
                    case 72:
                    case 74:
                    case 76:
                    case 94:
                    case 95:
                    case 97:
                    case 99:
                    case 105:
                        adjustedBaseRectangle.Height -= 8;
                        break;
                    case 11:
                    case 17:
                    case 20:
                    case 22:
                    case 23:
                    case 49:
                    case 50:
                    case 53:
                    case 56:
                    case 60:
                    case 69:
                    case 70:
                    case 71:
                    case 73:
                    case 75:
                    case 112:
                        adjustedBaseRectangle.Height -= 9;
                        break;
                    case 51:
                    case 106:
                        adjustedBaseRectangle.Height -= 12;
                        break;
                    case 52:
                        adjustedBaseRectangle.Height -= 11;
                        break;
                    case 77:
                        adjustedBaseRectangle.Height -= 10;
                        break;
                    case 107:
                    case 113:
                        adjustedBaseRectangle.Height -= 14;
                        break;
                    case 117:
                        adjustedBaseRectangle.Height -= 13;
                        break;
                    default:
                        adjustedBaseRectangle.Height -= 7;
                        break;
                }

                if (who.isMale)
                {
                    adjustedBaseRectangle.Height -= 1;
                }
            }
            b.Draw(baseTexture, position + origin + ___positionOffset, adjustedBaseRectangle, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

            if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming)
            {
                if (who.currentEyes != 0 && who.FacingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
                {
                    b.Draw(baseTexture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
                    b.Draw(baseTexture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);
                }

                __instance.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
                if (!ShouldHideWaterLine(who, facingDirection))
                {
                    b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)who.yOffset + 8, (int)position.Y - 128 + sourceRect.Height * 4 + (int)origin.Y - (int)who.yOffset, sourceRect.Width * 4 - (int)who.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth + 0.001f);
                }
                return;
            }

            // Utilize vanilla logic if no valid PantsModel is given
            if (pantsModel is null)
            {
                DrawPantsVanilla(b, sourceRect, __instance, who, animationFrame, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset, ___rotationAdjustment, overrideColor);
            }

            sourceRect.Offset(288, 0);
            FishingRod fishing_rod;
            if (who.currentEyes != 0 && facingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)) && (!who.UsingTool || (fishing_rod = who.CurrentTool as FishingRod) == null || fishing_rod.isFishing))
            {
                int x_adjustment = 5;
                x_adjustment = (animationFrame.flip ? (x_adjustment - FarmerRenderer.featureXOffsetPerFrame[currentFrame]) : (x_adjustment + FarmerRenderer.featureXOffsetPerFrame[currentFrame]));
                switch (facingDirection)
                {
                    case 1:
                        x_adjustment += 3;
                        break;
                    case 3:
                        x_adjustment++;
                        break;
                }
                x_adjustment *= 4;
                b.Draw(baseTexture, position + origin + ___positionOffset + new Vector2(x_adjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40)), new Rectangle(5, 16, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
                b.Draw(baseTexture, position + origin + ___positionOffset + new Vector2(x_adjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44)), new Rectangle(264 + ((facingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);
            }
            __instance.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);

            // Get the sleeves model, if applicable
            SleevesModel sleevesModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SLEEVES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(who.modData[ModDataKeys.CUSTOM_SLEEVES_ID]) is SleevesContentPack sleevesPack && sleevesPack != null)
            {
                sleevesModel = sleevesPack.GetSleevesFromFacingDirection(facingDirection);
            }

            // Handle the vanilla sleeve / arm drawing, if a custom sleeve model isn't given
            if (sleevesModel is null && !ShouldSleevesBeHidden(who, facingDirection))
            {
                float arm_layer_offset = 4.9E-05f;
                if (facingDirection == 0)
                {
                    arm_layer_offset = -1E-07f;
                }
                sourceRect.Offset(-288 + (animationFrame.secondaryArm ? 192 : 96), 0);

                b.Draw(baseTexture, position + origin + ___positionOffset + who.armOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + arm_layer_offset);
            }
        }

        private static bool ShouldUseBaldBase(Farmer who, int facingDirection)
        {
            // Hair pack
            HairModel hairModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]) is HairContentPack hPack && hPack != null)
            {
                hairModel = hPack.GetHairFromFacingDirection(facingDirection);
            }

            // Hat pack
            HatModel hatModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(who.modData[ModDataKeys.CUSTOM_HAT_ID]) is HatContentPack tPack && tPack != null)
            {
                hatModel = tPack.GetHatFromFacingDirection(facingDirection);
            }

            if ((hairModel is not null && hairModel.UseBaldHead) || (hatModel is not null && hatModel.UseBaldHead))
            {
                return true;
            }

            return false;
        }

        private static bool ShouldSleevesBeHidden(Farmer who, int facingDirection)
        {
            AppearanceModel[] appearances = GetCurrentlyEquippedModels(who, facingDirection);

            return FarmerRendererPatch.AreSleevesForcedHidden(appearances);
        }

        private static bool ShouldHideWaterLine(Farmer who, int facingDirection)
        {
            foreach (var model in GetCurrentlyEquippedModels(who, facingDirection).Where(m => m is not null))
            {
                if (model.HideWaterLine)
                {
                    return true;
                }
            }

            return false;
        }

        private static AppearanceModel[] GetCurrentlyEquippedModels(Farmer who, int facingDirection)
        {
            // Pants pack
            PantsModel pantsModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(who.modData[ModDataKeys.CUSTOM_PANTS_ID]) is PantsContentPack pPack && pPack != null)
            {
                pantsModel = pPack.GetPantsFromFacingDirection(facingDirection);
            }

            // Hair pack
            HairModel hairModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]) is HairContentPack hPack && hPack != null)
            {
                hairModel = hPack.GetHairFromFacingDirection(facingDirection);
            }

            // Accessory pack
            AccessoryModel accessoryModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]) is AccessoryContentPack aPack && aPack != null)
            {
                accessoryModel = aPack.GetAccessoryFromFacingDirection(facingDirection);

                if (accessoryModel != null)
                {
                    accessoryModel.Priority = AccessoryModel.Type.Primary;
                }
            }

            AccessoryModel secondaryAccessoryModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]) is AccessoryContentPack secAPack && secAPack != null)
            {
                secondaryAccessoryModel = secAPack.GetAccessoryFromFacingDirection(facingDirection);

                if (secondaryAccessoryModel != null)
                {
                    secondaryAccessoryModel.Priority = AccessoryModel.Type.Secondary;
                }
            }

            AccessoryModel tertiaryAccessoryModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID]) is AccessoryContentPack triAPack && triAPack != null)
            {
                tertiaryAccessoryModel = triAPack.GetAccessoryFromFacingDirection(facingDirection);

                if (tertiaryAccessoryModel != null)
                {
                    tertiaryAccessoryModel.Priority = AccessoryModel.Type.Tertiary;
                }
            }

            // Hat pack
            HatModel hatModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(who.modData[ModDataKeys.CUSTOM_HAT_ID]) is HatContentPack tPack && tPack != null)
            {
                hatModel = tPack.GetHatFromFacingDirection(facingDirection);
            }

            // Shirt pack
            ShirtModel shirtModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
            {
                shirtModel = sPack.GetShirtFromFacingDirection(facingDirection);
            }

            return new AppearanceModel[] { pantsModel, hairModel, accessoryModel, secondaryAccessoryModel, tertiaryAccessoryModel, hatModel, shirtModel };
        }

        private static void DrawPantsVanilla(SpriteBatch b, Rectangle sourceRect, FarmerRenderer renderer, Farmer who, FarmerSprite.AnimationFrame animationFrame, int currentFrame, int facingDirection, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, Color overrideColor)
        {
            Rectangle pants_rect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
            pants_rect.X += renderer.ClampPants(who.GetPantsIndex()) % 10 * 192;
            pants_rect.Y += renderer.ClampPants(who.GetPantsIndex()) / 10 * 688;

            if (!who.IsMale)
            {
                pants_rect.X += 96;
            }

            b.Draw(FarmerRenderer.pantsTexture, position + origin + positionOffset, pants_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + ((who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? 0.00092f : 9.2E-08f));
        }

        internal static SkinToneModel GetSkinTone(LocalizedContentManager farmerTextureManager, Texture2D baseTexture, Color[] pixels, NetInt skin, bool sickFrame)
        {
            Texture2D skinColors = farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
            Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
            int skin_index = skin.Value;

            if (skin_index < 0)
            {
                skin_index = skinColors.Height - 1;
            }
            if (skin_index > skinColors.Height - 1)
            {
                skin_index = 0;
            }

            skinColors.GetData(skinColorsData);
            Color darkest = skinColorsData[skin_index * 3 % (skinColors.Height * 3)];
            Color medium = skinColorsData[skin_index * 3 % (skinColors.Height * 3) + 1];
            Color lightest = skinColorsData[skin_index * 3 % (skinColors.Height * 3) + 2];

            if (sickFrame)
            {
                if (pixels is null)
                {
                    pixels = new Color[baseTexture.Width * baseTexture.Height];
                }
                darkest = pixels[260 + baseTexture.Width];
                medium = pixels[261 + baseTexture.Width];
                lightest = pixels[262 + baseTexture.Width];
            }

            return new SkinToneModel(lightest, medium, darkest);
        }

        internal static void ExecuteRecolorActionsReversePatch(FarmerRenderer __instance, Farmer who)
        {
            new NotImplementedException("It's a stub!");
        }
        internal static void SwapColorReversePatch(FarmerRenderer __instance, string texture_name, Color[] pixels, int color_index, Color color)
        {
            new NotImplementedException("It's a stub!");
        }

        private static void DrawReversePatch(FarmerRenderer __instance, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
