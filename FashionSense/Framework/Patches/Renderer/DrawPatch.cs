using FashionSense.Framework.Managers;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.UI;
using FashionSense.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Patches.Renderer
{
    internal class DrawPatch : PatchTemplate
    {
        internal static float? lastCustomLayerDepth;
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
            // Since the game doesn't pass the farmer over to the native ApplyShoeColor method, we have to find it by matching the FarmerRender instance
            Farmer who = Game1.player;
            if (who.FarmerRenderer != __instance && Game1.activeClickableMenu is SearchMenu searchMenu && searchMenu is not null)
            {
                foreach (var fakeFarmer in searchMenu.fakeFarmers)
                {
                    if (fakeFarmer.FarmerRenderer == __instance)
                    {
                        who = fakeFarmer;
                    }
                }
            }

            if (!who.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR) || !who.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) || who.modData[ModDataKeys.CUSTOM_SHOES_ID] is null || who.modData[ModDataKeys.CUSTOM_SHOES_ID] == "None")
            {
                return true;
            }
            else if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(who.modData[ModDataKeys.CUSTOM_SHOES_ID]) is null)
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

            var isDoingVanillaOverride = false;
            if (who.modData[ModDataKeys.CUSTOM_SHOES_ID] == ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID && AppearanceHelpers.ShouldHideLegs(who, who.FacingDirection) is false)
            {
                isDoingVanillaOverride = true;
            }

            SwapColorReversePatch(__instance, texture_name, pixels, 268, Utility.MultiplyColor(darkestColor, isDoingVanillaOverride ? shoeColor : Color.Transparent));
            SwapColorReversePatch(__instance, texture_name, pixels, 269, Utility.MultiplyColor(mediumColor, isDoingVanillaOverride ? shoeColor : Color.Transparent));
            SwapColorReversePatch(__instance, texture_name, pixels, 270, Utility.MultiplyColor(lightColor, isDoingVanillaOverride ? shoeColor : Color.Transparent));
            SwapColorReversePatch(__instance, texture_name, pixels, 271, Utility.MultiplyColor(lightestColor, isDoingVanillaOverride ? shoeColor : Color.Transparent));

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
        private static bool DrawPrefix(FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            if (who.isFakeEventActor && Game1.eventUp)
            {
                who = Game1.player;
            }
            DrawPatch.lastCustomLayerDepth = null;

            // Check what player base to use
            if (AppearanceHelpers.ShouldUseBaldBase(who, facingDirection))
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

            // Get the currently equipped models
            List<AppearanceMetadata> equippedModels = GetCurrentlyEquippedModels(who, facingDirection);

            if (equippedModels.Count > 0)
            {
                // Draw with modified SpriteSortMode method for UI to handle clipping issue
                //_monitor.Log(_helper.Reflection.GetField<SpriteSortMode>(b, "_sortMode").GetValue().ToString(), LogLevel.Debug);
                if (FarmerRenderer.isDrawingForUI)
                {
                    b.End();
                    b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                    HandleConditionalDraw(equippedModels, __instance, ___farmerTextureManager, ___baseTexture, ___skin, ref ___hairstyleSourceRect, ref ___shirtSourceRect, ref ___accessorySourceRect, ref ___hatSourceRect, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);

                    b.End();
                    b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                }
                else
                {
                    // Utilize standard SpriteSortMode if not using the UI
                    HandleConditionalDraw(equippedModels, __instance, ___farmerTextureManager, ___baseTexture, ___skin, ref ___hairstyleSourceRect, ref ___shirtSourceRect, ref ___accessorySourceRect, ref ___hatSourceRect, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
                }

                return false;
            }

            return true;
        }

        private static void HandleConditionalDraw(List<AppearanceMetadata> equippedModels, FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            // Check if we need to utilize custom draw logic
            if (equippedModels.Count() > 0 || AppearanceHelpers.AreSleevesForcedHidden(equippedModels))
            {
                HandleCustomDraw(equippedModels, __instance, ___farmerTextureManager, ___baseTexture, ___skin, ref ___hairstyleSourceRect, ref ___shirtSourceRect, ref ___accessorySourceRect, ref ___hatSourceRect, ref ___positionOffset, ref ___rotationAdjustment, ref ____sickFrame, ref ____shirtDirty, ref ____spriteDirty, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }
            else
            {
                DrawReversePatch(__instance, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            }
        }

        private static void HandleCustomDraw(List<AppearanceMetadata> equippedModels, FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            // Determine if player is currently sick
            bool sick_frame = currentFrame == 104 || currentFrame == 105;
            if (____sickFrame != sick_frame)
            {
                ____sickFrame = sick_frame;
                ____shirtDirty = true;
                ____spriteDirty = true;
            }

            // Recolor the player's base
            ExecuteRecolorActionsReversePatch(__instance, who);
            var baseTexture = _helper.Reflection.GetField<Texture2D>(__instance, "baseTexture").GetValue();

            position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));
            ___rotationAdjustment = Vector2.Zero;
            ___positionOffset.Y = animationFrame.positionOffset * 4;
            ___positionOffset.X = animationFrame.xOffset * 4;
            if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming.Value)
            {
                sourceRect.Height /= 2;
                sourceRect.Height -= (int)who.yOffset / 4;
                position.Y += 64f;
            }
            if (facingDirection == 3 || facingDirection == 1)
            {
                facingDirection = ((!animationFrame.flip) ? 1 : 3);
            }

            // Get skin tone
            var skinTone = DrawPatch.GetSkinTone(___farmerTextureManager, baseTexture, null, ___skin, ____sickFrame);

            // Establish the animation data for models
            Dictionary<AppearanceModel, AnimationModel> appearanceTypeToAnimationModels = new Dictionary<AppearanceModel, AnimationModel>();

            // Generate the list of layers to draw
            List<LayerData> layers = GenerateDrawLayers(equippedModels, __instance, ref appearanceTypeToAnimationModels, FarmerRenderer.isDrawingForUI, ___positionOffset, ___rotationAdjustment, ___farmerTextureManager, ___baseTexture, ___skin, ____sickFrame, ref ___hairstyleSourceRect, ref ___shirtSourceRect, ref ___accessorySourceRect, ref ___hatSourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);

            // Get the dyed_shirt_source_rect
            Rectangle dyedShirtSourceRect = ___shirtSourceRect;
            dyedShirtSourceRect.Offset(128, 0);

            // Offset the source rectangles for shirts, accessories and hats according to facingDirection
            AppearanceHelpers.OffsetSourceRectangles(who, facingDirection, rotation, ref ___shirtSourceRect, ref dyedShirtSourceRect, ref ___accessorySourceRect, ref ___hatSourceRect, ref ___rotationAdjustment);

            // Prepare the DrawManager
            DrawManager drawManager = new DrawManager(b, __instance, skinTone, baseTexture, sourceRect, ___shirtSourceRect, dyedShirtSourceRect, ___accessorySourceRect, ___hatSourceRect, appearanceTypeToAnimationModels, animationFrame, overrideColor, position, origin, ___positionOffset, ___rotationAdjustment, facingDirection, currentFrame, scale, rotation, FarmerRendererPatch.AreColorMasksPendingRefresh, FarmerRenderer.isDrawingForUI, AppearanceHelpers.AreSleevesForcedHidden(equippedModels))
            {
                LayerDepth = layerDepth
            };

            // Vanilla swim draw logic
            if (FarmerRenderer.isDrawingForUI is false && who.swimming.Value)
            {
                // Draw all sorted layers
                drawManager.DrawLayers(who, layers);
                if (!AppearanceHelpers.ShouldHideWaterLine(equippedModels))
                {
                    b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)who.yOffset + 8, (int)position.Y - 128 + sourceRect.Height * 4 + (int)origin.Y - (int)who.yOffset, sourceRect.Width * 4 - (int)who.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, drawManager.LayerDepth + 0.001f);
                }
                return;
            }

            // Draw all sorted layers
            drawManager.DrawLayers(who, layers);

            // Utilize custom draw for held object, if applicable
            DrawPatch.lastCustomLayerDepth = drawManager.LayerDepth;
            if (FarmerRenderer.isDrawingForUI is false && who.ActiveObject is not null && who.IsCarrying())
            {
                Game1.drawPlayerHeldObject(who);
            }

            FarmerRendererPatch.AreColorMasksPendingRefresh = false;
        }

        private static List<LayerData> GenerateDrawLayers(List<AppearanceMetadata> metadata, FarmerRenderer __instance, ref Dictionary<AppearanceModel, AnimationModel> appearanceTypeToAnimationModels, bool ___isDrawingForUI, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            // Check if all the models are null, if so revert back to vanilla logic
            List<AppearanceModel> models = metadata.Where(m => m.Model is not null).Select(m => m.Model).ToList();
            if (models.Count == 0)
            {
                return new List<LayerData>();
            }

            // Set up the initial data
            foreach (var model in models)
            {
                appearanceTypeToAnimationModels[model] = null;
            }

            // Handle any animations
            foreach (var model in models)
            {
                AppearanceHelpers.HandleAppearanceAnimation(models, model, who, facingDirection, ref appearanceTypeToAnimationModels);
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
            if ((int)who.accessory.Value >= 0)
            {
                ___accessorySourceRect = new Rectangle((int)who.accessory.Value * 16 % FarmerRenderer.accessoriesTexture.Width, (int)who.accessory.Value * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16);
            }
            if (who.hat.Value != null)
            {
                ___hatSourceRect = new Rectangle(20 * (int)who.hat.Value.which.Value % FarmerRenderer.hatsTexture.Width, 20 * (int)who.hat.Value.which.Value / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20);
            }

            // Go through the models and determine draw order
            return FashionSense.layerManager.SortModelsForDrawing(who, facingDirection, metadata);
        }

        private static Color GetColorValue(Farmer who, AppearanceModel model)
        {
            string key = null;
            switch (model)
            {
                case PantsModel:
                    key = ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR;
                    break;
                case SleevesModel:
                    key = ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR;
                    break;
                case ShirtModel:
                    key = ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR;
                    break;
                case AccessoryModel:
                    // Purposely skipping logic check as Fashion Sense utilizes
                    return Color.White;
                case HairModel:
                    // Purposely skipping logic check as Fashion Sense doesn't utilize ModData key for hair
                    return who.hairstyleColor.Value;
                case HatModel:
                    key = ModDataKeys.UI_HAND_MIRROR_HAT_COLOR;
                    break;
                case ShoesModel:
                    key = ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR;
                    break;
                default:
                    return Color.White;
            }

            return new Color() { PackedValue = who.modData.ContainsKey(key) ? uint.Parse(who.modData[key]) : who.hairstyleColor.Value.PackedValue };
        }

        internal static List<AppearanceMetadata> GetCurrentlyEquippedModels(Farmer who, int facingDirection)
        {
            // Set up each AppearanceModel
            List<AppearanceMetadata> models = new List<AppearanceMetadata>();

            // Pants pack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(who.modData[ModDataKeys.CUSTOM_PANTS_ID]) is PantsContentPack pPack && pPack != null)
            {
                var pantModel = pPack.GetPantsFromFacingDirection(facingDirection);
                models.Add(new AppearanceMetadata(pantModel, GetColorValue(who, pantModel)));
            }

            // Hair pack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]) is HairContentPack hPack && hPack != null)
            {
                var hairModel = hPack.GetHairFromFacingDirection(facingDirection);
                models.Add(new AppearanceMetadata(hairModel, GetColorValue(who, hairModel)));
            }

            // Accessory packs
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID))
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

                            models.Add(new AppearanceMetadata(accessoryModel, FashionSense.accessoryManager.GetColorFromIndex(who, index)));
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO: Flag error here
                }
            }

            // Hat pack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(who.modData[ModDataKeys.CUSTOM_HAT_ID]) is HatContentPack tPack && tPack != null)
            {
                var hatModel = tPack.GetHatFromFacingDirection(facingDirection);
                models.Add(new AppearanceMetadata(hatModel, GetColorValue(who, hatModel)));
            }

            // Shirt pack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
            {
                var shirtModel = sPack.GetShirtFromFacingDirection(facingDirection);
                models.Add(new AppearanceMetadata(shirtModel, GetColorValue(who, shirtModel)));
            }

            // Sleeves pack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SLEEVES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(who.modData[ModDataKeys.CUSTOM_SLEEVES_ID]) is SleevesContentPack slPack && slPack != null)
            {
                var slModel = slPack.GetSleevesFromFacingDirection(facingDirection);
                models.Add(new AppearanceMetadata(slModel, GetColorValue(who, slModel)));
            }

            // Shoes pack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(who.modData[ModDataKeys.CUSTOM_SHOES_ID]) is ShoesContentPack shPack && shPack != null)
            {
                var shModel = shPack.GetShoesFromFacingDirection(facingDirection);
                models.Add(new AppearanceMetadata(shModel, GetColorValue(who, shModel)));
            }

            return models.Where(m => m is not null && m.Model is not null && m.Model.Pack is not null).ToList();
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
