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

namespace FashionSense.Framework.Patches.Renderer
{
    internal class FarmerRendererPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(FarmerRenderer);

        internal FarmerRendererPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.drawHairAndAccesories), new[] { typeof(SpriteBatch), typeof(int), typeof(Farmer), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(float), typeof(Color), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawHairAndAccesoriesPrefix)));
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.drawMiniPortrat), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DrawMiniPortratPrefix)));

            harmony.CreateReversePatcher(AccessTools.Method(_entity, "executeRecolorActions", new[] { typeof(Farmer) }), new HarmonyMethod(GetType(), nameof(ExecuteRecolorActionsReversePatch))).Patch();
        }

        private static void DrawShirt(SpriteBatch b, Rectangle shirtSourceRect, Rectangle dyed_shirt_source_rect, FarmerRenderer renderer, Farmer who, int currentFrame, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Color overrideColor)
        {
            float dye_layer_offset = 1E-07f;

            if (!who.bathingClothes)
            {
                b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale - (float)(who.IsMale ? 0 : 0)), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f);
                b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale - (float)(who.IsMale ? 0 : 0)), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f + dye_layer_offset);
            }
        }

        private static void DrawAccessory(SpriteBatch b, Rectangle accessorySourceRect, FarmerRenderer renderer, Farmer who, int currentFrame, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, Color overrideColor)
        {
            if ((int)who.accessory >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset - 4), accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory < 8) ? 1.9E-05f : 2.9E-05f));
            }
        }

        private static void DrawHat(SpriteBatch b, Rectangle hatSourceRect, FarmerRenderer renderer, Farmer who, int currentFrame, int facingDirection, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset)
        {
            if (who.hat.Value != null && !who.bathingClothes)
            {
                bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
                float layer_offset = 3.9E-05f;
                if (who.hat.Value.isMask && facingDirection == 0)
                {
                    Rectangle mask_draw_rect = hatSourceRect;
                    mask_draw_rect.Height -= 11;
                    mask_draw_rect.Y += 11;
                    b.Draw(FarmerRenderer.hatsTexture, position + origin + positionOffset + new Vector2(0f, 44f) + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)renderer.heightOffset), mask_draw_rect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                    mask_draw_rect = hatSourceRect;
                    mask_draw_rect.Height = 11;
                    layer_offset = -1E-06f;
                    b.Draw(FarmerRenderer.hatsTexture, position + origin + positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)renderer.heightOffset), mask_draw_rect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                }
                else
                {
                    b.Draw(FarmerRenderer.hatsTexture, position + origin + positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)renderer.heightOffset), hatSourceRect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                }
            }
        }

        private static bool HasRequiredModDataKeys(Farmer who)
        {
            return who.modData.ContainsKey(ModDataKeys.ANIMATION_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
        }

        private static bool IsFrameValid(AnimationModel animationModel)
        {
            bool isValid = true;
            foreach (var condition in animationModel.Conditions)
            {
                var passedCheck = false;
                if (condition.Name is Condition.Type.MovementDuration)
                {
                    passedCheck = FashionSense.movementData.IsMovingLongEnough(condition.GetParsedValue<long>());
                }
                else if (condition.Name is Condition.Type.MovementSpeed)
                {
                    passedCheck = FashionSense.movementData.IsMovingFastEnough(condition.GetParsedValue<long>());
                }
                else if (condition.Name is Condition.Type.RidingHorse)
                {
                    passedCheck = Game1.player.isRidingHorse();
                }

                // If the condition is independent and is true, then skip rest of evaluations
                if (condition.Independent && passedCheck)
                {
                    return true;
                }
                else if (isValid)
                {
                    isValid = passedCheck;
                }
            }


            return isValid;
        }
        private static void UpdatePlayerAnimationData(Farmer who, AnimationModel.Type type, List<AnimationModel> animations, int facingDirection, int iterator, int startingIndex)
        {
            who.modData[ModDataKeys.ANIMATION_ITERATOR] = iterator.ToString();
            who.modData[ModDataKeys.ANIMATION_STARTING_INDEX] = startingIndex.ToString();
            who.modData[ModDataKeys.ANIMATION_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
            who.modData[ModDataKeys.ANIMATION_ELAPSED_DURATION] = "0";
            who.modData[ModDataKeys.ANIMATION_TYPE] = type.ToString();
            who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] = facingDirection.ToString();
        }

        private static void HandleHairAnimation(Farmer who, AnimationModel.Type type, List<AnimationModel> animations, int facingDirection, ref Rectangle sourceRectangle)
        {
            if (!HasRequiredModDataKeys(who) || who.modData[ModDataKeys.ANIMATION_TYPE] != type.ToString() || who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] != facingDirection.ToString())
            {
                FashionSense.ResetAnimationModDataFields(who, animations.ElementAt(0).Duration, type, facingDirection);
            }

            var iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ITERATOR]);
            var startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_STARTING_INDEX]);
            var frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_FRAME_DURATION]);
            var elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ELAPSED_DURATION]);

            // Get AnimationModel for this index
            var animationModel = animations.ElementAt(iterator);

            // Check if frame is valid
            if (IsFrameValid(animationModel))
            {
                if (animationModel.OverrideStartingIndex && startingIndex != iterator)
                {
                    // See if this particular frame overrides the StartingIndex
                    startingIndex = iterator;
                }
            }
            else
            {
                // Frame isn't valid, get the next available StartingIndex (or set it to 0)
                foreach (var animation in animations.Take(iterator).Reverse().Where(a => a.OverrideStartingIndex && IsFrameValid(a)))
                {
                    startingIndex = animations.IndexOf(animation);
                    break;
                }

                if (startingIndex == iterator)
                {
                    startingIndex = 0;
                }

                iterator = startingIndex;
                elapsedDuration = 0;
                animationModel = animations.ElementAt(iterator);

                UpdatePlayerAnimationData(who, type, animations, facingDirection, iterator, startingIndex);
            }

            // Perform time based logic for elapsed animations
            // Note: ANIMATION_ELAPSED_DURATION is updated via UpdateTicked event
            if (elapsedDuration >= frameDuration)
            {
                iterator = iterator + 1 >= animations.Count() ? startingIndex : iterator + 1;

                UpdatePlayerAnimationData(who, type, animations, facingDirection, iterator, startingIndex);
            }

            sourceRectangle.X += sourceRectangle.Width * animationModel.Frame;
        }

        private static bool IsWaitingOnRequiredAnimation(Farmer who, HairModel hairModel)
        {
            if (hairModel.RequireAnimationToFinish && who.modData.ContainsKey(ModDataKeys.ANIMATION_ITERATOR) && Int32.Parse(who.modData[ModDataKeys.ANIMATION_ITERATOR]) != 0)
            {
                return true;
            }

            return false;
        }
        private static void OffsetSourceRectangles(Farmer who, int facingDirection, float rotation, ref Rectangle shirtSourceRect, ref Rectangle dyed_shirt_source_rect, ref Rectangle accessorySourceRect, ref Rectangle hatSourceRect, ref Vector2 rotationAdjustment)
        {
            switch (facingDirection)
            {
                case 0:
                    shirtSourceRect.Offset(0, 24);
                    //hairstyleSourceRect.Offset(0, 64);

                    dyed_shirt_source_rect = shirtSourceRect;
                    dyed_shirt_source_rect.Offset(128, 0);
                    if (who.hat.Value != null)
                    {
                        hatSourceRect.Offset(0, 60);
                    }

                    return;
                case 1:
                    shirtSourceRect.Offset(0, 8);
                    //hairstyleSourceRect.Offset(0, 32);
                    dyed_shirt_source_rect = shirtSourceRect;
                    dyed_shirt_source_rect.Offset(128, 0);

                    if ((int)who.accessory >= 0)
                    {
                        accessorySourceRect.Offset(0, 16);
                    }
                    if (who.hat.Value != null)
                    {
                        hatSourceRect.Offset(0, 20);
                    }
                    if (rotation == -(float)Math.PI / 32f)
                    {
                        rotationAdjustment.X = 6f;
                        rotationAdjustment.Y = -2f;
                    }
                    else if (rotation == (float)Math.PI / 32f)
                    {
                        rotationAdjustment.X = -6f;
                        rotationAdjustment.Y = 1f;
                    }

                    return;
                case 2:
                    dyed_shirt_source_rect = shirtSourceRect;
                    dyed_shirt_source_rect.Offset(128, 0);

                    return;
                case 3:
                    {
                        bool flip2 = true;
                        shirtSourceRect.Offset(0, 16);
                        dyed_shirt_source_rect = shirtSourceRect;
                        dyed_shirt_source_rect.Offset(128, 0);

                        if ((int)who.accessory >= 0)
                        {
                            accessorySourceRect.Offset(0, 16);
                        }

                        /*
                        if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                        {
                            flip2 = false;
                            hairstyleSourceRect.Offset(0, 96);
                        }
                        else
                        {
                            hairstyleSourceRect.Offset(0, 32);
                        }
                        */

                        if (who.hat.Value != null)
                        {
                            hatSourceRect.Offset(0, 40);
                        }
                        if (rotation == -(float)Math.PI / 32f)
                        {
                            rotationAdjustment.X = 6f;
                            rotationAdjustment.Y = -2f;
                        }
                        else if (rotation == (float)Math.PI / 32f)
                        {
                            rotationAdjustment.X = -5f;
                            rotationAdjustment.Y = 1f;
                        }

                        return;
                    }
            }
        }

        private static bool DrawHairAndAccesoriesPrefix(FarmerRenderer __instance, bool ___isDrawingForUI, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID))
            {
                return true;
            }

            var appearanceModel = FashionSense.textureManager.GetSpecificAppearanceModel(who.modData[ModDataKeys.CUSTOM_HAIR_ID]);
            if (appearanceModel is null)
            {
                return true;
            }

            HairModel hairModel = appearanceModel.GetHairFromFacingDirection(facingDirection);
            if (hairModel is null)
            {
                return true;
            }
            Rectangle sourceRect = new Rectangle(hairModel.StartingPosition.X, hairModel.StartingPosition.Y, hairModel.HairSize.Width, hairModel.HairSize.Length);

            // Handle any animation
            if (hairModel.HasMovementAnimation() && (FashionSense.movementData.IsPlayerMoving() || IsWaitingOnRequiredAnimation(who, hairModel)))
            {
                HandleHairAnimation(who, AnimationModel.Type.Moving, hairModel.MovementAnimation, facingDirection, ref sourceRect);
            }
            else if (hairModel.HasIdleAnimation() && !FashionSense.movementData.IsPlayerMoving())
            {
                HandleHairAnimation(who, AnimationModel.Type.Idle, hairModel.IdleAnimation, facingDirection, ref sourceRect);
            }

            // Execute recolor
            ExecuteRecolorActionsReversePatch(__instance, who);

            // Set the source rectangles for shirts, accessories and hats
            ___shirtSourceRect = new Rectangle(__instance.ClampShirt(who.GetShirtIndex()) * 8 % 128, __instance.ClampShirt(who.GetShirtIndex()) * 8 / 128 * 32, 8, 8);
            if ((int)who.accessory >= 0)
            {
                ___accessorySourceRect = new Rectangle((int)who.accessory * 16 % FarmerRenderer.accessoriesTexture.Width, (int)who.accessory * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16);
            }
            if (who.hat.Value != null)
            {
                ___hatSourceRect = new Rectangle(20 * (int)who.hat.Value.which % FarmerRenderer.hatsTexture.Width, 20 * (int)who.hat.Value.which / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20);
            }

            Rectangle dyed_shirt_source_rect = ___shirtSourceRect;
            dyed_shirt_source_rect = ___shirtSourceRect;
            dyed_shirt_source_rect.Offset(128, 0);

            // Offset the source rectangles for shirts, accessories and hats according to facingDirection
            OffsetSourceRectangles(who, facingDirection, rotation, ref ___shirtSourceRect, ref dyed_shirt_source_rect, ref ___accessorySourceRect, ref ___hatSourceRect, ref ___rotationAdjustment);

            // Draw the shirt and accessory
            DrawShirt(b, ___shirtSourceRect, dyed_shirt_source_rect, __instance, who, currentFrame, rotation, scale, layerDepth, position, origin, ___positionOffset, overrideColor);
            DrawAccessory(b, ___accessorySourceRect, __instance, who, currentFrame, rotation, scale, layerDepth, position, origin, ___positionOffset, ___rotationAdjustment, overrideColor);

            // Draw hair
            float hair_draw_layer = 2.2E-05f;
            var hairColor = overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor;
            if (hairModel.DisableGrayscale)
            {
                hairColor = Color.White;
            }

            // This fixes a potential issue where the hair isn't offset correctly when facing left while sitting or riding
            // Note that I was unable to pinpoint why this occurs and this should be treated as a tape and glue fix (i.e. not good)
            var offsetFix = ((who.IsSitting() || who.isRidingHorse()) && who.FacingDirection == 3 && who.yJumpOffset == 0 ? new Vector2(-8, 0) : Vector2.Zero);

            // Draw the hair
            b.Draw(appearanceModel.Texture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) - offsetFix, sourceRect, hairColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), 4f * scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);

            // Perform hat draw logic
            DrawHat(b, ___hatSourceRect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset);
            return false;
        }

        private static bool DrawMiniPortratPrefix(FarmerRenderer __instance, Texture2D ___baseTexture, SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID))
            {
                return true;
            }

            var appearanceModel = FashionSense.textureManager.GetSpecificAppearanceModel(who.modData[ModDataKeys.CUSTOM_HAIR_ID]);
            if (appearanceModel is null)
            {
                return true;
            }

            HairModel hairModel = appearanceModel.GetHairFromFacingDirection(facingDirection);
            if (hairModel is null)
            {
                return true;
            }
            Rectangle sourceRect = new Rectangle(hairModel.StartingPosition.X, hairModel.StartingPosition.Y, hairModel.HairSize.Width, hairModel.HairSize.Length);

            // Execute recolor
            ExecuteRecolorActionsReversePatch(__instance, who);

            // Get the hairs current color
            var hairColor = who.hairstyleColor.Value;
            if (hairModel.DisableGrayscale)
            {
                hairColor = Color.White;
            }

            // Get hair metadata
            int hair_style = who.getHair(ignore_hat: true);
            HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);

            // This is in the vanilla code, which for some reason is always 2 instead of relying on facingDirection's initial value
            facingDirection = 2;

            // Vanilla logic to determine player's head position (though largely useless as it always executes facingDirection == 2)
            bool flip = false;
            int yOffset = 0;
            int feature_y_offset = 0;
            switch (facingDirection)
            {
                case 0:
                    yOffset = 64;
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[12];
                    break;
                case 3:
                    if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                    {
                        yOffset = 96;
                    }
                    else
                    {
                        yOffset = 32;
                    }
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[6];
                    break;
                case 1:
                    yOffset = 32;
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[6];
                    break;
                case 2:
                    yOffset = 0;
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[0];
                    break;
            }

            // Draw the player's face, then the custom hairstyle
            b.Draw(___baseTexture, position, new Rectangle(0, yOffset, 16, who.isMale ? 15 : 16), Color.White, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            int sort_direction = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));
            b.Draw(appearanceModel.Texture, position + new Vector2(0f, feature_y_offset * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))) * scale / 4f, sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 1.1E-07f * (float)sort_direction);

            return false;
        }

        private static void ExecuteRecolorActionsReversePatch(FarmerRenderer __instance, Farmer who)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
