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
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.draw), new[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));

            harmony.CreateReversePatcher(AccessTools.Method(_entity, "executeRecolorActions", new[] { typeof(Farmer) }), new HarmonyMethod(GetType(), nameof(ExecuteRecolorActionsReversePatch))).Patch();
            harmony.CreateReversePatcher(AccessTools.Method(_entity, "_SwapColor", new[] { typeof(string), typeof(Color[]), typeof(int), typeof(Color) }), new HarmonyMethod(GetType(), nameof(SwapColorReversePatch))).Patch();
            harmony.CreateReversePatcher(AccessTools.Method(_entity, nameof(FarmerRenderer.draw), new[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }), new HarmonyMethod(GetType(), nameof(DrawReversePatch))).Patch();
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
                Texture2D skinColors = ___farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
                Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
                int skin_index = ___skin.Value;

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

                if (____sickFrame)
                {
                    darkest = pixels[260 + ___baseTexture.Width];
                    medium = pixels[261 + ___baseTexture.Width];
                    lightest = pixels[262 + ___baseTexture.Width];
                }

                SwapColorReversePatch(__instance, texture_name, pixels, 256, darkest);
                SwapColorReversePatch(__instance, texture_name, pixels, 257, medium);
                SwapColorReversePatch(__instance, texture_name, pixels, 258, lightest);
            }
            else
            {
                SwapColorReversePatch(__instance, texture_name, pixels, 256, shirtModel.GetSleeveColor(0));
                SwapColorReversePatch(__instance, texture_name, pixels, 257, shirtModel.GetSleeveColor(1));
                SwapColorReversePatch(__instance, texture_name, pixels, 258, shirtModel.GetSleeveColor(2));
            }

            return false;
        }

        private static bool DrawPrefix(FarmerRenderer __instance, Texture2D ___baseTexture, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID))
            {
                return true;
            }

            // Draw with modified SpriteSortMode method for UI to handle clipping issue
            if (FarmerRenderer.isDrawingForUI)
            {
                b.End();
                b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                HandleConditionalDraw(__instance, ___baseTexture, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);

                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            }
            else
            {
                // Utilize standard SpriteSortMode if not using the UI
                HandleConditionalDraw(__instance, ___baseTexture, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }

            return false;
        }

        private static void HandleConditionalDraw(FarmerRenderer __instance, Texture2D ___baseTexture, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            if (___baseTexture != null && (FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null))
            {
                //DrawReversePatch(__instance, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
                HandleCustomDraw(__instance, ___baseTexture, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }
            else
            {
                DrawReversePatch(__instance, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }
        }

        private static void HandleCustomDraw(FarmerRenderer __instance, Texture2D ___baseTexture, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            bool sick_frame = currentFrame == 104 || currentFrame == 105;
            if (____sickFrame != sick_frame)
            {
                ____sickFrame = sick_frame;
                ____shirtDirty = true;
                ____spriteDirty = true;
            }
            ExecuteRecolorActionsReversePatch(__instance, who);
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
            b.Draw(___baseTexture, position + origin + ___positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming)
            {
                if (who.currentEyes != 0 && who.FacingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
                {
                    b.Draw(___baseTexture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
                    b.Draw(___baseTexture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);
                }
                __instance.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
                b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)who.yOffset + 8, (int)position.Y - 128 + sourceRect.Height * 4 + (int)origin.Y - (int)who.yOffset, sourceRect.Width * 4 - (int)who.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth + 0.001f);
                return;
            }
            Rectangle pants_rect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
            pants_rect.X += __instance.ClampPants(who.GetPantsIndex()) % 10 * 192;
            pants_rect.Y += __instance.ClampPants(who.GetPantsIndex()) / 10 * 688;
            if (!who.IsMale)
            {
                pants_rect.X += 96;
            }
            b.Draw(FarmerRenderer.pantsTexture, position + origin + ___positionOffset, pants_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + ((who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? 0.00092f : 9.2E-08f));
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
                b.Draw(___baseTexture, position + origin + ___positionOffset + new Vector2(x_adjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40)), new Rectangle(5, 16, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
                b.Draw(___baseTexture, position + origin + ___positionOffset + new Vector2(x_adjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44)), new Rectangle(264 + ((facingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);
            }
            __instance.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
            float arm_layer_offset = 4.9E-05f;
            if (facingDirection == 0)
            {
                arm_layer_offset = -1E-07f;
            }
            sourceRect.Offset(-288 + (animationFrame.secondaryArm ? 192 : 96), 0);

            b.Draw(___baseTexture, position + origin + ___positionOffset + who.armOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + arm_layer_offset);
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
