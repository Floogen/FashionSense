using FashionSense.Framework.Managers;
using FashionSense.Framework.Models;
using FashionSense.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Interfaces.API
{
    public interface IApi
    {
        KeyValuePair<bool, string> SetHatAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetHairAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessoryPrimaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessorySecondaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessoryTertiaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetShirtAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetSleevesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetPantsAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetShoesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

        KeyValuePair<bool, string> ClearHatAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearHairAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearAccessoryPrimaryAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearAccessorySecondaryAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearAccessoryTertiaryAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearShirtAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearSleevesAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearPantsAppearance(IManifest callerManifest);
        KeyValuePair<bool, string> ClearShoesAppearance(IManifest callerManifest);

        /*
         * Example usage (using the Fashion Sense example pack)
         * 
         * var api = Helper.ModRegistry.GetApi<IApi>(this.ModManifest.UniqueID);
         * var response = api.SetHatAppearance("ExampleAuthor.ExampleFashionSensePack", "Animated Pumpkin Head", this.ModManifest);
         * if (response.Key is true)
         * {
         *     // Setting was successful!
         * }
         * 
         */
    }

    public class Api : IApi
    {
        private IMonitor _monitor;
        private readonly TextureManager _textureManager;

        internal Api(IMonitor monitor, TextureManager textureManager)
        {
            _monitor = monitor;
            _textureManager = textureManager;
        }

        private KeyValuePair<bool, string> GenerateResponsePair(bool wasSuccessful, string responseText)
        {
            return new KeyValuePair<bool, string>(wasSuccessful, responseText);
        }

        private bool SetFarmerAppearance(string appearanceId, AppearanceContentPack.Type packType)
        {
            string modDataKey = String.Empty;

            switch (packType)
            {
                case AppearanceContentPack.Type.Hat:
                    modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                    break;
                case AppearanceContentPack.Type.Hair:
                    modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                    break;
                case AppearanceContentPack.Type.Accessory:
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_ID;
                    break;
                case AppearanceContentPack.Type.AccessorySecondary:
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID;
                    break;
                case AppearanceContentPack.Type.AccessoryTertiary:
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID;
                    break;
                case AppearanceContentPack.Type.Shirt:
                    modDataKey = ModDataKeys.CUSTOM_SHIRT_ID;
                    break;
                case AppearanceContentPack.Type.Sleeves:
                    modDataKey = ModDataKeys.CUSTOM_SLEEVES_ID;
                    break;
                case AppearanceContentPack.Type.Pants:
                    modDataKey = ModDataKeys.CUSTOM_PANTS_ID;
                    break;
                case AppearanceContentPack.Type.Shoes:
                    modDataKey = ModDataKeys.CUSTOM_SHOES_ID;
                    break;
            }

            if (String.IsNullOrEmpty(modDataKey))
            {
                return false;
            }

            Game1.player.modData[modDataKey] = appearanceId;
            FashionSense.SetSpriteDirty();

            return true;
        }

        private string GetAppearanceId(string packId, AppearanceContentPack.Type packType, string appearanceName)
        {
            return String.Concat(packId, "/", packType, "/", appearanceName);
        }

        private bool IsAppearanceIdValid(string appearanceId)
        {
            if (_textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId) is null)
            {
                return false;
            }

            return true;
        }

        private KeyValuePair<bool, string> SetFashionSenseAppearance(AppearanceContentPack.Type packType, string targetPackId, string targetAppearanceName, IManifest callerManifest)
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

            // Attempt to set the sprite
            if (SetFarmerAppearance(appearanceId, packType) is false)
            {
                return GenerateResponsePair(false, $"Failed to set the {packType} appearance with the id of {appearanceId}");
            }

            // Alert the player of the forceful change
            _monitor.Log($"The mod {callerManifest.Name} changed your Fashion Sense {packType} appearance.", LogLevel.Info);

            return GenerateResponsePair(true, $"Successfully set the {packType} appearance with the id of {appearanceId}");
        }

        private KeyValuePair<bool, string> ClearFashionSenseAppearance(AppearanceContentPack.Type packType, IManifest callerManifest)
        {
            if (callerManifest is null || String.IsNullOrEmpty(callerManifest.Name))
            {
                return GenerateResponsePair(false, "Given manifest is null or invalid. This API endpoint requires the caller to pass over their manifest in order to inform players of forceful changes.");
            }

            // Attempt to clear the sprite
            if (SetFarmerAppearance("None", packType) is false)
            {
                return GenerateResponsePair(false, $"Failed to clear the {packType} appearance");
            }

            // Alert the player of the forceful change
            _monitor.Log($"The mod {callerManifest.Name} cleared your Fashion Sense {packType} appearance.", LogLevel.Info);

            return GenerateResponsePair(true, $"Successfully cleared the {packType} appearance");
        }

        public KeyValuePair<bool, string> SetHatAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Hat, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetHairAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Hair, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessoryPrimaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Accessory, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessorySecondaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.AccessorySecondary, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetAccessoryTertiaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.AccessoryTertiary, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetShirtAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Shirt, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetSleevesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Sleeves, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetPantsAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Pants, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> SetShoesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest)
        {
            return SetFashionSenseAppearance(AppearanceContentPack.Type.Shoes, targetPackId, targetAppearanceName, callerManifest);
        }

        public KeyValuePair<bool, string> ClearHatAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Hat, callerManifest);
        }

        public KeyValuePair<bool, string> ClearHairAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Hair, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessoryPrimaryAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Accessory, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessorySecondaryAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.AccessorySecondary, callerManifest);
        }

        public KeyValuePair<bool, string> ClearAccessoryTertiaryAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.AccessoryTertiary, callerManifest);
        }

        public KeyValuePair<bool, string> ClearShirtAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Shirt, callerManifest);
        }

        public KeyValuePair<bool, string> ClearSleevesAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Sleeves, callerManifest);
        }

        public KeyValuePair<bool, string> ClearPantsAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Pants, callerManifest);
        }

        public KeyValuePair<bool, string> ClearShoesAppearance(IManifest callerManifest)
        {
            return ClearFashionSenseAppearance(AppearanceContentPack.Type.Shoes, callerManifest);
        }
    }
}
