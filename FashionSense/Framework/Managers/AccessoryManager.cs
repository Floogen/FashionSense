using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class AccessoryManager
    {
        private IMonitor _monitor;
        private const int MAX_ACCESSORY_LIMIT = 100;

        public AccessoryManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal IEnumerable<int> GetAllPossibleIndices()
        {
            return Enumerable.Range(0, 100);
        }

        internal void SetAccessories(Farmer who, List<string> accessoryIds, List<string> colors)
        {
            try
            {
                ClearAccessories(who);

                foreach (string accessoryId in accessoryIds)
                {
                    var index = AddAccessory(who, accessoryId, skipCacheUpdate: true);
                    who.modData[GetKeyForAccessoryColor(index)] = uint.Parse(colors[index]).ToString();

                    UpdateAccessoryCache(who);
                }
            }
            catch (Exception)
            {
                // TODO: Display error
            }
        }

        internal bool CopyAccessories(Farmer sourceFarmer, Farmer destinationFarmer)
        {
            foreach (int index in GetActiveAccessoryIndices(sourceFarmer))
            {
                ResetAccessory(destinationFarmer, index);

                destinationFarmer.modData[GetKeyForAccessoryId(index)] = GetAccessoryIdByIndex(sourceFarmer, index);
                destinationFarmer.modData[GetKeyForAccessoryColor(index)] = sourceFarmer.modData[GetKeyForAccessoryColor(index)];
            }

            return true;
        }

        internal void ClearAccessories(Farmer who)
        {
            for (int i = 0; i < MAX_ACCESSORY_LIMIT; i++)
            {
                RemoveAccessory(who, i);
            }
        }

        internal int AddAccessory(Farmer who, string accessoryId, int index = -1, bool preserveColor = false, bool skipCacheUpdate = false)
        {
            if (index == -1)
            {
                for (int i = 0; i < MAX_ACCESSORY_LIMIT; i++)
                {
                    if (who.modData.ContainsKey(GetKeyForAccessoryId(i)) is false)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index > -1)
            {
                ResetAccessory(who, index);

                who.modData[GetKeyForAccessoryId(index)] = accessoryId;
                who.modData[GetKeyForAccessoryColor(index)] = preserveColor is true && who.modData.ContainsKey(GetKeyForAccessoryColor(index)) is true && String.IsNullOrEmpty(who.modData[GetKeyForAccessoryColor(index)]) is false ? who.modData[GetKeyForAccessoryColor(index)] : Color.White.PackedValue.ToString();

                if (skipCacheUpdate is false)
                {
                    UpdateAccessoryCache(who);
                }

                return index;
            }

            return 0;
        }

        internal void RemoveAccessory(Farmer who, int index)
        {
            var idKey = GetKeyForAccessoryId(index);
            if (who.modData.ContainsKey(idKey))
            {
                who.modData.Remove(idKey);
            }

            var colorKey = GetKeyForAccessoryColor(index);
            if (who.modData.ContainsKey(colorKey))
            {
                who.modData.Remove(colorKey);
            }

            UpdateAccessoryCache(who);
        }

        internal void SetColorForIndex(Farmer who, int index, Color color, int maskLayerIndex = 0)
        {
            who.modData[GetKeyForAccessoryColor(index, maskLayerIndex)] = color.PackedValue.ToString();

            UpdateAccessoryCache(who);
        }

        internal void UpdateAccessoryCache(Farmer who)
        {
            List<string> accessoryIds = new List<string>();
            List<string> accessoryColors = new List<string>();
            foreach (int index in GetActiveAccessoryIndices(who))
            {
                accessoryIds.Add(GetAccessoryIdByIndex(who, index));
                accessoryColors.Add(GetColorFromIndex(who, index).PackedValue.ToString());
            }

            who.modData[ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID] = JsonConvert.SerializeObject(accessoryIds);
            who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLLECTIVE_COLOR] = JsonConvert.SerializeObject(accessoryColors);
        }

        internal string GetKeyForAccessoryId(int index)
        {
            return $"FashionSense.CustomAccessory.{index}.Id";
        }

        internal string GetKeyForAccessoryColor(int index, int maskLayerIndex = 0)
        {
            if (maskLayerIndex > 0)
            {
                return $"FashionSense.CustomAccessory.{index}.Color.{maskLayerIndex}.Mask";
            }
            return $"FashionSense.CustomAccessory.{index}.Color";
        }

        internal int GetAccessoryIndexById(Farmer who, string accessoryId)
        {
            foreach (int index in GetActiveAccessoryIndices(who))
            {
                var idKey = GetKeyForAccessoryId(index);
                if (IsKeyValid(who, idKey) && who.modData[idKey] == accessoryId)
                {
                    return index;
                }
            }

            return -1;
        }

        internal bool IsKeyValid(Farmer who, string key, bool checkForValue = false)
        {
            if (who is null || who.modData.ContainsKey(key) is false)
            {
                return false;
            }

            if (checkForValue is true && String.IsNullOrEmpty(who.modData[key]))
            {
                return false;
            }

            return true;
        }

        internal Color GetColorFromIndex(Farmer who, int index, int maskLayerIndex = 0)
        {
            var colorKey = GetKeyForAccessoryColor(index, maskLayerIndex);
            if (IsKeyValid(who, colorKey, checkForValue: true) && uint.TryParse(who.modData[colorKey], out var parsedColor))
            {
                return new Color(parsedColor);
            }

            return Color.White;
        }

        internal string GetAccessoryIdByIndex(Farmer who, int index)
        {
            var idKey = GetKeyForAccessoryId(index);
            if (who.modData.ContainsKey(idKey))
            {
                return who.modData[idKey];
            }

            return null;
        }

        internal IEnumerable<int> GetActiveAccessoryIndices(Farmer who)
        {
            List<int> activeIndices = new List<int>();
            foreach (int index in GetAllPossibleIndices())
            {
                if (who.modData.ContainsKey(GetKeyForAccessoryId(index)) is true)
                {
                    activeIndices.Add(index);
                }
            }

            return activeIndices;
        }

        internal List<string> GetActiveAccessoryIds(Farmer who)
        {
            List<string> accessoryIds = new List<string>();
            foreach (int index in GetActiveAccessoryIndices(who))
            {
                accessoryIds.Add(FashionSense.accessoryManager.GetAccessoryIdByIndex(who, index));
            }

            return accessoryIds;
        }

        internal List<string> GetActiveAccessoryColorValues(Farmer who)
        {
            List<string> accessoryColorValues = new List<string>();
            foreach (int index in GetActiveAccessoryIndices(who))
            {
                accessoryColorValues.Add(FashionSense.accessoryManager.GetColorFromIndex(who, index).PackedValue.ToString());
            }

            return accessoryColorValues;
        }

        internal void ResetAccessory(Farmer who, int index, int startingIndex = 0)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, GetKeyForAccessoryId(index));
            if (animationData is null)
            {
                return;
            }

            animationData.Reset(0, 0);
            animationData.StartingIndex = startingIndex;

            if (animationData.LightId is not null && Game1.currentLocation.sharedLights.ContainsKey(animationData.LightId.Value))
            {
                Game1.currentLocation.sharedLights.Remove(animationData.LightId.Value);
            }
            animationData.LightId = null;
        }

        internal void ResetAccessory(int index, Farmer who, int duration, AnimationModel.Type animationType, bool ignoreAnimationType = false, int startingIndex = 0)
        {
            var animationData = FashionSense.animationManager.GetSpecificAnimationData(who, GetKeyForAccessoryId(index));
            if (animationData is null)
            {
                return;
            }

            animationData.Reset(duration, who.FarmerSprite.CurrentFrame);
            animationData.StartingIndex = startingIndex;

            if (ignoreAnimationType is false)
            {
                animationData.Type = animationType;
            }
        }

        internal void ResetAllAccessories(Farmer who)
        {
            foreach (int index in GetActiveAccessoryIndices(who))
            {
                ResetAccessory(who, index);
            }
        }

        internal bool HandleOldAccessoryFormat(Farmer player)
        {
            var accessoryIds = new List<string>();
            var accessoryColors = new List<string>();

            if (player.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && String.IsNullOrEmpty(player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]) is false)
            {
                accessoryIds.Add(player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]);
                player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID] = null;

                accessoryColors.Add(player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR]);
                player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR] = null;
            }
            if (player.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID) && String.IsNullOrEmpty(player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]) is false)
            {
                accessoryIds.Add(player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]);
                player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID] = null;

                accessoryColors.Add(player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR]);
                player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR] = null;
            }
            if (player.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID) && String.IsNullOrEmpty(player.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID]) is false)
            {
                accessoryIds.Add(player.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID]);
                player.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID] = null;

                accessoryColors.Add(player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR]);
                player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR] = null;
            }

            // If any old accessories were detected, import them
            if (accessoryIds.Count > 0)
            {
                SetAccessories(player, accessoryIds, accessoryColors);
                return true;
            }

            return false;
        }
    }
}
