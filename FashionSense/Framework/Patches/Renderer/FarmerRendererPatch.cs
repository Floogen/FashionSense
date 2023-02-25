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
using FashionSense.Framework.Models.Shoes;
using Newtonsoft.Json;
using FashionSense.Framework.Managers;
using static StardewValley.HouseRenovation;

namespace FashionSense.Framework.Patches.Renderer
{
    internal class FarmerRendererPatch : PatchTemplate
    {
        internal static bool AreColorMasksPendingRefresh = false;
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
                switch (who.facingDirection.Value)
                {
                    case 0:
                        return;
                    case 1:
                        b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset - 4), accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory < 8) ? 1.9E-05f : 2.9E-05f));
                        break;
                    case 2:
                        b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset - 4), accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory < 8) ? 1.9E-05f : 2.9E-05f));
                        break;
                    case 3:
                        b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset), accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.FlipHorizontally, layerDepth + (((int)who.accessory < 8) ? 1.9E-05f : 2.9E-05f));
                        break;
                }
            }
        }

        private static void DrawHairVanilla(SpriteBatch b, Texture2D hair_texture, Rectangle hairstyleSourceRect, FarmerRenderer renderer, Farmer who, int currentFrame, int facingDirection, float rotation, float scale, float layerDepth, Vector2 position, Vector2 origin, Vector2 positionOffset, Color overrideColor)
        {
            float hair_draw_layer = 2.1E-05f;

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
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION);
                case HatModel hatModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_FARMER_FRAME);
                case ShirtModel shirtModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_FARMER_FRAME);
                case PantsModel pantsModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_PANTS_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_PANTS_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_PANTS_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_PANTS_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_PANTS_FARMER_FRAME);
                case SleevesModel sleevesModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SLEEVES_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SLEEVES_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SLEEVES_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SLEEVES_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SLEEVES_FARMER_FRAME);
                case ShoesModel shoesModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SHOES_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHOES_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHOES_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHOES_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_SHOES_FARMER_FRAME);
            }

            return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_ITERATOR) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_FRAME_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_TYPE) && who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) && who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_FARMER_FRAME);
        }

        private static bool IsFrameValid(Farmer who, AppearanceModel model, List<AnimationModel> animations, int iterator, bool probe = false)
        {
            AnimationModel animationModel = animations.ElementAtOrDefault(iterator);
            if (animationModel is null)
            {
                return false;
            }

            // Get the farmer's FarmerSprite.currentSingleAnimation via reflection
            int currentSingleAnimation = _helper.Reflection.GetField<int>(who.FarmerSprite, "currentSingleAnimation").GetValue();

            bool isValid = AreConditionsValid(animationModel.Conditions, currentSingleAnimation, who, model, animations, iterator, probe);

            if (!probe)
            {
                animationModel.WasDisplayed = isValid;
            }

            return isValid;
        }

        private static bool AreConditionsValid(List<Condition> conditions, int currentSingleAnimation, Farmer who, AppearanceModel model, List<AnimationModel> animations, int iterator, bool probe)
        {
            bool isValid = true;
            foreach (var condition in conditions)
            {
                var passedCheck = false;
                string groupKey = $"{model.Pack.PackId}.{condition.GroupName}".ToLower();
                if (FashionSense.conditionGroups.ContainsKey(groupKey))
                {
                    passedCheck = condition.IsValid(AreConditionsValid(FashionSense.conditionGroups[groupKey].Conditions, currentSingleAnimation, who, model, animations, iterator, probe));
                }
                else if (condition.Name is Condition.Type.MovementDuration)
                {
                    passedCheck = condition.IsValid(true, FashionSense.conditionData.IsMovingLongEnough(condition.GetParsedValue<long>(!probe)));
                }
                else if (condition.Name is Condition.Type.MovementDurationLogical)
                {
                    passedCheck = condition.IsValid(FashionSense.conditionData.GetMovementDuration(who));
                }
                else if (condition.Name is Condition.Type.IsElapsedTimeMultipleOf)
                {
                    passedCheck = condition.IsValid(true, FashionSense.conditionData.IsElapsedTimeMultipleOf(condition, probe));
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

                    passedCheck = condition.IsValid(true, passedCheck);
                }
                else if (condition.Name is Condition.Type.MovementSpeed)
                {
                    passedCheck = condition.IsValid(true, FashionSense.conditionData.IsMovingFastEnough(condition.GetParsedValue<long>(!probe)));
                }
                else if (condition.Name is Condition.Type.MovementSpeedLogical)
                {
                    passedCheck = condition.IsValid(FashionSense.conditionData.GetMovementSpeed(who));
                }
                else if (condition.Name is Condition.Type.RidingHorse)
                {
                    passedCheck = condition.IsValid(who.isRidingHorse());
                }
                else if (condition.Name is Condition.Type.InventoryItemCount)
                {
                    passedCheck = condition.IsValid(FashionSense.conditionData.GetActualPlayerInventoryCount(who));
                }
                else if (condition.Name is Condition.Type.IsInventoryFull)
                {
                    passedCheck = condition.IsValid(who.isInventoryFull());
                }
                else if (condition.Name is Condition.Type.IsDarkOut)
                {
                    passedCheck = condition.IsValid(Game1.isDarkOut() || Game1.IsRainingHere(Game1.currentLocation) || (Game1.mine != null && Game1.mine.isDarkArea()));
                }
                else if (condition.Name is Condition.Type.IsRaining)
                {
                    passedCheck = condition.IsValid(Game1.IsRainingHere(Game1.currentLocation));
                }
                else if (condition.Name is Condition.Type.IsWalking)
                {
                    passedCheck = condition.IsValid(!FashionSense.conditionData.IsRunning(who));
                }
                else if (condition.Name is Condition.Type.IsRunning)
                {
                    passedCheck = condition.IsValid(FashionSense.conditionData.IsRunning(who));
                }
                else if (condition.Name is Condition.Type.IsEating)
                {
                    passedCheck = condition.IsValid(who.isEating && currentSingleAnimation == FarmerSprite.eat);
                }
                else if (condition.Name is Condition.Type.IsDrinking)
                {
                    passedCheck = condition.IsValid(who.isEating && currentSingleAnimation == FarmerSprite.drink);
                }
                else if (condition.Name is Condition.Type.IsCasting)
                {
                    passedCheck = condition.IsValid(who.CurrentTool is FishingRod fishingRod && (fishingRod.isCasting || fishingRod.isTimingCast));
                }
                else if (condition.Name is Condition.Type.IsFishing)
                {
                    passedCheck = condition.IsValid(who.CurrentTool is FishingRod fishingRod && fishingRod.isFishing);
                }
                else if (condition.Name is Condition.Type.IsNibbling)
                {
                    passedCheck = condition.IsValid(who.CurrentTool is FishingRod fishingRod && fishingRod.isNibbling);
                }
                else if (condition.Name is Condition.Type.IsReeling)
                {
                    passedCheck = condition.IsValid(who.CurrentTool is FishingRod fishingRod && fishingRod.isReeling);
                }
                else if (condition.Name is Condition.Type.IsPullingFishOutOfWater)
                {
                    passedCheck = condition.IsValid(who.CurrentTool is FishingRod fishingRod && fishingRod.pullingOutOfWater);
                }
                else if (condition.Name is Condition.Type.IsUsingHeavyTool)
                {
                    passedCheck = condition.IsValid(who.UsingTool && (who.CurrentTool is Hoe || who.CurrentTool is Pickaxe || who.CurrentTool is Axe));
                }
                else if (condition.Name is Condition.Type.ToolChargeLevel)
                {
                    passedCheck = condition.IsValid(who.toolPower) && who.UsingTool && (who.CurrentTool is Hoe || who.CurrentTool is Axe || who.CurrentTool is WateringCan);
                }
                else if (condition.Name is Condition.Type.IsUsingMilkPail)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is MilkPail);
                }
                else if (condition.Name is Condition.Type.IsUsingShears)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is Shears);
                }
                else if (condition.Name is Condition.Type.IsUsingPan)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is Pan);
                }
                else if (condition.Name is Condition.Type.IsWatering)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is WateringCan);
                }
                else if (condition.Name is Condition.Type.IsUsingScythe)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is MeleeWeapon weapon && weapon.isScythe());
                }
                else if (condition.Name is Condition.Type.IsUsingMeleeWeapon)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is MeleeWeapon weapon);
                }
                else if (condition.Name is Condition.Type.IsUsingSlingshot)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is Slingshot);
                }
                else if (condition.Name is Condition.Type.IsUsingDagger)
                {
                    passedCheck = condition.IsValid(who.UsingTool && who.CurrentTool is MeleeWeapon weapon && weapon.type.Value == MeleeWeapon.dagger);
                }
                else if (condition.Name is Condition.Type.IsHarvesting)
                {
                    passedCheck = condition.IsValid(279 + who.FacingDirection == currentSingleAnimation);
                }
                else if (condition.Name is Condition.Type.IsInMines)
                {
                    passedCheck = condition.IsValid(Game1.mine != null);
                }
                else if (condition.Name is Condition.Type.IsOutdoors)
                {
                    passedCheck = condition.IsValid(Game1.currentLocation.IsOutdoors);
                }
                else if (condition.Name is Condition.Type.HealthLevel)
                {
                    passedCheck = condition.IsValid(who.health);
                }
                else if (condition.Name is Condition.Type.StaminaLevel)
                {
                    passedCheck = condition.IsValid((long)who.stamina);
                }
                else if (condition.Name is Condition.Type.IsSitting)
                {
                    passedCheck = condition.IsValid(who.IsSitting());
                }
                else if (condition.Name is Condition.Type.IsCarrying)
                {
                    passedCheck = condition.IsValid(who.IsCarrying());
                }
                else if (condition.Name is Condition.Type.IsSwimming)
                {
                    passedCheck = condition.IsValid(who.swimming.Value);
                }
                else if (condition.Name is Condition.Type.IsInBathingSuit)
                {
                    passedCheck = condition.IsValid(who.bathingClothes.Value);
                }
                else if (condition.Name is Condition.Type.IsSick)
                {
                    passedCheck = condition.IsValid(who.FarmerSprite.CurrentFrame == 104 || who.FarmerSprite.CurrentFrame == 105);
                }
                else if (condition.Name is Condition.Type.IsPassingOut)
                {
                    passedCheck = condition.IsValid(who.FarmerSprite.isPassingOut());
                }
                else if (condition.Name is Condition.Type.CurrentFarmerFrame)
                {
                    passedCheck = condition.IsValid(who.FarmerSprite.CurrentFrame);
                }
                else if (condition.Name is Condition.Type.RandomChance)
                {
                    passedCheck = condition.IsValid(Game1.random.NextDouble());
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

            return isValid;
        }

        private static void UpdatePlayerAnimationData(AppearanceModel model, Farmer who, AnimationModel.Type type, List<AnimationModel> animations, int facingDirection, int iterator, int startingIndex)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        who.modData[FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.AnimationType, accessoryIndex)] = type.ToString();
                        who.modData[FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.Iterator, accessoryIndex)] = iterator.ToString();
                        who.modData[FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.StartingIndex, accessoryIndex)] = startingIndex.ToString();
                        who.modData[FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.FrameDuration, accessoryIndex)] = animations.ElementAt(iterator).GetDuration(true).ToString();
                        who.modData[FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.ElapsedDuration, accessoryIndex)] = "0";
                        who.modData[FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.FarmerFrame, accessoryIndex)] = who.FarmerSprite.CurrentFrame.ToString();
                    }
                    break;
                case HatModel hatModel:
                    who.modData[ModDataKeys.ANIMATION_HAT_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_FRAME_DURATION] = animations.ElementAt(iterator).GetDuration(true).ToString();
                    who.modData[ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION] = "0";
                    who.modData[ModDataKeys.ANIMATION_HAT_FARMER_FRAME] = who.FarmerSprite.CurrentFrame.ToString();
                    break;
                case ShirtModel shirtModel:
                    who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION] = animations.ElementAt(iterator).GetDuration(true).ToString();
                    who.modData[ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION] = "0";
                    who.modData[ModDataKeys.ANIMATION_SHIRT_FARMER_FRAME] = who.FarmerSprite.CurrentFrame.ToString();
                    break;
                case PantsModel pantsModel:
                    who.modData[ModDataKeys.ANIMATION_PANTS_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_PANTS_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_PANTS_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_PANTS_FRAME_DURATION] = animations.ElementAt(iterator).GetDuration(true).ToString();
                    who.modData[ModDataKeys.ANIMATION_PANTS_ELAPSED_DURATION] = "0";
                    who.modData[ModDataKeys.ANIMATION_PANTS_FARMER_FRAME] = who.FarmerSprite.CurrentFrame.ToString();
                    break;
                case SleevesModel sleevesModel:
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_FRAME_DURATION] = animations.ElementAt(iterator).GetDuration(true).ToString();
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_ELAPSED_DURATION] = "0";
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_FARMER_FRAME] = who.FarmerSprite.CurrentFrame.ToString();
                    break;
                case ShoesModel shoesModel:
                    who.modData[ModDataKeys.ANIMATION_SHOES_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHOES_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHOES_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_SHOES_FRAME_DURATION] = animations.ElementAt(iterator).GetDuration(true).ToString();
                    who.modData[ModDataKeys.ANIMATION_SHOES_ELAPSED_DURATION] = "0";
                    who.modData[ModDataKeys.ANIMATION_SHOES_FARMER_FRAME] = who.FarmerSprite.CurrentFrame.ToString();
                    break;
                default:
                    who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] = type.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_ITERATOR] = iterator.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_STARTING_INDEX] = startingIndex.ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_FRAME_DURATION] = animations.ElementAt(iterator).GetDuration(true).ToString();
                    who.modData[ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION] = "0";
                    who.modData[ModDataKeys.ANIMATION_HAIR_FARMER_FRAME] = who.FarmerSprite.CurrentFrame.ToString();
                    break;
            }
        }

        private static void HandleAppearanceAnimation(List<AppearanceModel> models, AppearanceModel model, Farmer who, int facingDirection, ref Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles, bool forceUpdate = false)
        {
            var size = new Size();
            if (model is HairModel hairModel)
            {
                size = hairModel.HairSize;
            }
            else if (model is AccessoryModel accessoryModel)
            {
                size = accessoryModel.AccessorySize;
            }
            else if (model is HatModel hatModel)
            {
                size = hatModel.HatSize;
            }
            else if (model is ShirtModel shirtModel)
            {
                size = shirtModel.ShirtSize;
            }
            else if (model is PantsModel pantsModel)
            {
                size = pantsModel.PantsSize;
            }
            else if (model is SleevesModel sleevesModel)
            {
                size = sleevesModel.SleevesSize;
            }
            else if (model is ShoesModel shoesModel)
            {
                size = shoesModel.ShoesSize;
            }

            // Skip updating the animation if the Size is null
            if (size is null)
            {
                return;
            }

            // Reset any cached animation data, if needed
            if (model.HasMovementAnimation() && FashionSense.conditionData.IsPlayerMoving() && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Moving))
            {
                SetAnimationType(model, who, AnimationModel.Type.Moving);
                FashionSense.ResetAnimationModDataFields(who, model.MovementAnimation.ElementAt(0).GetDuration(true), AnimationModel.Type.Moving, facingDirection, true, model);

                foreach (var animation in model.MovementAnimation)
                {
                    animation.Reset();
                }
            }
            else if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving() && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Idle))
            {
                SetAnimationType(model, who, AnimationModel.Type.Idle);
                FashionSense.ResetAnimationModDataFields(who, model.IdleAnimation.ElementAt(0).GetDuration(true), AnimationModel.Type.Idle, facingDirection, true, model);

                foreach (var animation in model.IdleAnimation)
                {
                    animation.Reset();
                }
            }
            else if (model.HasUniformAnimation() && !model.HasMovementAnimation() && !model.HasIdleAnimation() && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Uniform))
            {
                SetAnimationType(model, who, AnimationModel.Type.Uniform);
                FashionSense.ResetAnimationModDataFields(who, model.UniformAnimation.ElementAt(0).GetDuration(true), AnimationModel.Type.Uniform, facingDirection, true, model);

                foreach (var animation in model.UniformAnimation)
                {
                    animation.Reset();
                }
            }

            // Update the animations
            appearanceTypeToSourceRectangles[model] = new Rectangle(model.StartingPosition.X, model.StartingPosition.Y, size.Width, size.Length);
            if (model.HasMovementAnimation() && (FashionSense.conditionData.IsPlayerMoving() || IsWaitingOnRequiredAnimation(who, model)))
            {
                HandleAppearanceAnimation(models, model, who, AnimationModel.Type.Moving, model.MovementAnimation, facingDirection, ref appearanceTypeToSourceRectangles, !FashionSense.conditionData.IsPlayerMoving() && IsWaitingOnRequiredAnimation(who, model), forceUpdate);
            }
            else if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving())
            {
                HandleAppearanceAnimation(models, model, who, AnimationModel.Type.Idle, model.IdleAnimation, facingDirection, ref appearanceTypeToSourceRectangles, forceUpdate);
            }
            else if (model.HasUniformAnimation())
            {
                HandleAppearanceAnimation(models, model, who, AnimationModel.Type.Uniform, model.UniformAnimation, facingDirection, ref appearanceTypeToSourceRectangles, forceUpdate);
            }
        }

        private static bool HasCorrectAnimationTypeCached(AppearanceModel model, Farmer who, AnimationModel.Type type)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        return FashionSense.accessoryManager.GetModData(who, accessoryIndex, AccessoryManager.AnimationKey.AnimationType) == type.ToString();
                    }
                    return false;
                case HatModel hatModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAT_TYPE) ? who.modData[ModDataKeys.ANIMATION_HAT_TYPE] == type.ToString() : false;
                case ShirtModel shirtModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SHIRT_TYPE) ? who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] == type.ToString() : false;
                case PantsModel pantsModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_PANTS_TYPE) ? who.modData[ModDataKeys.ANIMATION_PANTS_TYPE] == type.ToString() : false;
                case SleevesModel sleevesModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SLEEVES_TYPE) ? who.modData[ModDataKeys.ANIMATION_SLEEVES_TYPE] == type.ToString() : false;
                case ShoesModel shoesModel:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_SHOES_TYPE) ? who.modData[ModDataKeys.ANIMATION_SHOES_TYPE] == type.ToString() : false;
                default:
                    return who.modData.ContainsKey(ModDataKeys.ANIMATION_HAIR_TYPE) ? who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] == type.ToString() : false;
            }
        }

        private static void SetAnimationType(AppearanceModel model, Farmer who, AnimationModel.Type type)
        {
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        FashionSense.accessoryManager.SetModData(who, accessoryIndex, AccessoryManager.AnimationKey.AnimationType, type.ToString());
                    }
                    break;
                case HatModel hatModel:
                    who.modData[ModDataKeys.ANIMATION_HAT_TYPE] = type.ToString();
                    break;
                case ShirtModel shirtModel:
                    who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] = type.ToString();
                    break;
                case PantsModel pantsModel:
                    who.modData[ModDataKeys.ANIMATION_PANTS_TYPE] = type.ToString();
                    break;
                case SleevesModel sleevesModel:
                    who.modData[ModDataKeys.ANIMATION_SLEEVES_TYPE] = type.ToString();
                    break;
                case ShoesModel shoesModel:
                    who.modData[ModDataKeys.ANIMATION_SHOES_TYPE] = type.ToString();
                    break;
                default:
                    who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] = type.ToString();
                    break;
            }
        }

        private static void HandleAppearanceAnimation(List<AppearanceModel> activeModels, AppearanceModel appearanceModel, Farmer who, AnimationModel.Type animationType, List<AnimationModel> animations, int facingDirection, ref Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles, bool isAnimationFinishing = false, bool forceUpdate = false)
        {
            if (!HasRequiredModDataKeys(appearanceModel, who) || !HasCorrectAnimationTypeCached(appearanceModel, who, animationType) || who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] != facingDirection.ToString())
            {
                SetAnimationType(appearanceModel, who, animationType);
                FashionSense.ResetAnimationModDataFields(who, animations.ElementAt(0).GetDuration(true), animationType, facingDirection, true, appearanceModel);
            }
            var modelPack = appearanceModel.Pack;

            // Determine the modData keys to use based on AppearanceModel
            int iterator, startingIndex, frameDuration, elapsedDuration, lastFarmerFrame;
            switch (appearanceModel)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        iterator = Int32.Parse(FashionSense.accessoryManager.GetModData(who, accessoryIndex, AccessoryManager.AnimationKey.Iterator));
                        startingIndex = Int32.Parse(FashionSense.accessoryManager.GetModData(who, accessoryIndex, AccessoryManager.AnimationKey.StartingIndex));
                        frameDuration = Int32.Parse(FashionSense.accessoryManager.GetModData(who, accessoryIndex, AccessoryManager.AnimationKey.FrameDuration));
                        elapsedDuration = Int32.Parse(FashionSense.accessoryManager.GetModData(who, accessoryIndex, AccessoryManager.AnimationKey.ElapsedDuration));
                        lastFarmerFrame = Int32.Parse(FashionSense.accessoryManager.GetModData(who, accessoryIndex, AccessoryManager.AnimationKey.FarmerFrame));
                    }
                    else
                    {
                        iterator = 0;
                        startingIndex = 0;
                        frameDuration = 0;
                        elapsedDuration = 0;
                        lastFarmerFrame = 0;
                    }
                    break;
                case HatModel hatModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION]);
                    lastFarmerFrame = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAT_FARMER_FRAME]);
                    break;
                case ShirtModel shirtModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION]);
                    lastFarmerFrame = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHIRT_FARMER_FRAME]);
                    break;
                case PantsModel pantsModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_PANTS_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_PANTS_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_PANTS_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_PANTS_ELAPSED_DURATION]);
                    lastFarmerFrame = Int32.Parse(who.modData[ModDataKeys.ANIMATION_PANTS_FARMER_FRAME]);
                    break;
                case SleevesModel sleevesModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SLEEVES_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SLEEVES_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SLEEVES_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SLEEVES_ELAPSED_DURATION]);
                    lastFarmerFrame = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SLEEVES_FARMER_FRAME]);
                    break;
                case ShoesModel shoesModel:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHOES_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHOES_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHOES_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHOES_ELAPSED_DURATION]);
                    lastFarmerFrame = Int32.Parse(who.modData[ModDataKeys.ANIMATION_SHOES_FARMER_FRAME]);
                    break;
                default:
                    iterator = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_ITERATOR]);
                    startingIndex = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_STARTING_INDEX]);
                    frameDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_FRAME_DURATION]);
                    elapsedDuration = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION]);
                    lastFarmerFrame = Int32.Parse(who.modData[ModDataKeys.ANIMATION_HAIR_FARMER_FRAME]);
                    break;
            }

            // Get AnimationModel for this index
            var animationModel = animations.ElementAtOrDefault(iterator) is null ? animations.ElementAtOrDefault(0) : animations.ElementAtOrDefault(iterator);

            // Handle animations that are syncing with other appearances 
            bool defaultToEndWhenFarmerFrameUpdates = false;
            if (appearanceModel.AppearanceSyncing.FirstOrDefault(a => a.AnimationType == animationType) is AppearanceSync appearanceSync && appearanceSync is not null)
            {
                if (HasModelOfType(activeModels, appearanceSync.TargetAppearanceType) && forceUpdate is false)
                {
                    var rectangle = appearanceTypeToSourceRectangles[appearanceModel];
                    var frameOffset = (animationModel.Frame * rectangle.Width) - rectangle.Width;
                    if (modelPack is not null && frameOffset > modelPack.Texture.Width)
                    {
                        rectangle.X += (frameOffset % modelPack.Texture.Width) - rectangle.Width;
                        rectangle.Y += (frameOffset / modelPack.Texture.Width) * rectangle.Height;
                    }
                    else
                    {
                        rectangle.X += rectangle.Width * animationModel.Frame;
                    }
                    appearanceTypeToSourceRectangles[appearanceModel] = rectangle;

                    return;
                }
                else
                {
                    defaultToEndWhenFarmerFrameUpdates = true;
                }
            }

            // Check if frame is valid
            if (IsFrameValid(who, appearanceModel, animations, iterator, probe: true))
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
                foreach (var animation in animations.Skip(iterator + 1).Where(a => IsFrameValid(who, appearanceModel, animations, animations.IndexOf(a), probe: true)))
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
                    foreach (var animation in animations.Take(iterator + 1).Reverse().Where(a => a.OverrideStartingIndex && IsFrameValid(who, appearanceModel, animations, animations.IndexOf(a), probe: true)))
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

                UpdatePlayerAnimationData(appearanceModel, who, animationType, animations, facingDirection, iterator, startingIndex);
            }

            // Update the light, if any is given
            UpdateLight(appearanceModel, animationModel, who, false);

            // Perform time based logic for elapsed animations
            // Note: ANIMATION_ELAPSED_DURATION is updated via UpdateTicked event
            if ((elapsedDuration >= frameDuration && !animationModel.EndWhenFarmerFrameUpdates) || ((animationModel.EndWhenFarmerFrameUpdates || defaultToEndWhenFarmerFrameUpdates) && who.FarmerSprite.CurrentFrame != lastFarmerFrame) || forceUpdate)
            {
                // Force the frame's condition to evalute and update any caches
                IsFrameValid(who, appearanceModel, animations, iterator);
                UpdateLight(appearanceModel, animationModel, who, true);

                // Set this frame as having been displayed
                animationModel.WasDisplayed = true;

                int nextValidIndex = GetNextValidFrame(animations, who, appearanceModel, iterator, startingIndex);
                if (nextValidIndex <= iterator)
                {
                    if (IsFrameValid(who, appearanceModel, animations, startingIndex, probe: true))
                    {
                        nextValidIndex = startingIndex;
                    }

                    // Reset any cached values with the AnimationModel
                    foreach (var animation in animations)
                    {
                        animation.Reset();
                    }
                }
                animationModel = animations.ElementAtOrDefault(nextValidIndex) is null ? animations.ElementAtOrDefault(0) : animations.ElementAtOrDefault(nextValidIndex);
                iterator = nextValidIndex;

                UpdatePlayerAnimationData(appearanceModel, who, animationType, animations, facingDirection, iterator, startingIndex);

                foreach (var model in activeModels.Where(m => m is not null))
                {
                    if (model.AppearanceSyncing.FirstOrDefault(a => a.AnimationType == animationType && a.TargetAppearanceType == appearanceModel.GetPackType()) is not null)
                    {
                        HandleAppearanceAnimation(activeModels, model, who, facingDirection, ref appearanceTypeToSourceRectangles, forceUpdate: true);
                    }
                }
            }

            var sourceRect = appearanceTypeToSourceRectangles[appearanceModel];
            var sourceOffset = (animationModel.Frame * sourceRect.Width) - sourceRect.Width;
            if (modelPack is not null && sourceOffset >= modelPack.Texture.Width)
            {
                sourceRect.X += sourceOffset % modelPack.Texture.Width;
                sourceRect.Y += (sourceOffset / modelPack.Texture.Width) * sourceRect.Height;
            }
            else
            {
                sourceRect.X += sourceRect.Width * animationModel.Frame;
            }
            appearanceTypeToSourceRectangles[appearanceModel] = sourceRect;
        }

        private static int GetNextValidFrame(List<AnimationModel> animations, Farmer who, AppearanceModel appearanceModel, int iterator, int startingIndex)
        {
            var hasFoundNextFrame = false;
            foreach (var animation in animations.Skip(iterator + 1).Where(a => IsFrameValid(who, appearanceModel, animations, animations.IndexOf(a), probe: true)))
            {
                iterator = animations.IndexOf(animation);
                hasFoundNextFrame = true;
                break;
            }

            // If no frames are available from iterator onwards, then check for the next available frame from the start
            if (hasFoundNextFrame is false)
            {
                foreach (var animation in animations.Take(iterator + 1).Where(a => IsFrameValid(who, appearanceModel, animations, animations.IndexOf(a), probe: true)))
                {
                    iterator = animations.IndexOf(animation);
                    hasFoundNextFrame = true;
                    break;
                }

                if (hasFoundNextFrame is false)
                {
                    iterator = startingIndex;
                }
            }

            return iterator;
        }

        private static bool HasModelOfType(List<AppearanceModel> models, AppearanceContentPack.Type appearanceType)
        {
            foreach (var model in models)
            {
                if (model.GetPackType() == appearanceType)
                {
                    return true;
                }
            }

            return false;
        }

        private static void UpdateLight(AppearanceModel model, AnimationModel animationModel, Farmer who, bool recalculateLight)
        {
            if (Game1.currentLocation is null)
            {
                return;
            }

            int indexOffset = 0;
            string lightIdKey = null;
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        indexOffset = 10 + accessoryIndex;
                        lightIdKey = FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.LightId, accessoryIndex);
                    }
                    break;
                case HatModel hatModel:
                    indexOffset = 4;
                    lightIdKey = ModDataKeys.ANIMATION_HAT_LIGHT_ID;
                    break;
                case ShirtModel shirtModel:
                    indexOffset = 5;
                    lightIdKey = ModDataKeys.ANIMATION_SHIRT_LIGHT_ID;
                    break;
                case PantsModel pantsModel:
                    indexOffset = 6;
                    lightIdKey = ModDataKeys.ANIMATION_PANTS_LIGHT_ID;
                    break;
                case SleevesModel sleevesModel:
                    indexOffset = 7;
                    lightIdKey = ModDataKeys.ANIMATION_SLEEVES_LIGHT_ID;
                    break;
                case ShoesModel shoesModel:
                    indexOffset = 7;
                    lightIdKey = ModDataKeys.ANIMATION_SHOES_LIGHT_ID;
                    break;
                case HairModel hairModel:
                    indexOffset = 9;
                    lightIdKey = ModDataKeys.ANIMATION_HAIR_LIGHT_ID;
                    break;
                default:
                    // Unhandled model type
                    return;
            }

            // Handle any missing key value
            if (!who.modData.ContainsKey(lightIdKey) || who.modData[lightIdKey] == "0")
            {
                who.modData[lightIdKey] = GenerateLightId(indexOffset).ToString();

                if (who.modData[lightIdKey] == "0")
                {
                    return;
                }
            }

            var lightModel = animationModel.Light;
            int lightIdentifier = Int32.Parse(who.modData[lightIdKey]);
            if (lightModel is null)
            {
                if (Game1.currentLocation.sharedLights.ContainsKey(lightIdentifier))
                {
                    Game1.currentLocation.sharedLights.Remove(lightIdentifier);
                }
                return;
            }

            // Handle updating the position and other values of the light
            if (!Game1.currentLocation.sharedLights.ContainsKey(lightIdentifier))
            {
                Game1.currentLocation.sharedLights[lightIdentifier] = new LightSource(lightModel.GetTextureSource(), who.position - new Vector2(lightModel.Position.X, lightModel.Position.Y), lightModel.GetRadius(recalculateLight), lightModel.GetColor(), LightSource.LightContext.None);
            }
            else
            {
                Game1.currentLocation.sharedLights[lightIdentifier].position.Value = who.position - new Vector2(lightModel.Position.X, lightModel.Position.Y);
                Game1.currentLocation.sharedLights[lightIdentifier].radius.Value = lightModel.GetRadius(recalculateLight);
                Game1.currentLocation.sharedLights[lightIdentifier].color.Value = lightModel.GetColor();
            }
        }

        private static int GenerateLightId(int offset)
        {
            var baseKeyId = -1 * offset * 5000;
            for (int x = 0; x < 10; x++)
            {
                if (!Game1.currentLocation.sharedLights.ContainsKey(baseKeyId + x))
                {
                    return baseKeyId + x;
                }
            }

            return 0;
        }

        internal static bool AreSleevesForcedHidden(params AppearanceModel[] models)
        {
            foreach (var model in models.Where(m => m is not null))
            {
                if (model.HideSleeves)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWaitingOnRequiredAnimation(Farmer who, AppearanceModel model)
        {
            // Utilize the default modData key properties (HairModel)
            var iteratorKey = ModDataKeys.ANIMATION_HAIR_ITERATOR;

            // Determine the modData keys to use based on AppearanceModel
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        iteratorKey = FashionSense.accessoryManager.GetModDataKey(who, AccessoryManager.AnimationKey.Iterator, accessoryIndex);
                    }
                    break;
                case HatModel hatModel:
                    iteratorKey = ModDataKeys.ANIMATION_HAT_ITERATOR;
                    break;
                case ShirtModel shirtModel:
                    iteratorKey = ModDataKeys.ANIMATION_SHIRT_ITERATOR;
                    break;
                case PantsModel pantsModel:
                    iteratorKey = ModDataKeys.ANIMATION_PANTS_ITERATOR;
                    break;
                case SleevesModel sleevesModel:
                    iteratorKey = ModDataKeys.ANIMATION_SLEEVES_ITERATOR;
                    break;
                case ShoesModel shoesModel:
                    iteratorKey = ModDataKeys.ANIMATION_SHOES_ITERATOR;
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

        private static Vector2 GetFeatureOffset(int facingDirection, int currentFrame, float scale, FarmerRenderer renderer, AppearanceModel model, bool flip = false)
        {
            Vector2 offset = Vector2.Zero;
            if (model.DisableNativeOffset)
            {
                return offset;
            }

            var type = model.GetPackType();
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
            else if (type is not AppearanceContentPack.Type.Sleeves)
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

            if (type is AppearanceContentPack.Type.Accessory or AppearanceContentPack.Type.AccessorySecondary or AppearanceContentPack.Type.AccessoryTertiary)
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

        private static Vector2 GetScaledPosition(Vector2 position, AppearanceModel model, bool isDrawingForUI)
        {
            if (isDrawingForUI)
            {
                position.Y += (4f - model.Scale) * 32;
            }

            return position;
        }

        private static void DrawColorMask(SpriteBatch b, AppearanceContentPack appearancePack, AppearanceModel appearanceModel, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (appearancePack.ColorMaskTexture is null || AreColorMasksPendingRefresh)
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
                appearancePack.ColorMaskTexture = maskedTexture;
            }

            b.Draw(appearancePack.ColorMaskTexture, position, sourceRect, color, rotation, origin, scale, appearanceModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private static void DrawSleeveColorMask(SpriteBatch b, SleevesContentPack sleevesPack, SleevesModel sleevesModel, ShirtModel shirtModel, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (sleevesPack.ShirtToneTexture is null || AreColorMasksPendingRefresh)
            {
                Color[] data = new Color[sleevesPack.Texture.Width * sleevesPack.Texture.Height];
                sleevesPack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, sleevesPack.Texture.Width, sleevesPack.Texture.Height);

                var firstSleeveColor = shirtModel.GetSleeveColor(0);
                var secondSleeveColor = shirtModel.GetSleeveColor(1);
                var thirdSleeveColor = shirtModel.GetSleeveColor(2);

                for (int i = 0; i < data.Length; i++)
                {
                    if (sleevesModel.ShirtToneMask is not null)
                    {
                        if (sleevesModel.IsShirtToneMaskColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ShirtToneMask.DarkTone is not null && data[i] == sleevesModel.ShirtToneMask.Darkest && shirtModel.HasSleeveColorAtLayer(0))
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.MediumTone is not null && data[i] == sleevesModel.ShirtToneMask.Medium && shirtModel.HasSleeveColorAtLayer(1))
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.LightTone is not null && data[i] == sleevesModel.ShirtToneMask.Lightest && shirtModel.HasSleeveColorAtLayer(2))
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                    else if (sleevesModel.ColorMasks is not null)
                    {
                        if (sleevesModel.IsMaskedColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ColorMasks.Count > 0 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[0]) && shirtModel.HasSleeveColorAtLayer(0))
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 1 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[1]) && shirtModel.HasSleeveColorAtLayer(1))
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 2 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[2]) && shirtModel.HasSleeveColorAtLayer(2))
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                }

                maskedTexture.SetData(data);
                sleevesPack.ShirtToneTexture = maskedTexture;
            }

            b.Draw(sleevesPack.ShirtToneTexture, position, sourceRect, Color.White, rotation, origin, scale, sleevesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private static Color[] GetVanillaShirtSleeveColors(Farmer who, FarmerRenderer renderer)
        {
            // Perform initial vanilla logic
            Color[] shirtData = new Color[FarmerRenderer.shirtsTexture.Bounds.Width * FarmerRenderer.shirtsTexture.Bounds.Height];
            FarmerRenderer.shirtsTexture.GetData(shirtData);
            int index = renderer.ClampShirt(who.GetShirtIndex()) * 8 / 128 * 32 * FarmerRenderer.shirtsTexture.Bounds.Width + renderer.ClampShirt(who.GetShirtIndex()) * 8 % 128 + FarmerRenderer.shirtsTexture.Width * 4;
            int dye_index = index + 128;
            Color shirtSleeveColor = Color.White;

            Color actualShirtColor = Utility.MakeCompletelyOpaque(who.GetShirtColor());
            shirtSleeveColor = shirtData[dye_index];
            Color clothes_color = actualShirtColor;
            if (shirtSleeveColor.A < byte.MaxValue)
            {
                shirtSleeveColor = shirtData[index];
                clothes_color = Color.White;
            }
            shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothes_color);

            var firstSleeveColor = shirtSleeveColor;
            shirtSleeveColor = shirtData[dye_index - FarmerRenderer.shirtsTexture.Width];
            if (shirtSleeveColor.A < byte.MaxValue)
            {
                shirtSleeveColor = shirtData[index - FarmerRenderer.shirtsTexture.Width];
                clothes_color = Color.White;
            }
            shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothes_color);

            var secondSleeveColor = shirtSleeveColor;
            shirtSleeveColor = shirtData[dye_index - FarmerRenderer.shirtsTexture.Width * 2];
            if (shirtSleeveColor.A < byte.MaxValue)
            {
                shirtSleeveColor = shirtData[index - FarmerRenderer.shirtsTexture.Width * 2];
                clothes_color = Color.White;
            }
            shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothes_color);

            var thirdSleeveColor = shirtSleeveColor;

            return new Color[] { firstSleeveColor, secondSleeveColor, thirdSleeveColor };
        }

        private static Rectangle GetSourceRectangle(AppearanceModel model, Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles)
        {
            return appearanceTypeToSourceRectangles[model];
        }

        private static void DrawSleeveColorMaskVanilla(SpriteBatch b, SleevesContentPack sleevesPack, SleevesModel sleevesModel, Farmer who, FarmerRenderer renderer, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (sleevesPack.ShirtToneTexture is null || AreColorMasksPendingRefresh)
            {
                Color[] data = new Color[sleevesPack.Texture.Width * sleevesPack.Texture.Height];
                sleevesPack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, sleevesPack.Texture.Width, sleevesPack.Texture.Height);

                var shirtSleeveColors = GetVanillaShirtSleeveColors(who, renderer);
                var firstSleeveColor = shirtSleeveColors[0];
                var secondSleeveColor = shirtSleeveColors[1];
                var thirdSleeveColor = shirtSleeveColors[2];

                for (int i = 0; i < data.Length; i++)
                {
                    if (sleevesModel.ShirtToneMask is not null)
                    {
                        if (sleevesModel.IsShirtToneMaskColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ShirtToneMask.DarkTone is not null && data[i] == sleevesModel.ShirtToneMask.Darkest)
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.MediumTone is not null && data[i] == sleevesModel.ShirtToneMask.Medium)
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.LightTone is not null && data[i] == sleevesModel.ShirtToneMask.Lightest)
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                    else if (sleevesModel.ColorMasks is not null)
                    {
                        if (sleevesModel.IsMaskedColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ColorMasks.Count > 0 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[0]))
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 1 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[1]))
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 2 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[2]))
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                }

                maskedTexture.SetData(data);
                sleevesPack.ShirtToneTexture = maskedTexture;
            }

            b.Draw(sleevesPack.ShirtToneTexture, position, sourceRect, Color.White, rotation, origin, scale, sleevesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private static void DrawSkinToneMask(SpriteBatch b, AppearanceContentPack appearancePack, AppearanceModel appearanceModel, SkinToneModel skinTone, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (appearancePack.SkinMaskTexture is null || AreColorMasksPendingRefresh)
            {
                Color[] data = new Color[appearancePack.Texture.Width * appearancePack.Texture.Height];
                appearancePack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, appearancePack.Texture.Width, appearancePack.Texture.Height);

                for (int i = 0; i < data.Length; i++)
                {
                    if (!appearanceModel.IsSkinToneMaskColor(data[i]))
                    {
                        data[i] = Color.Transparent;
                    }
                    else if (appearanceModel.SkinToneMasks is not null)
                    {
                        if (appearanceModel.SkinToneMasks.LightTone is not null && data[i] == appearanceModel.SkinToneMasks.Lightest)
                        {
                            data[i] = skinTone.Lightest;
                        }
                        else if (appearanceModel.SkinToneMasks.MediumTone is not null && data[i] == appearanceModel.SkinToneMasks.Medium)
                        {
                            data[i] = skinTone.Medium;
                        }
                        else if (appearanceModel.SkinToneMasks.DarkTone is not null && data[i] == appearanceModel.SkinToneMasks.Darkest)
                        {
                            data[i] = skinTone.Darkest;
                        }
                    }
                }

                maskedTexture.SetData(data);
                appearancePack.SkinMaskTexture = maskedTexture;
            }

            b.Draw(appearancePack.SkinMaskTexture, position, sourceRect, appearanceModel.DisableSkinGrayscale ? Color.White : color, rotation, origin, scale, appearanceModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private static void DrawCustomAccessory(AccessoryContentPack accessoryPack, AccessoryModel accessoryModel, Rectangle customAccessorySourceRect, Color accessoryColor, SkinToneModel skinTone, FarmerRenderer renderer, bool isDrawingForUI, SpriteBatch b, Farmer who, int facingDirection, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, float scale, int currentFrame, float rotation, float layerDepth)
        {
            if (accessoryModel.DisableGrayscale)
            {
                accessoryColor = Color.White;
            }
            else if (accessoryModel.IsPrismatic)
            {
                accessoryColor = Utility.GetPrismaticColor(speedMultiplier: accessoryModel.PrismaticAnimationSpeedMultiplier);
            }

            // Correct how the accessory is drawn according to facingDirection and AccessoryModel.DrawBehindHair
            var layerFix = facingDirection == 0 ? (accessoryModel.DrawBeforeHair ? 3.9E-05f : 0) : (accessoryModel.DrawBeforeHair ? -0.15E-05f : 2.9E-05f);
            if (isDrawingForUI)
            {
                layerFix = facingDirection == 0 ? (accessoryModel.DrawBeforeHair ? 3.9E-05f : 2E-05f) : (accessoryModel.DrawBeforeHair ? -0.15E-05f : 3.9E-05f);
            }

            if (accessoryModel.DrawAfterSleeves)
            {
                layerFix += 3E-05f;
            }
            else if (accessoryModel.DrawAfterPlayer)
            {
                layerFix += 1.5E-05f;
            }

            b.Draw(accessoryPack.Texture, GetScaledPosition(position, accessoryModel, isDrawingForUI) + origin + positionOffset + rotationAdjustment + GetFeatureOffset(facingDirection, currentFrame, scale, renderer, accessoryModel), customAccessorySourceRect, accessoryModel.HasColorMask() ? Color.White : accessoryColor, rotation, origin + new Vector2(accessoryModel.HeadPosition.X, accessoryModel.HeadPosition.Y), accessoryModel.Scale * scale + ((rotation != 0f) ? 0f : 0f), accessoryModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerFix);

            if (accessoryModel.HasColorMask())
            {
                DrawColorMask(b, accessoryPack, accessoryModel, GetScaledPosition(position, accessoryModel, isDrawingForUI) + origin + positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, renderer, accessoryModel), customAccessorySourceRect, accessoryColor, rotation, origin + new Vector2(accessoryModel.HeadPosition.X, accessoryModel.HeadPosition.Y), accessoryModel.Scale * scale, layerDepth + layerFix + 0.01E-05f);
            }
            if (accessoryModel.HasSkinToneMask())
            {
                DrawSkinToneMask(b, accessoryPack, accessoryModel, skinTone, GetScaledPosition(position, accessoryModel, isDrawingForUI) + origin + positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, renderer, accessoryModel), customAccessorySourceRect, accessoryColor, rotation, origin + new Vector2(accessoryModel.HeadPosition.X, accessoryModel.HeadPosition.Y), accessoryModel.Scale * scale, layerDepth + layerFix + 0.01E-05f);
            }
        }

        private static bool DrawHairAndAccesoriesPrefix(FarmerRenderer __instance, bool ___isDrawingForUI, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID))
            {
                return true;
            }

            // Set up each AppearanceModel
            List<AppearanceModel> models = new List<AppearanceModel>();

            // Pants pack
            PantsContentPack pantsPack = null;
            PantsModel pantsModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(who.modData[ModDataKeys.CUSTOM_PANTS_ID]) is PantsContentPack pPack && pPack != null)
            {
                pantsPack = pPack;
                pantsModel = pPack.GetPantsFromFacingDirection(facingDirection);

                models.Add(pantsModel);
            }

            // Hair pack
            HairContentPack hairPack = null;
            HairModel hairModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]) is HairContentPack hPack && hPack != null)
            {
                hairPack = hPack;
                hairModel = hPack.GetHairFromFacingDirection(facingDirection);

                models.Add(hairModel);
            }

            // Accessory packs
            Dictionary<AccessoryContentPack, AccessoryModel> accessoriesData = new Dictionary<AccessoryContentPack, AccessoryModel>();
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID))
            {
                try
                {
                    foreach (int index in FashionSense.accessoryManager.GetActiveAccessoryIndices(who))
                    {
                        var accessoryKey = FashionSense.accessoryManager.GetAccessoryIdByIndex(who, index);
                        if (FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(accessoryKey) is AccessoryContentPack aPack && aPack != null)
                        {
                            AccessoryModel accessoryModel = aPack.GetAccessoryFromFacingDirection(facingDirection);
                            if (accessoryModel is null)
                            {
                                continue;
                            }

                            models.Add(accessoryModel);
                            accessoriesData[aPack] = accessoryModel;
                        }
                    }
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }

            // Hat pack
            HatContentPack hatPack = null;
            HatModel hatModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(who.modData[ModDataKeys.CUSTOM_HAT_ID]) is HatContentPack tPack && tPack != null)
            {
                hatPack = tPack;
                hatModel = tPack.GetHatFromFacingDirection(facingDirection);

                models.Add(hatModel);
            }

            // Shirt pack
            ShirtContentPack shirtPack = null;
            ShirtModel shirtModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
            {
                shirtPack = sPack;
                shirtModel = sPack.GetShirtFromFacingDirection(facingDirection);

                models.Add(shirtModel);
            }

            // Sleeves pack
            SleevesContentPack sleevesPack = null;
            SleevesModel sleevesModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SLEEVES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(who.modData[ModDataKeys.CUSTOM_SLEEVES_ID]) is SleevesContentPack armPack && armPack != null)
            {
                sleevesPack = armPack;
                sleevesModel = armPack.GetSleevesFromFacingDirection(facingDirection);

                models.Add(sleevesModel);
            }

            // Shoes pack
            ShoesContentPack shoesPack = null;
            ShoesModel shoesModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(who.modData[ModDataKeys.CUSTOM_SHOES_ID]) is ShoesContentPack bootPack && bootPack != null)
            {
                // Ignore our internal color override pack
                if (bootPack.Id != ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID)
                {
                    shoesPack = bootPack;
                    shoesModel = bootPack.GetShoesFromFacingDirection(facingDirection);

                    models.Add(shoesModel);
                }
            }

            // Check if all the models are null, if so revert back to vanilla logic
            if (pantsModel is null && hairModel is null && accessoriesData.Count == 0 && hatModel is null && shirtModel is null && sleevesModel is null && shoesModel is null)
            {
                return true;
            }

            // Get skin tone
            var skinTone = DrawPatch.GetSkinTone(___farmerTextureManager, ___baseTexture, null, ___skin, ____sickFrame);

            // Set up source rectangles
            Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles = new Dictionary<AppearanceModel, Rectangle>();
            foreach (var model in models)
            {
                appearanceTypeToSourceRectangles[model] = new Rectangle();
            }

            // Handle any animations
            if (pantsModel != null)
            {
                HandleAppearanceAnimation(models, pantsModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }
            if (hairModel != null)
            {
                HandleAppearanceAnimation(models, hairModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }
            foreach (var accessoryModel in accessoriesData.Values)
            {
                HandleAppearanceAnimation(models, accessoryModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }
            if (hatModel != null)
            {
                HandleAppearanceAnimation(models, hatModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }
            if (shirtModel != null)
            {
                HandleAppearanceAnimation(models, shirtModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }
            if (sleevesModel != null)
            {
                HandleAppearanceAnimation(models, sleevesModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }
            if (shoesModel != null)
            {
                HandleAppearanceAnimation(models, shoesModel, who, facingDirection, ref appearanceTypeToSourceRectangles);
            }

            // Check if the cached facing direction needs to be updated
            if (who.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) is false || who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] != facingDirection.ToString())
            {
                who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] = facingDirection.ToString();
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

            // Prepare the layer offsets
            float sleevesLayer = 0f;
            float shirtLayer = 0f;
            float shoesLayer = 0f;
            float hairLayer = 0f;

            // Draw the pants
            if (pantsModel is null || (pantsModel.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (pantsModel.HideWhileSwimming && who.swimming.Value))
            {
                // Handled in DrawPatch.HandleCustomDraw
            }
            else
            {
                var pantsColor = new Color() { PackedValue = who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR) ? uint.Parse(who.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR]) : who.hairstyleColor.Value.PackedValue };
                if (pantsModel.DisableGrayscale)
                {
                    pantsColor = Color.White;
                }
                else if (pantsModel.IsPrismatic)
                {
                    pantsColor = Utility.GetPrismaticColor(speedMultiplier: pantsModel.PrismaticAnimationSpeedMultiplier);
                }

                var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, pantsModel, false);
                featureOffset.Y -= who.isMale ? 4 : 0;

                b.Draw(pantsPack.Texture, GetScaledPosition(position, pantsModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(pantsModel, appearanceTypeToSourceRectangles), pantsModel.HasColorMask() ? Color.White : pantsColor, rotation, origin + new Vector2(pantsModel.BodyPosition.X, pantsModel.BodyPosition.Y), pantsModel.Scale * scale, pantsModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.01E-05f);

                if (pantsModel.HasColorMask())
                {
                    DrawColorMask(b, pantsPack, pantsModel, GetScaledPosition(position, pantsModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(pantsModel, appearanceTypeToSourceRectangles), pantsColor, rotation, origin + new Vector2(pantsModel.BodyPosition.X, pantsModel.BodyPosition.Y), pantsModel.Scale * scale, layerDepth + 0.02E-05f);
                }
                if (pantsModel.HasSkinToneMask())
                {
                    DrawSkinToneMask(b, pantsPack, pantsModel, skinTone, GetScaledPosition(position, pantsModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(pantsModel, appearanceTypeToSourceRectangles), pantsColor, rotation, origin + new Vector2(pantsModel.BodyPosition.X, pantsModel.BodyPosition.Y), pantsModel.Scale * scale, layerDepth + 0.02E-05f);
                }
            }
            layerDepth += 0.03E-05f;

            // Draw the shoes
            if (shoesModel is null || (shoesModel.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (shoesModel.HideWhileSwimming && who.swimming.Value) || DrawPatch.ShouldHideLegs(who, facingDirection))
            {
                // Handled in DrawPatch.HandleCustomDraw
            }
            else
            {
                var shoesColor = new Color() { PackedValue = who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR) ? uint.Parse(who.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR]) : who.hairstyleColor.Value.PackedValue };
                if (shoesModel.DisableGrayscale)
                {
                    shoesColor = Color.White;
                }
                else if (shoesModel.IsPrismatic)
                {
                    shoesColor = Utility.GetPrismaticColor(speedMultiplier: shoesModel.PrismaticAnimationSpeedMultiplier);
                }

                // Adjust the shoesLayer according to model's adjustment properties
                shoesLayer = layerDepth;
                if (shoesModel.DrawBeforePants)
                {
                    shoesLayer = pantsModel is not null ? shoesLayer - 0.4E-05f : shoesLayer - 1.1E-05f;
                }

                var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, shoesModel, false);
                featureOffset.Y -= who.isMale ? 4 : 0;

                b.Draw(shoesPack.Texture, GetScaledPosition(position, shoesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(shoesModel, appearanceTypeToSourceRectangles), shoesModel.HasColorMask() ? Color.White : shoesColor, rotation, origin + new Vector2(shoesModel.BodyPosition.X, shoesModel.BodyPosition.Y), shoesModel.Scale * scale, shoesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, shoesLayer + 0.01E-05f);

                if (shoesModel.HasColorMask())
                {
                    DrawColorMask(b, shoesPack, shoesModel, GetScaledPosition(position, shoesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(shoesModel, appearanceTypeToSourceRectangles), shoesColor, rotation, origin + new Vector2(shoesModel.BodyPosition.X, shoesModel.BodyPosition.Y), shoesModel.Scale * scale, shoesLayer + 0.02E-05f);
                }
                if (shoesModel.HasSkinToneMask())
                {
                    DrawSkinToneMask(b, shoesPack, shoesModel, skinTone, GetScaledPosition(position, shoesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(shoesModel, appearanceTypeToSourceRectangles), shoesColor, rotation, origin + new Vector2(shoesModel.BodyPosition.X, shoesModel.BodyPosition.Y), shoesModel.Scale * scale, shoesLayer + 0.02E-05f);
                }
            }
            layerDepth += 0.03E-05f;

            // Draw the shirt
            if (shirtModel is null || (shirtModel.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (shirtModel.HideWhileSwimming && who.swimming.Value))
            {
                DrawShirtVanilla(b, ___shirtSourceRect, dyed_shirt_source_rect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset, ___rotationAdjustment, overrideColor);
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
                shirtLayer = layerDepth + 0.01E-05f;

                var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, shirtModel, false);
                b.Draw(shirtPack.Texture, GetScaledPosition(position, shirtModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(shirtModel, appearanceTypeToSourceRectangles), shirtModel.HasColorMask() ? Color.White : shirtColor, rotation, origin + new Vector2(shirtModel.BodyPosition.X, shirtModel.BodyPosition.Y), shirtModel.Scale * scale, shirtModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, shirtLayer);

                if (shirtModel.HasColorMask())
                {
                    shirtLayer += 0.01E-05f;
                    DrawColorMask(b, shirtPack, shirtModel, GetScaledPosition(position, shirtModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(shirtModel, appearanceTypeToSourceRectangles), shirtColor, rotation, origin + new Vector2(shirtModel.BodyPosition.X, shirtModel.BodyPosition.Y), shirtModel.Scale * scale, shirtLayer);
                }
                if (shirtModel.HasSkinToneMask())
                {
                    shirtLayer += 0.01E-05f;
                    DrawSkinToneMask(b, shirtPack, shirtModel, skinTone, GetScaledPosition(position, shirtModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(shirtModel, appearanceTypeToSourceRectangles), shirtColor, rotation, origin + new Vector2(shirtModel.BodyPosition.X, shirtModel.BodyPosition.Y), shirtModel.Scale * scale, shirtLayer);
                }

                layerDepth = shirtLayer;
            }
            layerDepth += 0.02E-05f;

            // Draw accessory
            if (accessoriesData.Count == 0)
            {
                DrawAccessoryVanilla(b, ___accessorySourceRect, __instance, who, currentFrame, rotation, scale, layerDepth, position, origin, ___positionOffset, ___rotationAdjustment, overrideColor);
            }
            else
            {
                var accessoryLayer = who.FacingDirection == 2 ? layerDepth - 1E-5f : layerDepth;

                int index = 0;
                foreach (var pair in accessoriesData)
                {
                    var accessoryPack = pair.Key;
                    var accessoryModel = pair.Value;

                    if (!(accessoryModel.HideWhileSwimming && who.swimming.Value) && !(accessoryModel.HideWhileWearingBathingSuit && who.bathingClothes.Value))
                    {
                        var colorKey = FashionSense.accessoryManager.GetKeyForAccessoryColor(index);
                        var accessoryColor = new Color() { PackedValue = who.modData.ContainsKey(colorKey) ? uint.Parse(who.modData[colorKey]) : who.hairstyleColor.Value.PackedValue };

                        DrawCustomAccessory(accessoryPack, accessoryModel, GetSourceRectangle(accessoryModel, appearanceTypeToSourceRectangles), accessoryColor, skinTone, __instance, ___isDrawingForUI, b, who, facingDirection, position, origin, ___positionOffset, ___rotationAdjustment, scale, currentFrame, rotation, accessoryLayer);
                    }

                    index++;
                }
            }
            layerDepth += 0.02E-05f;

            // Draw hair
            if (hairModel is null || (hairModel.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (hairModel.HideWhileSwimming && who.swimming.Value))
            {
                if (hatModel is null || !hatModel.HideHair)
                {
                    DrawHairVanilla(b, FarmerRenderer.hairStylesTexture, ___hairstyleSourceRect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset, overrideColor);
                }
            }
            else
            {
                float hairOffset = 2.2E-05f;
                var hairColor = overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : overrideColor;
                if (hairModel.DisableGrayscale)
                {
                    hairColor = Color.White;
                }
                else if (hairModel.IsPrismatic)
                {
                    hairColor = Utility.GetPrismaticColor(speedMultiplier: hairModel.PrismaticAnimationSpeedMultiplier);
                }
                hairLayer = layerDepth + hairOffset;

                if (hatModel is null || !hatModel.HideHair)
                {
                    var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hairModel, true);
                    featureOffset.Y -= who.isMale ? 4 : 0;

                    // Draw the hair
                    b.Draw(hairPack.Texture, GetScaledPosition(position, hairModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(hairModel, appearanceTypeToSourceRectangles), hairModel.HasColorMask() ? Color.White : hairColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), hairModel.Scale * scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, hairLayer);

                    if (hairModel.HasColorMask())
                    {
                        hairLayer += 2.2E-05f + 0.01E-05f;
                        DrawColorMask(b, hairPack, hairModel, GetScaledPosition(position, hairModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(hairModel, appearanceTypeToSourceRectangles), hairColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), hairModel.Scale * scale, hairLayer);
                    }
                    if (hairModel.HasSkinToneMask())
                    {
                        hairLayer += 2.2E-05f + 0.01E-05f;
                        DrawSkinToneMask(b, hairPack, hairModel, skinTone, GetScaledPosition(position, hairModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(hairModel, appearanceTypeToSourceRectangles), hairColor, rotation, origin + new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), hairModel.Scale * scale, hairLayer);
                    }

                    layerDepth = hairLayer;
                }
            }
            layerDepth += 0.01E-05f;

            // Draw the sleeves
            if (sleevesModel is null || (sleevesModel.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (sleevesModel.HideWhileSwimming && who.swimming.Value) || AreSleevesForcedHidden(pantsModel, hairModel, hatModel, shirtModel) || AreSleevesForcedHidden(accessoriesData.Values.ToArray()))
            {
                // Handled in DrawPatch.HandleCustomDraw
            }
            else
            {
                var sleevesColor = new Color() { PackedValue = who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR) ? uint.Parse(who.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR]) : who.hairstyleColor.Value.PackedValue };
                if (sleevesModel.DisableGrayscale)
                {
                    sleevesColor = Color.White;
                }
                else if (sleevesModel.IsPrismatic)
                {
                    sleevesColor = Utility.GetPrismaticColor(speedMultiplier: sleevesModel.PrismaticAnimationSpeedMultiplier);
                }

                // Adjust the sleevesLayer according to model's adjustment properties
                sleevesLayer = layerDepth;
                if (sleevesModel.DrawBeforeShirt)
                {
                    sleevesLayer = hairModel is not null ? shirtLayer - 0.03E-05f : sleevesLayer;
                }
                else if (sleevesModel.DrawBeforeHair)
                {
                    sleevesLayer = hairModel is not null ? layerDepth - hairLayer : sleevesLayer;
                }

                var featureOffset = GetFeatureOffset(facingDirection, currentFrame, scale, __instance, sleevesModel, false);
                featureOffset.Y -= who.isMale ? 4 : 0;

                b.Draw(sleevesPack.Texture, GetScaledPosition(position, sleevesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(sleevesModel, appearanceTypeToSourceRectangles), sleevesModel.HasColorMask() ? Color.White : sleevesColor, rotation, origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * scale, sleevesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, sleevesLayer);

                if (sleevesModel.HasColorMask())
                {
                    sleevesLayer += 0.01E-05f;
                    DrawColorMask(b, sleevesPack, sleevesModel, GetScaledPosition(position, sleevesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(sleevesModel, appearanceTypeToSourceRectangles), sleevesColor, rotation, origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * scale, sleevesLayer);
                }
                if ((sleevesModel.HasColorMask() || sleevesModel.HasShirtToneMask()) && sleevesModel.UseShirtColors)
                {
                    sleevesLayer += 0.01E-05f;
                    if (shirtModel is not null && shirtModel.SleeveColors is not null)
                    {
                        DrawSleeveColorMask(b, sleevesPack, sleevesModel, shirtModel, GetScaledPosition(position, sleevesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(sleevesModel, appearanceTypeToSourceRectangles), sleevesColor, rotation, origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * scale, sleevesLayer);
                    }
                    else
                    {
                        DrawSleeveColorMaskVanilla(b, sleevesPack, sleevesModel, who, __instance, GetScaledPosition(position, sleevesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(sleevesModel, appearanceTypeToSourceRectangles), sleevesColor, rotation, origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * scale, sleevesLayer);
                    }
                }
                if (sleevesModel.HasSkinToneMask())
                {
                    sleevesLayer += 0.01E-05f;
                    DrawSkinToneMask(b, sleevesPack, sleevesModel, skinTone, GetScaledPosition(position, sleevesModel, ___isDrawingForUI) + origin + ___positionOffset + featureOffset, GetSourceRectangle(sleevesModel, appearanceTypeToSourceRectangles), sleevesColor, rotation, origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * scale, sleevesLayer);
                }
            }
            layerDepth += 0.01E-05f;

            // Draw hat
            if (hatModel is null || (hatModel.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (hatModel.HideWhileSwimming && who.swimming.Value))
            {
                DrawHatVanilla(b, ___hatSourceRect, __instance, who, currentFrame, facingDirection, rotation, scale, layerDepth, position, origin, ___positionOffset);
            }
            else
            {
                var hatColor = new Color() { PackedValue = who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_HAT_COLOR) ? uint.Parse(who.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) : who.hairstyleColor.Value.PackedValue };
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
                b.Draw(hatPack.Texture, GetScaledPosition(position, hatModel, ___isDrawingForUI) + origin + ___positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hatModel, flip), GetSourceRectangle(hatModel, appearanceTypeToSourceRectangles), hatModel.HasColorMask() ? Color.White : hatColor, rotation, origin + new Vector2(hatModel.HeadPosition.X, hatModel.HeadPosition.Y), hatModel.Scale * scale, hatModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerOffset);

                if (hatModel.HasColorMask())
                {
                    DrawColorMask(b, hatPack, hatModel, GetScaledPosition(position, hatModel, ___isDrawingForUI) + origin + ___positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hatModel, flip), GetSourceRectangle(hatModel, appearanceTypeToSourceRectangles), hatColor, rotation, origin + new Vector2(hatModel.HeadPosition.X, hatModel.HeadPosition.Y), hatModel.Scale * scale, layerDepth + layerOffset + 0.01E-05f);
                }
                if (hatModel.HasSkinToneMask())
                {
                    DrawSkinToneMask(b, hatPack, hatModel, skinTone, GetScaledPosition(position, hatModel, ___isDrawingForUI) + origin + ___positionOffset + GetFeatureOffset(facingDirection, currentFrame, scale, __instance, hatModel, flip), GetSourceRectangle(hatModel, appearanceTypeToSourceRectangles), hatColor, rotation, origin + new Vector2(hatModel.HeadPosition.X, hatModel.HeadPosition.Y), hatModel.Scale * scale, layerDepth + layerOffset + 0.01E-05f);
                }
            }

            AreColorMasksPendingRefresh = false;
            return false;
        }

        private static bool DrawMiniPortratPrefix(FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) || ___baseTexture is null || ___baseTexture.IsDisposed)
            {
                return true;
            }

            var hairPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]);
            if (hairPack is null)
            {
                return true;
            }

            // This is in the vanilla code, which for some reason is always 2 instead of relying on facingDirection's initial value
            facingDirection = 2;

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
                DrawColorMask(b, hairPack, hairModel, position + new Vector2(0f, feature_y_offset * 4) * scale / 4f, sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, layerDepth + hair_draw_layer + 0.01E-05f);
            }
            if (hairModel.HasSkinToneMask())
            {
                // Get skin tone
                var skinTone = DrawPatch.GetSkinTone(___farmerTextureManager, ___baseTexture, null, ___skin, ____sickFrame);
                DrawSkinToneMask(b, hairPack, hairModel, skinTone, position + new Vector2(0f, feature_y_offset * 4) * scale / 4f, sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, layerDepth + hair_draw_layer + 0.01E-05f);
            }

            return false;
        }
    }
}
