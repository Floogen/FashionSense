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
        }

        private static void DrawShirtVanilla(SpriteBatch b, Rectangle shirtSourceRect, Rectangle dyed_shirt_source_rect, FarmerRenderer renderer, Farmer who, int currentFrame, int facingDirection, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, Color overrideColor)
        {
            float dye_layer_offset = 1E-07f;
            switch (facingDirection)
            {
                case 0:
                    if (!who.bathingClothes)
                    {
                        b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f);
                        b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f + dye_layer_offset);
                    }
                    break;
                case 1:
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
                    if (!who.bathingClothes)
                    {
                        b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), 56f * scale + (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.8E-07f);
                        b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), 56f * scale + (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.8E-07f + dye_layer_offset);
                    }
                    break;
                case 2:
                    if (!who.bathingClothes)
                    {
                        b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale - (float)(who.IsMale ? 0 : 0)), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f);
                        b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale - (float)(who.IsMale ? 0 : 0)), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f + dye_layer_offset);
                    }
                    break;
                case 3:
                    {
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
                        if (!who.bathingClothes)
                        {
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset), shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.5E-07f);
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset), dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.5E-07f + dye_layer_offset);
                        }
                        break;
                    }
            }
        }

        private static void DrawAccessoryVanilla(SpriteBatch b, Rectangle accessorySourceRect, FarmerRenderer renderer, Farmer who, int currentFrame, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, Color overrideColor)
        {
            if ((int)who.accessory >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset - 4), accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory < 8) ? 1.9E-05f : 2.9E-05f));
            }
        }

        private static void DrawHairVanilla(SpriteBatch b, Texture2D hair_texture, Rectangle hairstyleSourceRect, FarmerRenderer renderer, Farmer who, int currentFrame, int facingDirection, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Color overrideColor)
        {
            float hair_draw_layer = 2.2E-05f;

            int hair_style = who.getHair();
            HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
            if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
            {
                hair_style = hair_metadata.coveredIndex;
                hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
            }


            hairstyleSourceRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
            if (hair_metadata != null)
            {
                hair_texture = hair_metadata.texture;
                hairstyleSourceRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
            }

            switch (facingDirection)
            {
                case 0:
                    hairstyleSourceRect.Offset(0, 64);
                    b.Draw(hair_texture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 4 + ((who.IsMale && hair_style >= 16) ? (-4) : ((!who.IsMale && hair_style < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer);
                    break;
                case 1:
                    hairstyleSourceRect.Offset(0, 32);
                    b.Draw(hair_texture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer);
                    break;
                case 2:
                    b.Draw(hair_texture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer);
                    break;
                case 3:
                    bool flip2 = true;
                    if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                    {
                        flip2 = false;
                        hairstyleSourceRect.Offset(0, 96);
                    }
                    else
                    {
                        hairstyleSourceRect.Offset(0, 32);
                    }
                    b.Draw(hair_texture, position + origin + positionOffset + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale, flip2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);
                    break;
            }
        }

        private static void DrawHatVanilla(SpriteBatch b, Rectangle hatSourceRect, FarmerRenderer renderer, Farmer who, int currentFrame, int facingDirection, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset)
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

        private static bool HasRequiredModDataKeys(AppearanceModel model, Farmer who)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    if (accessoryModel.Priority == AccessoryModel.Type.Secondary)
                    {
                        return who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
                    }
                    if (accessoryModel.Priority == AccessoryModel.Type.Tertiary)
                    {
                        return who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
                    }
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
                case HatModel hatModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
                case ShirtModel shirtModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
            }

            return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
        }

        private static bool IsFrameValid(List<AnimationModel> animations, int iterator, bool probe = false)
        {
            AnimationModel animationModel = animations.ElementAtOrDefault(iterator);
            if (animationModel is null)
            {
                return false;
            }

            bool isValid = true;
            foreach (var condition in animationModel.Conditions)
            {
                var passedCheck = false;
                if (condition.Name is Condition.Type.MovementDuration)
                {
                    passedCheck = FashionSense.conditionData.IsMovingLongEnough(condition.GetParsedValue<long>(!probe));
                }
                else if (condition.Name is Condition.Type.IsElapsedTimeMultipleOf)
                {
                    passedCheck = FashionSense.conditionData.IsElapsedTimeMultipleOf(condition, probe);
                }
                else if (condition.Name is Condition.Type.DidPreviousFrameDisplay)
                {
                    var previousAnimationModel = animations.ElementAtOrDefault(iterator - 1);
                    if (previousAnimationModel is null)
                    {
                        passedCheck = false;
                    }
                    else
                    {
                        passedCheck = (condition.GetParsedValue<bool>(!probe) && previousAnimationModel.WasDisplayed) || (!condition.GetParsedValue<bool>(!probe) && !previousAnimationModel.WasDisplayed);
                    }
                }
                else if (condition.Name is Condition.Type.MovementSpeed)
                {
                    passedCheck = FashionSense.conditionData.IsMovingFastEnough(condition.GetParsedValue<long>(!probe));
                }
                else if (condition.Name is Condition.Type.RidingHorse)
                {
                    passedCheck = Game1.player.isRidingHorse();
                }

                // If the condition is independent and is true, then skip rest of evaluations
                if (condition.Independent && passedCheck)
                {
                    isValid = true;
                    break;
                }
                else if (isValid)
                {
                    isValid = passedCheck;
                }
            }

            if (!probe)
            {
                animationModel.WasDisplayed = isValid;
            }

            return isValid;
        }
        private static void UpdatePlayerAnimationData(AppearanceModel model, Farmer who, AnimationModel.Type type, List<AnimationModel> animations, int facingDirection, int iterator, int startingIndex)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    if (accessoryModel.Priority == AccessoryModel.Type.Secondary)
                    {
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_TYPE] = type.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ITERATOR] = iterator.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_STARTING_INDEX] = startingIndex.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ELAPSED_DURATION] = "0";
                    }
                    else if (accessoryModel.Priority == AccessoryModel.Type.Tertiary)
                    {
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_TYPE] = type.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ITERATOR] = iterator.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_STARTING_INDEX] = startingIndex.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ELAPSED_DURATION] = "0";
                    }
                    else
                    {
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TYPE] = type.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_ITERATOR] = iterator.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_STARTING_INDEX] = startingIndex.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_ELAPSED_DURATION] = "0";
                    }
                    break;
                case HatModel hatModel:
                    who.modData[ModDataKeys.ANIMATION_HAT_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION] = "0";
                    break;
                case ShirtModel shirtModel:
                    who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION] = "0";
                    break;
                default:
                    who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_FRAME_DURATION] = animations.ElementAt(iterator).Duration.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION] = "0";
                    break;
            }

            who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] = facingDirection.ToString();
        }

        private static void HandleAppearanceAnimation(AppearanceModel model, Farmer who, int facingDirection, ref Rectangle sourceRectangle)
        {
            var size = new Size();
            if (model is HairModel hairModel)
            {
                size.Width = hairModel.HairSize.Width;
                size.Length = hairModel.HairSize.Length;
            }
            else if (model is AccessoryModel accessoryModel)
            {
                size.Width = accessoryModel.AccessorySize.Width;
                size.Length = accessoryModel.AccessorySize.Length;
            }
            else if (model is HatModel hatModel)
            {
                size.Width = hatModel.HatSize.Width;
                size.Length = hatModel.HatSize.Length;
            }
            else if (model is ShirtModel shirtModel)
            {
                size.Width = shirtModel.ShirtSize.Width;
                size.Length = shirtModel.ShirtSize.Length;
            }

            // Reset any cached animation data, if needd
            if (model.HasMovementAnimation() && FashionSense.conditionData.IsPlayerMoving() && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Moving))
            {
                SetAnimationType(model, who, AnimationModel.Type.Moving);
                FashionSense.ResetAnimationModDataFields(who, 0, AnimationModel.Type.Moving, facingDirection, true);
            }
            else if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving() && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Idle))
            {
                SetAnimationType(model, who, AnimationModel.Type.Idle);
                FashionSense.ResetAnimationModDataFields(who, 0, AnimationModel.Type.Idle, facingDirection, true);
            }
            else if (!model.HasMovementAnimation() && !model.HasIdleAnimation() && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Uniform))
            {
                SetAnimationType(model, who, AnimationModel.Type.Uniform);
                FashionSense.ResetAnimationModDataFields(who, 0, AnimationModel.Type.Uniform, facingDirection, true);
            }

            // Update the animations
            sourceRectangle = new Rectangle(model.StartingPosition.X, model.StartingPosition.Y, size.Width, size.Length);
            if (model.HasMovementAnimation() && (FashionSense.conditionData.IsPlayerMoving() || IsWaitingOnRequiredAnimation(who, model)))
            {
                HandleAppearanceAnimation(model, who, AnimationModel.Type.Moving, model.MovementAnimation, facingDirection, ref sourceRectangle, !FashionSense.conditionData.IsPlayerMoving() && IsWaitingOnRequiredAnimation(who, model));
            }
            else if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving())
            {
                HandleAppearanceAnimation(model, who, AnimationModel.Type.Idle, model.IdleAnimation, facingDirection, ref sourceRectangle);
            }
            else if (model.HasUniformAnimation())
            {
                HandleAppearanceAnimation(model, who, AnimationModel.Type.Uniform, model.UniformAnimation, facingDirection, ref sourceRectangle);
            }
        }

        private static bool HasCorrectAnimationTypeCached(AppearanceModel model, Farmer who, AnimationModel.Type type)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    if (accessoryModel.Priority == AccessoryModel.Type.Secondary)
                    {
                        return who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_TYPE) ? who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_TYPE] == type.ToString() : false;
                    }
                    if (accessoryModel.Priority == AccessoryModel.Type.Tertiary)
                    {
                        return who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_TYPE) ? who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_TYPE] == type.ToString() : false;
                    }
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_ACCESSORY_TYPE) ? who.modData[ModDataKeys.ANIMATION_ACCESSORY_TYPE] == type.ToString() : false;
                case HatModel hatModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_TYPE) ? who.modData[ModDataKeys.ANIMATION_HAT_TYPE] == type.ToString() : false;
                case ShirtModel shirtModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_TYPE) ? who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] == type.ToString() : false;
                default:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_TYPE) ? who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] == type.ToString() : false;
            }
        }

        private static void SetAnimationType(AppearanceModel model, Farmer who, AnimationModel.Type type)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    if (accessoryModel.Priority == AccessoryModel.Type.Secondary)
                    {
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_TYPE] = type.ToString();
                    }
                    else if (accessoryModel.Priority == AccessoryModel.Type.Tertiary)
                    {
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_TYPE] = type.ToString();
                    }
                    else
                    {
                        who.modData[ModDataKeys.ANIMATION_ACCESSORY_TYPE] = type.ToString();
                    }
                    break;
                case HatModel hatModel:
                    who.modData[ModDataKeys.ANIMATION_HAT_TYPE] = type.ToString();
                    break;
                case ShirtModel shirtModel:
                    who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] = type.ToString();
                    break;
                default:
                    who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] = type.ToString();
                    break;
            }
        }

        private static void HandleAppearanceAnimation(AppearanceModel model, Farmer who, AnimationModel.Type type, List<AnimationModel> animations, int facingDirection, ref Rectangle sourceRectangle, bool isAnimationFinishing = false)
        {
            if (!HasRequiredModDataKeys(model, who) || !HasCorrectAnimationTypeCached(model, who, type) || who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] != facingDirection.ToString())
            {
                SetAnimationType(model, who, type);
                FashionSense.ResetAnimationModDataFields(who, animations.ElementAt(0).Duration, type, facingDirection, true);
            }

            // Utilize the default modData key properties (HairModel)
            var iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_ITERATOR]);
            var startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_STARTING_INDEX]);
            var frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_FRAME_DURATION]);
            var elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION]);

            // Determine the modData keys to use based on AppearanceModel
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    if (accessoryModel.Priority == AccessoryModel.Type.Secondary)
                    {
                        iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ITERATOR]);
                        startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_STARTING_INDEX]);
                        frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_FRAME_DURATION]);
                        elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ELAPSED_DURATION]);
                    }
                    else if (accessoryModel.Priority == AccessoryModel.Type.Tertiary)
                    {
                        iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ITERATOR]);
                        startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_STARTING_INDEX]);
                        frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_FRAME_DURATION]);
                        elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ELAPSED_DURATION]);
                    }
                    else
                    {
                        iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_ITERATOR]);
                        startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_STARTING_INDEX]);
                        frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_FRAME_DURATION]);
                        elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_ACCESSORY_ELAPSED_DURATION]);
                    }
                    break;
                case HatModel hatModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION]);
                    break;
                case ShirtModel shirtModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION]);
                    break;
            }

            // Get AnimationModel for this index
            var animationModel = animations.ElementAtOrDefault(iterator) is null ? animations.ElementAtOrDefault(0) : animations.ElementAtOrDefault(iterator);

            // Check if frame is valid
            if (IsFrameValid(animations, iterator, probe: true))
            {
                if (animationModel.OverrideStartingIndex && startingIndex != iterator)
                {
                    // See if this particular frame overrides the StartingIndex
                    startingIndex = iterator;
                }
                else if (isAnimationFinishing)
                {
                    startingIndex = 0;
                }
            }
            else
            {
                // Frame isn't valid, get the next available frame starting from iterator
                var hasFoundNextFrame = false;
                foreach (var animation in animations.Skip(iterator + 1).Where(a => IsFrameValid(animations, animations.IndexOf(a), probe: true)))
                {
                    iterator = animations.IndexOf(animation);

                    if (animation.OverrideStartingIndex)
                    {
                        startingIndex = iterator;
                    }
                    elapsedDuration = 0;

                    hasFoundNextFrame = true;
                    break;
                }

                // If no frames are available from iterator onwards, then check backwards for the next available frame with OverrideStartingIndex
                if (!hasFoundNextFrame)
                {
                    foreach (var animation in animations.Take(iterator + 1).Reverse().Where(a => a.OverrideStartingIndex && IsFrameValid(animations, animations.IndexOf(a), probe: true)))
                    {
                        iterator = animations.IndexOf(animation);
                        startingIndex = iterator;
                        elapsedDuration = 0;

                        hasFoundNextFrame = true;
                        break;
                    }
                }

                // If next frame is not available, revert to the first one
                if (!hasFoundNextFrame)
                {
                    iterator = 0;
                    startingIndex = 0;
                    elapsedDuration = 0;
                }

                animationModel = animations.ElementAt(iterator);

                UpdatePlayerAnimationData(model, who, type, animations, facingDirection, iterator, startingIndex);
            }

            // Perform time based logic for elapsed animations
            // Note: ANIMATION_ELAPSED_DURATION is updated via UpdateTicked event
            if (elapsedDuration >= frameDuration)
            {
                // Force the frame's condition to evalute and update any caches
                IsFrameValid(animations, iterator);

                iterator = iterator + 1 >= animations.Count() ? startingIndex : iterator + 1;

                UpdatePlayerAnimationData(model, who, type, animations, facingDirection, iterator, startingIndex);

                animationModel.WasDisplayed = true;
                if (iterator == startingIndex)
                {
                    // Reset any cached values with the AnimationModel
                    foreach (var animation in animations)
                    {
                        animation.Reset();
                    }
                }
            }

            sourceRectangle.X += sourceRectangle.Width * animationModel.Frame;
        }

        private static bool IsWaitingOnRequiredAnimation(Farmer who, AppearanceModel model)
        {
            // Utilize the default modData key properties (HairModel)
            var iteratorKey = ModDataKeys.ANIMATION_HAIR_ITERATOR;

            // Determine the modData keys to use based on AppearanceModel
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    if (accessoryModel.Priority == AccessoryModel.Type.Secondary)
                    {
                        iteratorKey = ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ITERATOR;
                    }
                    else if (accessoryModel.Priority == AccessoryModel.Type.Tertiary)
                    {
                        iteratorKey = ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ITERATOR;
                    }
                    else
                    {
                        iteratorKey = ModDataKeys.ANIMATION_ACCESSORY_ITERATOR;
                    }
                    break;
                case HatModel hatModel:
                    iteratorKey = ModDataKeys.ANIMATION_HAT_ITERATOR;
                    break;
                case ShirtModel shirtModel:
                    iteratorKey = ModDataKeys.ANIMATION_SHIRT_ITERATOR;
                    break;
            }

            if (model.RequireAnimationToFinish && who.modData.ContainsKey(iteratorKey) && Int32.Parse(who.modData[iteratorKey]) != 0)
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

        private static Vector2 GetFeatureOffset(int facingDirection, int currentFrame, float scale, FarmerRenderer renderer, AppearanceContentPack.Type type, bool flip = false)
        {
            Vector2 offset = Vector2.Zero;
            if (type is AppearanceContentPack.Type.Hat)
            {
                return new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 4 + (int)renderer.heightOffset);
            }

            if (type is AppearanceContentPack.Type.Shirt)
            {
                switch (facingDirection)
                {
                    case 0:
                        return new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale);
                    case 1:
                        return new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), 56f * scale + (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale);
                    case 2:
                        return new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset * scale);
                    case 3:
                        return new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset);
                }
            }
            else
            {
                switch (facingDirection)
                {
                    case 0:
                        offset = new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                    case 1:
                        offset = new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                    case 2:
                        offset = new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                    case 3:
                        offset = new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (flip ? 4 : 0) + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                }
            }

            if (type is AppearanceContentPack.Type.Accessory)
            {
                switch (facingDirection)
                {
                    case 0:
                    case 1:
                        break;
                    case 2:
                    case 3:
                        offset.Y += 4;
                        break;
                }

                offset.Y += renderer.heightOffset;
            }

            return offset;
        }

        private static void DrawColorMask(SpriteBatch b, AppearanceContentPack appearancePack, AppearanceModel appearanceModel, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            Color[] data = new Color[appearancePack.Texture.Width * appearancePack.Texture.Height];
            appearancePack.Texture.GetData(data);
            Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, appearancePack.Texture.Width, appearancePack.Texture.Height);

            for (int i = 0; i < data.Length; i++)
            {
                if (!appearanceModel.IsMaskedColor(data[i]))
                {
                    data[i] = Color.Transparent;
                }
            }

            maskedTexture.SetData(data);
            b.Draw(maskedTexture, position, sourceRect, color, rotation, origin, scale, appearanceModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private static void DrawCustomAccessory(AccessoryContentPack accessoryPack, AccessoryModel accessoryModel, Rectangle customAccessorySourceRect, string colorModDataKey, FarmerRenderer renderer, SpriteBatch b, Farmer who, int facingDirection, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, float scale, int currentFrame, float rotation, float layerDepth)
        {
            var accessoryColor = new Color() { PackedValue = Game1.player.modData.ContainsKey(colorModDataKey) ? uint.Parse(Game1.player.modData[colorModDataKey]) : who.hairstyleColor.Value.PackedValue };
            if (accessoryModel.DisableGrayscale)
            {
                accessoryColor = Color.White;
            }
            else if (accessoryModel.IsPrismatic)
            {
                accessoryColor = Utility.GetPrismaticColor(speedMultiplier: accessoryModel.PrismaticAnimationSpeedMultiplier);
            }

            // Correct how the accessory is drawn according to facingDirection and AccessoryModel.DrawBehindHair
            var layerFix = facingDirection == 0 ? (accessoryModel.DrawBeforeHair ? 3.9E-05f : 2E-05f) : (accessoryModel.DrawBeforeHair ? -0.1E-05f : 2.9E-05f);
            layerFix += accessoryModel.DrawAfterPlayer ? 0.2E-05f : 0;

            b.Draw(accessoryPack.Texture, position + origin + positionOffset + rotationAdjustment + GetFeatureOffset(facingDirection, currentFrame, scale, renderer, accessoryPack.PackType), customAccessorySourceRect, accessoryModel.HasColorMask() ? Color.White : accessoryColor, rotation, origin + new Vector2(accessoryModel.HeadPosition.X, accessoryModel.HeadPosition.Y), 4f * scale + ((rotation != 0f) ? 0f : 0f), accessoryModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerFix);

            if (accessoryModel.HasColorMask())
            {
                DrawColorMask(b, accessoryPack, accessoryModel, position + origin + positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, renderer, accessoryPack.PackType), customAccessorySourceRect, accessoryColor, rotation, origin + new Vector2(accessoryModel.HeadPosition.X, accessoryModel.HeadPosition.Y), 4f * scale, layerDepth + layerFix + 0.01E-05f);
            }
        }

        private static bool DrawHairAndAccesoriesPrefix(FarmerRenderer __instance, bool ___isDrawingForUI, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID))
            {
                return true;
            }

            // Set up each AppearanceModel
            // Hair pack
            HairContentPack hairPack = null;
            HairModel hairModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]) is HairContentPack hPack && hPack != null)
            {
                hairPack = hPack;
                hairModel = hPack.GetHairFromFacingDirection(facingDirection);
            }

            // Accessory pack
            AccessoryContentPack accessoryPack = null;
            AccessoryModel accessoryModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]) is AccessoryContentPack aPack && aPack != null)
            {
                accessoryPack = aPack;
                accessoryModel = aPack.GetAccessoryFromFacingDirection(facingDirection);

                if (accessoryModel != null)
                {
                    accessoryModel.Priority = AccessoryModel.Type.Primary;
                }
            }

            AccessoryContentPack secondaryAccessoryPack = null;
            AccessoryModel secondaryAccessoryModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]) is AccessoryContentPack secAPack && secAPack != null)
            {
                secondaryAccessoryPack = secAPack;
                secondaryAccessoryModel = secAPack.GetAccessoryFromFacingDirection(facingDirection);

                if (secondaryAccessoryModel != null)
                {
                    secondaryAccessoryModel.Priority = AccessoryModel.Type.Secondary;
                }
            }

            AccessoryContentPack tertiaryAccessoryPack = null;
            AccessoryModel tertiaryAccessoryModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID]) is AccessoryContentPack triAPack && triAPack != null)
            {
                tertiaryAccessoryPack = triAPack;
                tertiaryAccessoryModel = triAPack.GetAccessoryFromFacingDirection(facingDirection);

                if (tertiaryAccessoryModel != null)
                {
                    tertiaryAccessoryModel.Priority = AccessoryModel.Type.Tertiary;
                }
            }

            // Hat pack
            HatContentPack hatPack = null;
            HatModel hatModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(who.modData[ModDataKeys.CUSTOM_HAT_ID]) is HatContentPack tPack && tPack != null)
            {
                hatPack = tPack;
                hatModel = tPack.GetHatFromFacingDirection(facingDirection);
            }

            // Short pack
            ShirtContentPack shirtPack = null;
            ShirtModel shirtModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
            {
                shirtPack = sPack;
                shirtModel = sPack.GetShirtFromFacingDirection(facingDirection);
            }

            // Check if all the models are null, if so revert back to vanilla logic
            if (hairModel is null && accessoryModel is null && secondaryAccessoryModel is null && tertiaryAccessoryModel is null && hatModel is null && shirtModel is null)
            {
                return true;
            }

            // Set up source rectangles
            Rectangle customHairSourceRect = new Rectangle();
            Rectangle customAccessorySourceRect = new Rectangle();
            Rectangle customSecondaryAccessorySourceRect = new Rectangle();
            Rectangle customTertiaryAccessorySourceRect = new Rectangle();
            Rectangle customHatSourceRect = new Rectangle();
            Rectangle customShirtSourceRect = new Rectangle();

            // Handle any animations
            if (hairModel != null)
            {
                HandleAppearanceAnimation(hairModel, who, facingDirection, ref customHairSourceRect);
            }
            if (accessoryModel != null)
            {
                HandleAppearanceAnimation(accessoryModel, who, facingDirection, ref customAccessorySourceRect);
            }
            if (secondaryAccessoryModel != null)
            {
                HandleAppearanceAnimation(secondaryAccessoryModel, who, facingDirection, ref customSecondaryAccessorySourceRect);
            }
            if (tertiaryAccessoryModel != null)
            {
                HandleAppearanceAnimation(tertiaryAccessoryModel, who, facingDirection, ref customTertiaryAccessorySourceRect);
            }
            if (hatModel != null)
            {
                HandleAppearanceAnimation(hatModel, who, facingDirection, ref customHatSourceRect);
            }
            if (shirtModel != null)
            {
                HandleAppearanceAnimation(shirtModel, who, facingDirection, ref customShirtSourceRect);
            }

            // Execute recolor
            DrawPatch.ExecuteRecolorActionsReversePatch(__instance, who);

            // Set the source rectangles for vanilla shirts, accessories and hats
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

            // Draw the shirt
            if (shirtModel is null || who.bathingClothes)
            {
                DrawShirtVanilla(b, ___shirtSourceRect, dyed_shirt_source_rect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset, ___rotationAdjustment, overrideColor);
            }
            else
            {
                float layerOffset = who.FacingDirection > 1 ? 1.5E-07f : 1.8E-07f;

                var shirtColor = new Color() { PackedValue = Game1.player.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR) ? uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR]) : who.hairstyleColor.Value.PackedValue };
                if (shirtModel.DisableGrayscale)
                {
                    shirtColor = Color.White;
                }
                else if (shirtModel.IsPrismatic)
                {
                    shirtColor = Utility.GetPrismaticColor(speedMultiplier: shirtModel.PrismaticAnimationSpeedMultiplier);
                }

                var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, shirtPack.PackType, false);
                b.Draw(shirtPack.Texture, position + origin + ___positionOffset + featureOffset, customShirtSourceRect, shirtModel.HasColorMask() ? Color.White : shirtColor, rotation, origin + new Vector2(shirtModel.BodyPosition.X, shirtModel.BodyPosition.Y), 4f * scale, shirtModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerOffset);

                if (shirtModel.HasColorMask())
                {
                    DrawColorMask(b, shirtPack, shirtModel, position + origin + ___positionOffset + featureOffset, customShirtSourceRect, shirtColor, rotation, origin + new Vector2(shirtModel.BodyPosition.X, shirtModel.BodyPosition.Y), 4f * scale, layerDepth + layerOffset + 0.01E-05f);
                }
            }

            // Draw accessory
            if (accessoryModel is null && secondaryAccessoryModel is null && tertiaryAccessoryModel is null)
            {
                DrawAccessoryVanilla(b, ___accessorySourceRect, __instance, who, currentFrame, rotation, scale, layerDepth, position, origin, ___positionOffset, ___rotationAdjustment, overrideColor);
            }
            else
            {
                if (accessoryModel != null)
                {
                    DrawCustomAccessory(accessoryPack, accessoryModel, customAccessorySourceRect, ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR, __instance, b, who, facingDirection, position, origin, ___positionOffset, ___rotationAdjustment, scale, currentFrame, rotation, layerDepth);
                }
                if (secondaryAccessoryModel != null)
                {
                    DrawCustomAccessory(secondaryAccessoryPack, secondaryAccessoryModel, customSecondaryAccessorySourceRect, ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR, __instance, b, who, facingDirection, position, origin, ___positionOffset, ___rotationAdjustment, scale, currentFrame, rotation, layerDepth + 0.01E-05f);
                }
                if (tertiaryAccessoryModel != null)
                {
                    DrawCustomAccessory(tertiaryAccessoryPack, tertiaryAccessoryModel, customTertiaryAccessorySourceRect, ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR, __instance, b, who, facingDirection, position, origin, ___positionOffset, ___rotationAdjustment, scale, currentFrame, rotation, layerDepth + 0.02E-05f);
                }
            }

            // Draw hair
            if (hairModel is null)
            {
                if (hatModel is null || !hatModel.HideHair)
                {
                    DrawHairVanilla(b, FarmerRenderer.hairStylesTexture, ___hairstyleSourceRect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset, overrideColor);
                }
            }
            else
            {
                float hair_draw_layer = 2.2E-05f;
                var hairColor = overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor;
                if (hairModel.DisableGrayscale)
                {
                    hairColor = Color.White;
                }
                else if (hairModel.IsPrismatic)
                {
                    hairColor = Utility.GetPrismaticColor(speedMultiplier: hairModel.PrismaticAnimationSpeedMultiplier);
                }

                if (hatModel is null || !hatModel.HideHair)
                {
                    var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hairPack.PackType, true);
                    featureOffset.Y -= who.isMale ? 4 : 0;

                    // Draw the hair
                    b.Draw(hairPack.Texture, position + origin + ___positionOffset + featureOffset, customHairSourceRect, hairModel.HasColorMask() ? Color.White : hairColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), 4f * scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);

                    if (hairModel.HasColorMask())
                    {
                        DrawColorMask(b, hairPack, hairModel, position + origin + ___positionOffset + featureOffset, customHairSourceRect, hairColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), 4f * scale, layerDepth + hair_draw_layer + 0.01E-05f);
                    }
                }
            }

            // Draw hat
            if (hatModel is null)
            {
                DrawHatVanilla(b, ___hatSourceRect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset);
            }
            else
            {
                var hatColor = new Color() { PackedValue = Game1.player.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_HAT_COLOR) ? uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) : who.hairstyleColor.Value.PackedValue };
                if (hatModel.DisableGrayscale)
                {
                    hatColor = Color.White;
                }
                else if (hatModel.IsPrismatic)
                {
                    hatColor = Utility.GetPrismaticColor(speedMultiplier: hatModel.PrismaticAnimationSpeedMultiplier);
                }

                bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
                float layerOffset = 3.88E-05f;
                b.Draw(hatPack.Texture, position + origin + ___positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hatPack.PackType, flip), customHatSourceRect, hatModel.HasColorMask() ? Color.White : hatColor, rotation, origin + new Vector2(hatModel.HeadPosition.X, hatModel.HeadPosition.Y), 4f * scale, hatModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerOffset);

                if (hatModel.HasColorMask())
                {
                    DrawColorMask(b, hatPack, hatModel, position + origin + ___positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hatPack.PackType, flip), customHatSourceRect, hatColor, rotation, origin + new Vector2(hatModel.HeadPosition.X, hatModel.HeadPosition.Y), 4f * scale, layerDepth + layerOffset + 0.01E-05f);
                }
            }

            return false;
        }

        private static bool DrawMiniPortratPrefix(FarmerRenderer __instance, Texture2D ___baseTexture, SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID))
            {
                return true;
            }

            var hairPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]);
            if (hairPack is null)
            {
                return true;
            }

            HairModel hairModel = hairPack.GetHairFromFacingDirection(facingDirection);
            if (hairModel is null)
            {
                return true;
            }
            Rectangle sourceRect = new Rectangle(hairModel.StartingPosition.X, hairModel.StartingPosition.Y, hairModel.HairSize.Width, hairModel.HairSize.Length);

            // Execute recolor
            DrawPatch.ExecuteRecolorActionsReversePatch(__instance, who);

            // Get the hairs current color
            var hairColor = who.hairstyleColor.Value;
            if (hairModel.DisableGrayscale)
            {
                hairColor = Color.White;
            }
            else if (hairModel.IsPrismatic)
            {
                hairColor = Utility.GetPrismaticColor(speedMultiplier: hairModel.PrismaticAnimationSpeedMultiplier);
            }

            // Get hair metadata
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
            feature_y_offset -= who.isMale ? 1 : 0;

            // Draw the player's face, then the custom hairstyle
            b.Draw(___baseTexture, position, new Rectangle(0, yOffset, 16, who.isMale ? 15 : 16), Color.White, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

            // Draw the hair
            float hair_draw_layer = 2.2E-05f;
            b.Draw(hairPack.Texture, position + new Vector2(0f, feature_y_offset * 4), sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);

            if (hairModel.HasColorMask())
            {
                DrawColorMask(b, hairPack, hairModel, position + new Vector2(0f, feature_y_offset * 4) * scale / 4f, sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), 4f * scale, layerDepth + hair_draw_layer + 0.01E-05f);
            }

            return false;
        }
    }
}
