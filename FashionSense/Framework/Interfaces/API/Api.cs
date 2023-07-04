using FashionSense.Framework.Managers;
using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static FashionSense.Framework.Interfaces.API.IApi;
using static StardewValley.FarmerSprite;

namespace FashionSense.Framework.Interfaces.API
{
    public interface IApi
    {
        public enum Type
        {
            Unknown,
            Hair,
            Accessory,
            [Obsolete("No longer maintained. Use Accessory instead.")]
            AccessorySecondary,
            [Obsolete("No longer maintained. Use Accessory instead.")]
            AccessoryTertiary,
            Hat,
            Shirt,
            Pants,
            Sleeves,
            Shoes,
            Player
        }

        public record RawTextureData(int Width, int Height, Color[] Data) : IRawTextureData;

        KeyValuePair<bool, string> SetAppearance(Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessorySlot(string accessoryId, int accessorySlot);
        KeyValuePair<bool, string> SetAppearanceColor(Type appearanceType, Color color, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessoryColor(Color color, int accessorySlot);

        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetHatAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetHairAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
        KeyValuePair<bool, string> SetAccessoryPrimaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
        KeyValuePair<bool, string> SetAccessorySecondaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
        KeyValuePair<bool, string> SetAccessoryTertiaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetShirtAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetSleevesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetPantsAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetShoesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

        KeyValuePair<bool, string> ClearAppearance(Type appearanceType, IManifest callerManifest);
        KeyValuePair<bool, string> ClearAccessorySlot(int accessorySlot, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearHatAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearHairAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
        KeyValuePair<bool, string> ClearAccessoryPrimaryAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
        KeyValuePair<bool, string> ClearAccessorySecondaryAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
        KeyValuePair<bool, string> ClearAccessoryTertiaryAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearShirtAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearSleevesAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearPantsAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearShoesAppearance(IManifest callerManifest);

        KeyValuePair<bool, string> GetCurrentAppearanceId(Type appearanceType, Farmer target = null);
        KeyValuePair<bool, Color> GetAppearanceColor(Type appearanceType, Farmer target = null);
        KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, bool getOriginalTexture = false);
        KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(string appearanceId, bool getOriginalTexture = false);
        KeyValuePair<bool, string> SetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false);
        KeyValuePair<bool, string> SetAppearanceTexture(string appearanceId, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false);
        KeyValuePair<bool, string> ResetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> ResetAppearanceTexture(string appearanceId, IManifest callerManifest);

        KeyValuePair<bool, List<string>> GetOutfitIds();
        KeyValuePair<bool, string> GetCurrentOutfitId();
        KeyValuePair<bool, string> SetCurrentOutfitId(string outfitId, IManifest callerManifest);

        KeyValuePair<bool, string> SetAppearanceLockState(Type appearanceType, string targetPackId, string targetAppearanceName, bool isLocked, IManifest callerManifest);

        public interface IDrawTool
        {
            public Farmer Farmer { get; init; }
            public SpriteBatch SpriteBatch { get; init; }
            public FarmerRenderer FarmerRenderer { get; init; }
            public Texture2D BaseTexture { get; init; }
            public Rectangle FarmerSourceRectangle { get; init; }
            public AnimationFrame AnimationFrame { get; init; }
            public bool IsDrawingForUI { get; init; }
            public Color OverrideColor { get; init; }
            public Color AppearanceColor { get; set; }
            public Vector2 Position { get; init; }
            public Vector2 Origin { get; init; }
            public Vector2 PositionOffset { get; init; }
            public int FacingDirection { get; init; }
            public int CurrentFrame { get; init; }
            public float Scale { get; init; }
            public float Rotation { get; init; }
            public float LayerDepthSnapshot { get; set; }
        }
        KeyValuePair<bool, string> RegisterAppearanceDrawOverride(Type appearanceType, IManifest callerManifest, Func<IDrawTool, bool> appearanceDrawOverride);
        KeyValuePair<bool, string> UnregisterAppearanceDrawOverride(Type appearanceType, IManifest callerManifest);
        KeyValuePair<bool, string> IsDrawOverrideActive(Type appearanceType, IManifest callerManifest);

        // Events
        event EventHandler SetSpriteDirtyTriggered;

        /*
         * Example usages (using the Fashion Sense example pack)
         * 
         * var api = Helper.ModRegistry.GetApi<IApi>("PeacefulEnd.FashionSense");
         * var response = api.SetHatAppearance("ExampleAuthor.ExampleFashionSensePack", "Animated Pumpkin Head", this.ModManifest);
         * if (response.Key is true)
         * {
         *     // Setting was successful!
         * }
         * 
         * // Attempt to get and brighten the current hair texture (note that counter is a variable outside of this snippet)
         * response = api.GetCurrentAppearanceId(IFashionSenseApi.Type.Hair);
         * if (response.Key is true)
         * {
         *    var appearanceId = response.Value;
         *    var dataResponse = api.GetAppearanceTexture(appearanceId);
         *    if (dataResponse.Key is true)
         *    {
         *       var textureData = dataResponse.Value;
         *       Color[] colors = new Color[textureData.Width * textureData.Height];
         *
         *       bool shouldReset = false;
         *       for (int i = 0; i < textureData.Data.Length; i++)
         *       {
         *          var pixel = textureData.Data[i];
         *          if (pixel != Color.Transparent)
         *          {
         *              var red = pixel.R + 10 > 255 ? 255 : pixel.R + 10;
         *              var green = pixel.G + 10 > 255 ? 255 : pixel.G + 10;
         *              var blue = pixel.B + 10 > 255 ? 255 : pixel.B + 10;
         *
         *              colors[i] = new Color(red, green, blue);
         *              if (red == 255 && green == 255 && blue == 255)
         *              {
         *                  shouldReset = true;
         *              }
         *          }
         *          else
         *          {
         *              colors[i] = pixel;
         *          }
         *       }
         *       
         *       // Reset the texture after 15 brighten cycles
         *       if (shouldReset && counter >= 15)
         *       {
         *          response = api.ResetAppearanceTexture(appearanceId, this.ModManifest);
         *          counter = 0;
         *          
         *          Monitor.Log(response.Value, LogLevel.Debug);
         *       }
         *       else
         *       {
         *          response = api.SetAppearanceTexture(appearanceId, new IFashionSenseApi.RawTextureData(textureData.Width, textureData.Height, colors), this.ModManifest);
         *          counter += 1;
         *
         *          Monitor.Log(response.Value, LogLevel.Debug);
         *       }
         *    }
         * }
         */
    }

