using HarmonyLib;
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

namespace LivelyHair.Framework.Patches.Entities
{
    internal class FarmerRendererPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(FarmerRenderer);

        internal FarmerRendererPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.drawHairAndAccesories), new[] { typeof(SpriteBatch), typeof(int), typeof(Farmer), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(float), typeof(Color), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
        }

        private static bool DrawPrefix(FarmerRenderer __instance, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            if (!who.modData.ContainsKey("LivelyHair.CustomHair.Id") || who.FacingDirection != 2)
            {
                return true;
            }

            var appearanceModel = LivelyHair.textureManager.GetSpecificAppearanceModel(who.modData["LivelyHair.CustomHair.Id"]);
            if (appearanceModel is null)
            {
                return true;
            }

            Rectangle sourceRect = new Rectangle(0, 0, appearanceModel.FrontHair.HairSize.Width, appearanceModel.FrontHair.HairSize.Length);
            if (appearanceModel.FrontHair.HasIdleAnimation())
            {
                if (!who.modData.ContainsKey("LivelyHair.Animation.FrameIndex") || !who.modData.ContainsKey("LivelyHair.Animation.FrameDuration") || !who.modData.ContainsKey("LivelyHair.Animation.ElapsedDuration"))
                {
                    who.modData["LivelyHair.Animation.FrameIndex"] = "0";
                    who.modData["LivelyHair.Animation.FrameDuration"] = appearanceModel.FrontHair.GetIdleAnimationAtFrame(0).Duration.ToString();
                    who.modData["LivelyHair.Animation.ElapsedDuration"] = "0";
                }

                var frameIndex = Int32.Parse(who.modData["LivelyHair.Animation.FrameIndex"]);
                var frameDuration = Int32.Parse(who.modData["LivelyHair.Animation.FrameDuration"]);
                var elapsedDuration = Int32.Parse(who.modData["LivelyHair.Animation.ElapsedDuration"]);

                if (elapsedDuration >= frameDuration)
                {
                    frameIndex = frameIndex + 1 >= appearanceModel.FrontHair.IdleAnimation.Count() ? 0 : frameIndex + 1;

                    who.modData["LivelyHair.Animation.FrameIndex"] = frameIndex.ToString();
                    who.modData["LivelyHair.Animation.FrameDuration"] = appearanceModel.FrontHair.GetIdleAnimationAtFrame(frameIndex).Duration.ToString();
                    who.modData["LivelyHair.Animation.ElapsedDuration"] = "0";
                }
                else
                {
                    who.modData["LivelyHair.Animation.ElapsedDuration"] = (elapsedDuration + Game1.currentGameTime.ElapsedGameTime.Milliseconds).ToString();
                }

                sourceRect.X += sourceRect.Width * frameIndex;
            }

            int hair_style = who.getHair();
            HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
            if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
            {
                hair_style = hair_metadata.coveredIndex;
                hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
            }
            //__instance.executeRecolorActions(who);

            if ((int)who.accessory >= 0)
            {
                ___accessorySourceRect = new Rectangle((int)who.accessory * 16 % FarmerRenderer.accessoriesTexture.Width, (int)who.accessory * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16);
            }
            if (who.hat.Value != null)
            {
                ___hatSourceRect = new Rectangle(20 * (int)who.hat.Value.which % FarmerRenderer.hatsTexture.Width, 20 * (int)who.hat.Value.which / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20);
            }
            ___shirtSourceRect = new Rectangle(__instance.ClampShirt(who.GetShirtIndex()) * 8 % 128, __instance.ClampShirt(who.GetShirtIndex()) * 8 / 128 * 32, 8, 8);

            Rectangle dyed_shirt_source_rect = ___shirtSourceRect;
            float dye_layer_offset = 1E-07f;
            float hair_draw_layer = 2.2E-05f;

            dyed_shirt_source_rect = ___shirtSourceRect;
            dyed_shirt_source_rect.Offset(128, 0);
            if (!who.bathingClothes)
            {
                b.Draw(FarmerRenderer.shirtsTexture, position + origin + ___positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)__instance.heightOffset * scale - (float)(who.IsMale ? 0 : 0)), ___shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f);
                b.Draw(FarmerRenderer.shirtsTexture, position + origin + ___positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)__instance.heightOffset * scale - (float)(who.IsMale ? 0 : 0)), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f + dye_layer_offset);
            }
            if ((int)who.accessory >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + origin + ___positionOffset + ___rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)__instance.heightOffset - 4), ___accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory < 8) ? 1.9E-05f : 2.9E-05f));
            }
            b.Draw(appearanceModel.Texture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), sourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), 4f * scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);

            if (who.hat.Value != null && !who.bathingClothes)
            {
                bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
                float layer_offset = 3.9E-05f;
                if (who.hat.Value.isMask && facingDirection == 0)
                {
                    Rectangle mask_draw_rect = ___hatSourceRect;
                    mask_draw_rect.Height -= 11;
                    mask_draw_rect.Y += 11;
                    b.Draw(FarmerRenderer.hatsTexture, position + origin + ___positionOffset + new Vector2(0f, 44f) + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)__instance.heightOffset), mask_draw_rect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                    mask_draw_rect = ___hatSourceRect;
                    mask_draw_rect.Height = 11;
                    layer_offset = -1E-06f;
                    b.Draw(FarmerRenderer.hatsTexture, position + origin + ___positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)__instance.heightOffset), mask_draw_rect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                }
                else
                {
                    b.Draw(FarmerRenderer.hatsTexture, position + origin + ___positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)__instance.heightOffset), ___hatSourceRect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                }
            }
            return false;
        }
    }
}
