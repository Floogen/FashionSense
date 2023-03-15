using FashionSense.Framework.Managers;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Utilities
{
    public class AppearanceHelpers
    {
        public static void HandleAppearanceAnimation(List<AppearanceModel> models, AppearanceModel model, Farmer who, int facingDirection, ref Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles, bool forceUpdate = false)
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

            // Establish the source rectangle for the model
            appearanceTypeToSourceRectangles[model] = new Rectangle(model.StartingPosition.X, model.StartingPosition.Y, size.Width, size.Length);

            // Reset any cached animation data, if needed
            if (model.HasMovementAnimation() && FashionSense.conditionData.IsPlayerMoving(who) && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Moving))
            {
                SetAnimationType(model, who, AnimationModel.Type.Moving);
                FashionSense.ResetAnimationModDataFields(who, model.MovementAnimation.ElementAt(0).GetDuration(true), AnimationModel.Type.Moving, facingDirection, true, model);

                foreach (var animation in model.MovementAnimation)
                {
                    animation.Reset();
                }
            }
            else if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving(who) && !HasCorrectAnimationTypeCached(model, who, AnimationModel.Type.Idle))
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
            if (model.HasMovementAnimation() && (FashionSense.conditionData.IsPlayerMoving(who) || IsWaitingOnRequiredAnimation(who, model)))
            {
                HandleAppearanceAnimation(models, model, who, AnimationModel.Type.Moving, model.MovementAnimation, facingDirection, ref appearanceTypeToSourceRectangles, !FashionSense.conditionData.IsPlayerMoving(who) && IsWaitingOnRequiredAnimation(who, model), forceUpdate);
            }
            else if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving(who))
            {
                HandleAppearanceAnimation(models, model, who, AnimationModel.Type.Idle, model.IdleAnimation, facingDirection, ref appearanceTypeToSourceRectangles, forceUpdate);
            }
            else if (model.HasUniformAnimation())
            {
                HandleAppearanceAnimation(models, model, who, AnimationModel.Type.Uniform, model.UniformAnimation, facingDirection, ref appearanceTypeToSourceRectangles, forceUpdate);
            }
        }

        public static List<AnimationModel> GetModelAnimation(AppearanceModel model, Farmer who)
        {
            if (model.HasMovementAnimation() && (FashionSense.conditionData.IsPlayerMoving(who) || IsWaitingOnRequiredAnimation(who, model)))
            {
                return model.MovementAnimation;
            }
            if (model.HasIdleAnimation() && !FashionSense.conditionData.IsPlayerMoving(who))
            {
                return model.IdleAnimation;
            }
            if (model.HasUniformAnimation())
            {
                return model.UniformAnimation;
            }

            return new List<AnimationModel>();
        }

        public static int GetModelAnimationIterator(AppearanceModel model, Farmer who)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model);
            if (animationData is null)
            {
                return 0;
            }

            return animationData.Iterator;
        }

        public static bool HasCorrectAnimationTypeCached(AppearanceModel model, Farmer who, AnimationModel.Type type)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model);
            if (animationData is null)
            {
                return false;
            }

            return animationData.Type == type;
        }

        public static void SetAnimationType(AppearanceModel model, Farmer who, AnimationModel.Type type)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model);
            if (animationData is null)
            {
                return;
            }

            animationData.Type = type;
        }

        public static void HandleAppearanceAnimation(List<AppearanceModel> activeModels, AppearanceModel appearanceModel, Farmer who, AnimationModel.Type animationType, List<AnimationModel> animations, int facingDirection, ref Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles, bool isAnimationFinishing = false, bool forceUpdate = false)
        {
            if (!HasRequiredModDataKeys(appearanceModel, who) || !HasCorrectAnimationTypeCached(appearanceModel, who, animationType) || who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] != facingDirection.ToString())
            {
                SetAnimationType(appearanceModel, who, animationType);
                FashionSense.ResetAnimationModDataFields(who, animations.ElementAt(0).GetDuration(true), animationType, facingDirection, true, appearanceModel);
            }
            var modelPack = appearanceModel.Pack;

            // Determine the modData keys to use based on AppearanceModel
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, appearanceModel);
            if (animationData is null)
            {
                return;
            }

            // Pull values from animationData, as we don't want to modify them here
            int iterator = animationData.Iterator;
            int startingIndex = animationData.StartingIndex;
            int frameDuration = animationData.FrameDuration;
            int elapsedDuration = animationData.ElapsedDuration;
            int lastFarmerFrame = animationData.FarmerFrame;

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

        public static int GetNextValidFrame(List<AnimationModel> animations, Farmer who, AppearanceModel appearanceModel, int iterator, int startingIndex)
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

        public static bool HasModelOfType(List<AppearanceModel> models, AppearanceContentPack.Type appearanceType)
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

        public static void UpdateLight(AppearanceModel model, AnimationModel animationModel, Farmer who, bool recalculateLight)
        {
            if (Game1.currentLocation is null)
            {
                return;
            }

            int indexOffset = 0;
            switch (model)
            {
                case AccessoryModel accessoryModel:
                    var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                    if (accessoryIndex != -1)
                    {
                        indexOffset = 10 + accessoryIndex;
                    }
                    break;
                case HatModel hatModel:
                    indexOffset = 4;
                    break;
                case ShirtModel shirtModel:
                    indexOffset = 5;
                    break;
                case PantsModel pantsModel:
                    indexOffset = 6;
                    break;
                case SleevesModel sleevesModel:
                    indexOffset = 7;
                    break;
                case ShoesModel shoesModel:
                    indexOffset = 7;
                    break;
                case HairModel hairModel:
                    indexOffset = 9;
                    break;
                default:
                    // Unhandled model type
                    return;
            }

            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model);
            if (animationData is null)
            {
                return;
            }

            // Handle any missing key value
            if (animationData.LightId is null)
            {
                animationData.LightId = GenerateLightId(indexOffset);

                if (animationData.LightId is null)
                {
                    return;
                }
            }

            var lightModel = animationModel.Light;
            if (lightModel is null)
            {
                if (Game1.currentLocation.sharedLights.ContainsKey(animationData.LightId.Value))
                {
                    Game1.currentLocation.sharedLights.Remove(animationData.LightId.Value);
                }
                return;
            }

            // Handle updating the position and other values of the light
            if (!Game1.currentLocation.sharedLights.ContainsKey(animationData.LightId.Value))
            {
                Game1.currentLocation.sharedLights[animationData.LightId.Value] = new LightSource(lightModel.GetTextureSource(), who.position - new Vector2(lightModel.Position.X, lightModel.Position.Y), lightModel.GetRadius(recalculateLight), lightModel.GetColor(), LightSource.LightContext.None);
            }
            else
            {
                Game1.currentLocation.sharedLights[animationData.LightId.Value].position.Value = who.position - new Vector2(lightModel.Position.X, lightModel.Position.Y);
                Game1.currentLocation.sharedLights[animationData.LightId.Value].radius.Value = lightModel.GetRadius(recalculateLight);
                Game1.currentLocation.sharedLights[animationData.LightId.Value].color.Value = lightModel.GetColor();
            }
        }

        public static int? GenerateLightId(int offset)
        {
            var baseKeyId = -1 * offset * 5000;
            for (int x = 0; x < 10; x++)
            {
                if (!Game1.currentLocation.sharedLights.ContainsKey(baseKeyId + x))
                {
                    return baseKeyId + x;
                }
            }

            return null;
        }

        internal static bool AreSleevesForcedHidden(List<AppearanceMetadata> metadata)
        {
            foreach (var data in metadata.Where(d => d.Model is not null))
            {
                if (data.Model.HideSleeves)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsHatHidingHair(List<AppearanceMetadata> metadata)
        {
            foreach (var data in metadata.Where(d => d.Model is not null && d.Model is HatModel))
            {
                if ((data.Model as HatModel).HideHair)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool ShouldUseBaldBase(Farmer who, int facingDirection)
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
            else if (hairModel is null && who.IsBaldHairStyle(who.getHair()) && (hatModel is null || hatModel.HideHair is false))
            {
                return true;
            }

            return false;
        }

        internal static bool ShouldHideWaterLine(List<AppearanceMetadata> metadata)
        {
            foreach (var data in metadata.Where(d => d.Model is not null))
            {
                if (data.Model.HideWaterLine)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool ShouldHideWhileSwimmingOrWearingBathingSuit(Farmer who, AppearanceModel model)
        {
            return (model.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (model.HideWhileSwimming && who.swimming.Value);
        }

        internal static bool ShouldHideLegs(Farmer who, int facingDirection)
        {
            // Get the pants model, if applicable
            PantsModel pantsModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(who.modData[ModDataKeys.CUSTOM_PANTS_ID]) is PantsContentPack pPack && pPack != null)
            {
                pantsModel = pPack.GetPantsFromFacingDirection(facingDirection);
            }

            return pantsModel is not null && pantsModel.HideLegs;
        }

        public static bool IsWaitingOnRequiredAnimation(Farmer who, AppearanceModel model)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model);
            if (animationData is null)
            {
                return false;
            }

            return model.RequireAnimationToFinish && animationData.Iterator != 0;
        }


        public static bool HasRequiredModDataKeys(AppearanceModel model, Farmer who)
        {
            return FashionSense.animationManager.GetSpecificAnimationData(who, model) is not null;
        }

        public static bool IsFrameValid(Farmer who, AppearanceModel model, List<AnimationModel> animations, int iterator, bool probe = false)
        {
            AnimationModel animationModel = animations.ElementAtOrDefault(iterator);
            if (animationModel is null)
            {
                return false;
            }

            // Get the farmer's FarmerSprite.currentSingleAnimation via reflection
            int currentSingleAnimation = FashionSense.modHelper.Reflection.GetField<int>(who.FarmerSprite, "currentSingleAnimation").GetValue();

            bool isValid = AreConditionsValid(animationModel.Conditions, currentSingleAnimation, who, model, animations, iterator, probe);

            if (!probe)
            {
                animationModel.WasDisplayed = isValid;
            }

            return isValid;
        }

        public static bool AreConditionsValid(List<Condition> conditions, int currentSingleAnimation, Farmer who, AppearanceModel model, List<AnimationModel> animations, int iterator, bool probe)
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
                    passedCheck = condition.IsValid(true, FashionSense.conditionData.IsMovingLongEnough(who, condition.GetParsedValue<long>(!probe)));
                }
                else if (condition.Name is Condition.Type.MovementDurationLogical)
                {
                    passedCheck = condition.IsValid(FashionSense.conditionData.GetMovementDuration(who));
                }
                else if (condition.Name is Condition.Type.IsElapsedTimeMultipleOf)
                {
                    passedCheck = condition.IsValid(true, FashionSense.conditionData.IsElapsedTimeMultipleOf(who, condition, probe));
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
                    passedCheck = condition.IsValid(true, FashionSense.conditionData.IsMovingFastEnough(who, condition.GetParsedValue<long>(!probe)));
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

        public static void UpdatePlayerAnimationData(AppearanceModel model, Farmer who, AnimationModel.Type type, List<AnimationModel> animations, int facingDirection, int iterator, int startingIndex)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model);
            if (animationData is null)
            {
                return;
            }

            animationData.Type = type;
            animationData.Iterator = iterator;
            animationData.StartingIndex = startingIndex;
            animationData.FrameDuration = animations.ElementAt(iterator).GetDuration(true);
            animationData.ElapsedDuration = 0;
            animationData.FarmerFrame = who.FarmerSprite.CurrentFrame;
        }

        public static void OffsetSourceRectangles(Farmer who, int facingDirection, float rotation, ref Rectangle shirtSourceRect, ref Rectangle dyed_shirt_source_rect, ref Rectangle accessorySourceRect, ref Rectangle hatSourceRect, ref Vector2 rotationAdjustment)
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

        public static Vector2 GetFeatureOffset(int facingDirection, int currentFrame, float scale, FarmerRenderer renderer, AppearanceModel model, bool flip = false)
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

        public static Vector2 GetScaledPosition(Vector2 position, AppearanceModel model, bool isDrawingForUI)
        {
            if (isDrawingForUI)
            {
                position.Y += (4f - model.Scale) * 32;
            }

            return position;
        }

        public static Color[] GetVanillaShirtSleeveColors(Farmer who, FarmerRenderer renderer)
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
    }
}