    public class Api : IApi
    {
        private IMonitor _monitor;
        private readonly TextureManager _textureManager;
        private readonly AccessoryManager _accessoryManager;
        private Dictionary<IApi.Type, Dictionary<IManifest, Func<IDrawTool, bool>>> appearanceTypeToDrawOverrides;

        // Events
        public event EventHandler SetSpriteDirtyTriggered;

        internal Api(IMonitor monitor, TextureManager textureManager, AccessoryManager accessoryManager)
        {
            _monitor = monitor;
            _textureManager = textureManager;
            _accessoryManager = accessoryManager;
            appearanceTypeToDrawOverrides = new Dictionary<IApi.Type, Dictionary<IManifest, Func<IDrawTool, bool>>>();
        }

        internal bool HandleDrawOverride(IApi.Type appearanceType, IDrawTool drawTool)
        {
            if (appearanceTypeToDrawOverrides.ContainsKey(appearanceType) is false)
            {
                return false;
            }

            foreach (var keyPair in appearanceTypeToDrawOverrides[appearanceType])
            {
                var manifest = keyPair.Key;
                var appearanceDrawOverrideMethod = keyPair.Value;

                if (appearanceDrawOverrideMethod(drawTool) is true)
                {
                    _monitor.LogOnce($"Draw logic for appearance type {appearanceType} was overriden by {manifest.UniqueID}", LogLevel.Trace);
                    return true;
                }
            }

            return false;
        }

