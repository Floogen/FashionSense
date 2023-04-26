using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace FashionSense.Framework.Managers
{
    internal class AnimationManager
    {
        private IMonitor _monitor;
        private Dictionary<Farmer, Dictionary<string, AnimationData>> _farmerToAppearanceIdToAppearanceAnimationData;

        public AnimationManager(IMonitor monitor)
        {
            _monitor = monitor;
            _farmerToAppearanceIdToAppearanceAnimationData = new Dictionary<Farmer, Dictionary<string, AnimationData>>();
        }

        public List<AnimationData> GetAllAnimationData(Farmer who)
        {
            List<AnimationData> animationData = new List<AnimationData>();
            if (_farmerToAppearanceIdToAppearanceAnimationData.ContainsKey(who) is false)
            {
                return animationData;
            }

            foreach (var pair in _farmerToAppearanceIdToAppearanceAnimationData[who])
            {
                animationData.Add(pair.Value);
            }

            return animationData;
        }

        public AnimationData GetSpecificAnimationData(Farmer who, AppearanceModel model)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, model.Pack.PackType);
            if (animationData is null && model is AccessoryModel)
            {
                var accessoryIndex = FashionSense.accessoryManager.GetAccessoryIndexById(who, model.Pack.Id);
                if (accessoryIndex == -1)
                {
                    return null;
                }

                animationData = FashionSense.animationManager.GetSpecificAnimationData(who, FashionSense.accessoryManager.GetKeyForAccessoryId(accessoryIndex));
            }

            return animationData;
        }

        public AnimationData GetSpecificAnimationData(Farmer who, string appearanceId)
        {
            if (_farmerToAppearanceIdToAppearanceAnimationData.ContainsKey(who) is false)
            {
                _farmerToAppearanceIdToAppearanceAnimationData[who] = new Dictionary<string, AnimationData>();
            }

            if (_farmerToAppearanceIdToAppearanceAnimationData[who].ContainsKey(appearanceId) is false)
            {
                _farmerToAppearanceIdToAppearanceAnimationData[who][appearanceId] = new AnimationData();
            }

            return _farmerToAppearanceIdToAppearanceAnimationData[who][appearanceId];
        }

        public AnimationData GetSpecificAnimationData(Farmer who, IApi.Type Type)
        {
            string appearanceId;
            switch (Type)
            {
                case IApi.Type.Pants:
                    appearanceId = ModDataKeys.CUSTOM_PANTS_ID;
                    break;
                case IApi.Type.Sleeves:
                    appearanceId = ModDataKeys.CUSTOM_SLEEVES_ID;
                    break;
                case IApi.Type.Shirt:
                    appearanceId = ModDataKeys.CUSTOM_SHIRT_ID;
                    break;
                case IApi.Type.Hair:
                    appearanceId = ModDataKeys.CUSTOM_HAIR_ID;
                    break;
                case IApi.Type.Hat:
                    appearanceId = ModDataKeys.CUSTOM_HAT_ID;
                    break;
                case IApi.Type.Shoes:
                    appearanceId = ModDataKeys.CUSTOM_SHOES_ID;
                    break;
                // Purposely returning null for accessories, as they require the full appearanceId to be passed over
                default:
                case IApi.Type.Accessory:
                case IApi.Type.AccessorySecondary:
                case IApi.Type.AccessoryTertiary:
                    return null;
            }

            return GetSpecificAnimationData(who, appearanceId);
        }
    }
}