        internal void OnSetSpriteDirtyTriggered(EventArgs e)
        {
            EventHandler handler = SetSpriteDirtyTriggered;
            if (handler is not null)
            {
                handler(this, e);
            }
        }

        private string GetAppearanceModDataKey(IApi.Type appearanceType)
        {
            string modDataKey = String.Empty;

            switch (appearanceType)
            {
                case IApi.Type.Hat:
                    modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                    break;
                case IApi.Type.Hair:
                    modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                    break;
                case IApi.Type.Accessory:
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_ID;
                    break;
                case IApi.Type.AccessorySecondary:
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID;
                    break;
                case IApi.Type.AccessoryTertiary:
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID;
                    break;
                case IApi.Type.Shirt:
                    modDataKey = ModDataKeys.CUSTOM_SHIRT_ID;
                    break;
                case IApi.Type.Sleeves:
                    modDataKey = ModDataKeys.CUSTOM_SLEEVES_ID;
                    break;
                case IApi.Type.Pants:
                    modDataKey = ModDataKeys.CUSTOM_PANTS_ID;
                    break;
                case IApi.Type.Shoes:
                    modDataKey = ModDataKeys.CUSTOM_SHOES_ID;
                    break;
            }

            return modDataKey;
        }

        private KeyValuePair<bool, string> GenerateResponsePair(bool wasSuccessful, string responseText)
        {
            return new KeyValuePair<bool, string>(wasSuccessful, responseText);
        }

        private bool SetFarmerAppearance(string appearanceId, IApi.Type appearanceType)
        {
            string modDataKey = GetAppearanceModDataKey(appearanceType);
            if (String.IsNullOrEmpty(modDataKey))
            {
                return false;
            }

            Game1.player.modData[modDataKey] = appearanceId;
            FashionSense.SetSpriteDirty();

            return true;
        }

        private string GetAppearanceId(string packId, IApi.Type appearanceType, string appearanceName)
        {
            return String.Concat(packId, "/", appearanceType, "/", appearanceName);
        }

        private bool IsAppearanceIdValid(string appearanceId)
        {
            if (_textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId) is null)
            {
                return false;
            }

            return true;
        }

        private KeyValuePair<bool, string> SetFashionSenseAppearance(IApi.Type packType, string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            if (callerManifest is null || String.IsNullOrEmpty(callerManifest.Name))
            {
                return GenerateResponsePair(false, "Given manifest is null or invalid. This API endpoint requires the caller to pass over their manifest in order to inform players of forceful changes.");
            }

            var appearanceId = GetAppearanceId(targetPackId, packType, targetAppearanceName);
            if (IsAppearanceIdValid(appearanceId) is false)
            {
                return GenerateResponsePair(false, $"No Fashion Sense {packType} found with the id of {appearanceId}");
            }

            // See if we need to reset the texture, if it is dirty
            if (FashionSense.ResetTextureIfNecessary(appearanceId) is false)
            {
                return GenerateResponsePair(false, $"No Fashion Sense {packType} found with the id of {appearanceId}");
            }

            // Attempt to set the sprite
            if (packType is IApi.Type.Accessory || packType is IApi.Type.AccessorySecondary || packType is IApi.Type.AccessoryTertiary)
            {
                if (packType is IApi.Type.Accessory)
                {
                    SetAccessorySlot(appearanceId, 0);
                }
                else if (packType is IApi.Type.AccessorySecondary)
                {
                    SetAccessorySlot(appearanceId, 1);
                }
                else if (packType is IApi.Type.AccessoryTertiary)
                {
                    SetAccessorySlot(appearanceId, 2);
                }

                FashionSense.SetSpriteDirty();
            }
            else if (SetFarmerAppearance(appearanceId, packType) is false)
            {
                return GenerateResponsePair(false, $"Failed to set the {packType} appearance with the id of {appearanceId}");
            }

            // Alert the player of the forceful change
            _monitor.Log($"The mod {callerManifest.Name} changed your Fashion Sense {packType} appearance.", LogLevel.Info);

            return GenerateResponsePair(true, $"Successfully set the {packType} appearance with the id of {appearanceId}");
        }

        private KeyValuePair<bool, string> ClearFashionSenseAppearance(IApi.Type packType, IManifest callerManifest)
        {
            if (callerManifest is null || String.IsNullOrEmpty(callerManifest.Name))
            {
                return GenerateResponsePair(false, "Given manifest is null or invalid. This API endpoint requires the caller to pass over their manifest in order to inform players of forceful changes.");
            }

            // Attempt to clear the sprite
            if (packType is IApi.Type.Accessory || packType is IApi.Type.AccessorySecondary || packType is IApi.Type.AccessoryTertiary)
            {
                _accessoryManager.ClearAccessories(Game1.player);
            }
            else if (SetFarmerAppearance("None", packType) is false)
            {
                return GenerateResponsePair(false, $"Failed to clear the {packType} appearance");
            }

            // Alert the player of the forceful change
            _monitor.Log($"The mod {callerManifest.Name} cleared your Fashion Sense {packType} appearance.", LogLevel.Info);

            return GenerateResponsePair(true, $"Successfully cleared the {packType} appearance");
        }


        public KeyValuePair<bool, string> SetAppearance(IApi.Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(appearanceType, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessorySlot(string accessoryId, int accessorySlot)
        {
            if (accessorySlot < 0)
            {
                return GenerateResponsePair(false, $"Invalid accessorySlot given: {accessorySlot}! Must be at least 0.");
            }

            if (IsAppearanceIdValid(accessoryId) is false)
            {
                return GenerateResponsePair(false, $"No Fashion Sense Accessory found with the id of {accessoryId}");
            }

            _accessoryManager.AddAccessory(Game1.player, accessoryId, accessorySlot, preserveColor: true);
            FashionSense.SetSpriteDirty();

            return GenerateResponsePair(true, $"Set farmer's accessory slot ({accessorySlot}) to accessory {accessoryId}.");
        }

        public KeyValuePair<bool, string> SetAppearanceColor(IApi.Type appearanceType, Color color, IManifest callerManifest)
        {
            switch (appearanceType)
            {
                case IApi.Type.Hat:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR] = color.PackedValue.ToString();
                    break;
                case IApi.Type.Hair:
                    Game1.player.changeHairColor(color);
                    break;
                case IApi.Type.Accessory:
                    _accessoryManager.SetColorForIndex(Game1.player, 0, color);
                    break;
                case IApi.Type.AccessorySecondary:
                    _accessoryManager.SetColorForIndex(Game1.player, 1, color);
                    break;
                case IApi.Type.AccessoryTertiary:
                    _accessoryManager.SetColorForIndex(Game1.player, 2, color);
                    break;
                case IApi.Type.Shirt:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR] = color.PackedValue.ToString();
                    break;
                case IApi.Type.Sleeves:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR] = color.PackedValue.ToString();
                    break;
                case IApi.Type.Pants:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR] = color.PackedValue.ToString();
                    break;
                case IApi.Type.Shoes:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR] = color.PackedValue.ToString();
                    break;
            }

            return GenerateResponsePair(true, $"Successfully set the color for {appearanceType} appearance to color {color.PackedValue}");
        }

        public KeyValuePair<bool, string> SetAccessoryColor(Color color, int accessorySlot)
        {
            if (accessorySlot < 0)
            {
                return GenerateResponsePair(false, $"Invalid accessorySlot given: {accessorySlot}! Must be at least 0.");
            }
            _accessoryManager.SetColorForIndex(Game1.player, accessorySlot, color);

            return GenerateResponsePair(true, $"Set color of farmer's accessory slot ({accessorySlot}) to {color.PackedValue}.");
        }

        #region Obsolete set methods
        public KeyValuePair<bool, string> SetHatAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Hat, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetHairAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Hair, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessoryPrimaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Accessory, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessorySecondaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.AccessorySecondary, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessoryTertiaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.AccessoryTertiary, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetShirtAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Shirt, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetSleevesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Sleeves, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetPantsAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Pants, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetShoesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(IApi.Type.Shoes, targetPackId, targetAppearanceName, callerManifest);
        }
        #endregion


        public KeyValuePair<bool, string> ClearAppearance(IApi.Type appearanceType, IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(appearanceType, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessorySlot(int accessorySlot, IManifest callerManifest)
        {
            if (accessorySlot < 0)
            {
                return GenerateResponsePair(false, $"Invalid accessorySlot given: {accessorySlot}! Must be at least 0.");
            }

            _accessoryManager.RemoveAccessory(Game1.player, accessorySlot);
            FashionSense.SetSpriteDirty();

            return GenerateResponsePair(true, $"Cleared farmer's accessory slot ({accessorySlot}).");
        }

        #region Obsolete clear methods
        public KeyValuePair<bool, string> ClearHatAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Hat, callerManifest);
        }

        public KeyValuePair<bool, string> ClearHairAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Hair, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessoryPrimaryAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Accessory, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessorySecondaryAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.AccessorySecondary, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessoryTertiaryAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.AccessoryTertiary, callerManifest);
        }

        public KeyValuePair<bool, string> ClearShirtAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Shirt, callerManifest);
        }

        public KeyValuePair<bool, string> ClearSleevesAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Sleeves, callerManifest);
        }

        public KeyValuePair<bool, string> ClearPantsAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Pants, callerManifest);
        }

        public KeyValuePair<bool, string> ClearShoesAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(IApi.Type.Shoes, callerManifest);
        }
        #endregion

        public KeyValuePair<bool, string> GetCurrentAppearanceId(IApi.Type appearanceType, Farmer target = null)
        {
            if (target is null)
            {
                target = Game1.player;
            }

            string modDataKey = GetAppearanceModDataKey(appearanceType);

            if (String.IsNullOrEmpty(modDataKey))
            {
                return GenerateResponsePair(false, $"No match for the IApi.Type value of: {appearanceType}");
            }

            if (target.modData.ContainsKey(modDataKey) is false)
            {
                return GenerateResponsePair(false, $"The player has not worn a Fashion Sense appearance of the type {appearanceType} | {modDataKey}");
            }

            var appearancePack = _textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(target.modData[modDataKey]);
            if (appearancePack is null)
            {
                return GenerateResponsePair(false, $"Invalid or deleted appearance pack is currently saved for the type {appearanceType} | {target.modData[modDataKey]}");
            }

            return GenerateResponsePair(true, appearancePack.Id);
        }

        public KeyValuePair<bool, Color> GetAppearanceColor(IApi.Type appearanceType, Farmer target = null)
        {
            if (target is null)
            {
                target = Game1.player;
            }

            string modDataKey = GetAppearanceModDataKey(appearanceType);

            if (String.IsNullOrEmpty(modDataKey))
            {
                return new KeyValuePair<bool, Color>(false, Color.White);
            }

            if (target.modData.ContainsKey(modDataKey) is false)
            {
                return new KeyValuePair<bool, Color>(false, Color.White);
            }

            var appearancePack = _textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(target.modData[modDataKey]);
            if (appearancePack is null)
            {
                return new KeyValuePair<bool, Color>(false, Color.White);
            }

            return new KeyValuePair<bool, Color>(true, FashionSense.colorManager.GetColor(target, AppearanceModel.GetColorKey(appearanceType)));
        }

        public KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(IApi.Type appearanceType, string targetPackId, string targetAppearanceName, bool getOriginalTexture = false)
        {
            return GetAppearanceTexture(GetAppearanceId(targetPackId, appearanceType, targetAppearanceName), getOriginalTexture);
        }

        public KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(string appearanceId, bool getOriginalTexture = false)
        {
            var appearancePack = _textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId);
            if (appearancePack is null)
            {
                return new KeyValuePair<bool, IRawTextureData>(false, null);
            }

            var texture = getOriginalTexture is false ? appearancePack.Texture : appearancePack.GetCachedTexture();
            Color[] data = new Color[appearancePack.Texture.Width * appearancePack.Texture.Height];
            appearancePack.Texture.GetData(data);

            return new KeyValuePair<bool, IRawTextureData>(true, new RawTextureData(texture.Width, texture.Height, data));
        }

        public KeyValuePair<bool, string> SetAppearanceTexture(IApi.Type appearanceType, string targetPackId, string targetAppearanceName, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false)
        {
            return SetAppearanceTexture(GetAppearanceId(targetPackId, appearanceType, targetAppearanceName), textureData, callerManifest, shouldOverridePersist);
        }

        public KeyValuePair<bool, string> SetAppearanceTexture(string appearanceId, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false)
        {
            if (callerManifest is null || String.IsNullOrEmpty(callerManifest.Name))
            {
                return GenerateResponsePair(false, "Given manifest is null or invalid. This API endpoint requires the caller to pass over their manifest in order to inform players of forceful changes.");
            }

            var appearancePack = _textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId);
            if (appearancePack is null)
            {
                return GenerateResponsePair(false, $"Invalid or deleted appearance pack is currently saved for {appearanceId}!");
            }

            Color[] currentTextureData = new Color[appearancePack.Texture.Width * appearancePack.Texture.Height];
            appearancePack.Texture.GetData(currentTextureData);

            if (textureData.Data.Length != currentTextureData.Length)
            {
                return GenerateResponsePair(false, $"The given textureData.Data.Length ({textureData.Data.Length}) doesn't match the current appearance's texture length ({currentTextureData.Length}): {appearanceId}!");
            }

            try
            {
                appearancePack.Texture.SetData(textureData.Data);
                appearancePack.IsTextureDirty = shouldOverridePersist is false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Setting texture via SetAppearanceTexture failed with the following error: {ex}", LogLevel.Trace);
                return GenerateResponsePair(true, $"Failed to set texture data, see the log for details.");
            }
            _monitor.LogOnce($"{callerManifest.Name} [{callerManifest.UniqueID}] overrode the texture for {appearanceId}!", LogLevel.Trace);

            return GenerateResponsePair(true, $"Successfully set the texture data for {appearanceId}!");
        }

        public KeyValuePair<bool, string> ResetAppearanceTexture(IApi.Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return ResetAppearanceTexture(GetAppearanceId(targetPackId, appearanceType, targetAppearanceName), callerManifest);
        }

        public KeyValuePair<bool, string> ResetAppearanceTexture(string appearanceId, IManifest callerManifest)
        {
            if (callerManifest is null || String.IsNullOrEmpty(callerManifest.Name))
            {
                return GenerateResponsePair(false, "Given manifest is null or invalid. This API endpoint requires the caller to pass over their manifest in order to inform players of forceful changes.");
            }

            var appearancePack = _textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId);
            if (appearancePack is null)
            {
                return GenerateResponsePair(false, $"Invalid or deleted appearance pack is currently saved for {appearanceId}!");
            }

            if (appearancePack.ResetTexture() is false)
            {
                return GenerateResponsePair(true, $"Failed to reset texture data, see the log for details.");
            }
            _monitor.LogOnce($"{callerManifest.Name} [{callerManifest.UniqueID}] reset the texture for {appearanceId}!", LogLevel.Trace);

            return GenerateResponsePair(true, $"Successfully reset the texture data for {appearanceId}!");
        }

        public KeyValuePair<bool, List<string>> GetOutfitIds()
        {
            List<string> outfitIds = FashionSense.outfitManager.GetOutfits(Game1.player).Select(o => o.Name).ToList();

            return new KeyValuePair<bool, List<string>>(true, outfitIds);
        }

        public KeyValuePair<bool, string> GetCurrentOutfitId()
        {
            if (Game1.player.modData.ContainsKey(ModDataKeys.CURRENT_OUTFIT_ID) is false)
            {
                return GenerateResponsePair(false, $"Player has not selected an outfit!");
            }

            return GenerateResponsePair(true, Game1.player.modData[ModDataKeys.CURRENT_OUTFIT_ID]);
        }

        public KeyValuePair<bool, string> SetCurrentOutfitId(string outfitId, IManifest callerManifest)
        {
            if (FashionSense.outfitManager.DoesOutfitExist(Game1.player, outfitId) is false)
            {
                return GenerateResponsePair(false, $"No outfit matched for {outfitId}!");
            }
            _monitor.Log($"The mod {callerManifest.Name} set your Fashion Sense outfit to {outfitId}.", LogLevel.Info);

            Outfit outfit = FashionSense.outfitManager.GetOutfit(Game1.player, outfitId);
            FashionSense.outfitManager.SetOutfit(Game1.player, outfit);

            return GenerateResponsePair(true, $"Player's outfit has been set to {outfitId}.");
        }

        public KeyValuePair<bool, string> SetAppearanceLockState(IApi.Type appearanceType, string targetPackId, string targetAppearanceName, bool isLocked, IManifest callerManifest)
        {
            var appearanceId = GetAppearanceId(targetPackId, appearanceType, targetAppearanceName);

            var appearancePack = _textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId);
            if (appearancePack is null)
            {
                return GenerateResponsePair(false, $"Invalid or deleted appearance pack is currently saved for {appearanceId}!");
            }
            appearancePack.IsLocked = isLocked;

            _monitor.Log($"The mod {callerManifest.Name} set IsLocked to {isLocked} for the appearance {appearanceId}.", LogLevel.Trace);
            return GenerateResponsePair(true, $"Set IsLocked to {isLocked} for the appearance {appearanceId}.");
        }

        public KeyValuePair<bool, string> RegisterAppearanceDrawOverride(IApi.Type appearanceType, IManifest callerManifest, Func<IDrawTool, bool> appearanceDrawOverride)
        {
            if (appearanceTypeToDrawOverrides.ContainsKey(appearanceType) is false)
            {
                appearanceTypeToDrawOverrides[appearanceType] = new Dictionary<IManifest, Func<IDrawTool, bool>>();
            }

            appearanceTypeToDrawOverrides[appearanceType][callerManifest] = appearanceDrawOverride;

            _monitor.Log($"The mod {callerManifest.Name} registered a draw override for the appearance type {appearanceType}.", LogLevel.Info);
            return GenerateResponsePair(true, $"Registered the draw override for the appearance type {appearanceType}.");
        }

        public KeyValuePair<bool, string> UnregisterAppearanceDrawOverride(IApi.Type appearanceType, IManifest callerManifest)
        {
            if (appearanceTypeToDrawOverrides.ContainsKey(appearanceType) is false || appearanceTypeToDrawOverrides[appearanceType].ContainsKey(callerManifest) is false)
            {
                return GenerateResponsePair(false, $"There were no registered draw overrides under {callerManifest.Name} for the appearance type {appearanceType}.");
            }
            appearanceTypeToDrawOverrides[appearanceType].Remove(callerManifest);

            return GenerateResponsePair(true, $"Unregistered the draw override for the appearance type {appearanceType}.");
        }

        public KeyValuePair<bool, string> IsDrawOverrideActive(IApi.Type appearanceType, IManifest callerManifest)
        {
            if (appearanceTypeToDrawOverrides.ContainsKey(appearanceType) is false || appearanceTypeToDrawOverrides[appearanceType].ContainsKey(callerManifest) is false)
            {
                return GenerateResponsePair(false, $"There were no registered draw overrides under {callerManifest.Name} for the appearance type {appearanceType}.");
            }

            return GetCurrentAppearanceId(appearanceType);
        }
    }
}
