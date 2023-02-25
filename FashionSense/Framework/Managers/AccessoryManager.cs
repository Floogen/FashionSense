using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FashionSense.Framework.Managers
{
    class AccessoryManager
    {
        private IMonitor _monitor;
        private const int MAX_ACCESSORY_LIMIT = 100;

        internal enum AnimationKey
        {
            Iterator,
            StartingIndex,
            FrameDuration,
            ElapsedDuration,
            LightId,
            FarmerFrame,
            AnimationType
        }

        public AccessoryManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal void SetAccessories(Farmer who, List<string> accessoryIds, List<string> colors)
        {
            try
            {
                ClearAccessories(who);

                foreach (string accessoryId in accessoryIds)
                {
                    var index = AddAccessory(who, accessoryId);
                    who.modData[GetKeyForAccessoryColor(index)] = uint.Parse(colors[index]).ToString();
                }
            }
            catch (Exception)
            {
                // TODO: Display error
            }
        }

        internal void ClearAccessories(Farmer who)
        {
            for (int i = 0; i < MAX_ACCESSORY_LIMIT; i++)
            {
                RemoveAccessory(who, i);
            }

            SetActiveAccessoryCount(who, 0);
        }

        internal int AddAccessory(Farmer who, string accessoryId, int index = -1)
        {
            if (index == -1)
            {
                index = GetActiveAccessoryCount(who);
            }

            if (index > -1)
            {
                if (who.modData.ContainsKey(GetKeyForAccessoryId(index)) is false)
                {
                    SetActiveAccessoryCount(who, GetActiveAccessoryCount(who) + 1);
                }
                ResetAccessory(who, index);

                who.modData[GetKeyForAccessoryId(index)] = accessoryId;
                who.modData[GetKeyForAccessoryColor(index)] = Color.White.PackedValue.ToString();

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
                SetActiveAccessoryCount(who, GetActiveAccessoryCount(who) - 1);
            }

            var colorKey = GetKeyForAccessoryColor(index);
            if (who.modData.ContainsKey(colorKey))
            {
                who.modData.Remove(colorKey);
            }
        }

        internal int GetActiveAccessoryCount(Farmer who)
        {
            if (IsKeyValid(who, ModDataKeys.ACTIVE_ACCESSORIES_COUNT) && Int32.TryParse(who.modData[ModDataKeys.ACTIVE_ACCESSORIES_COUNT], out int count) && count > 0)
            {
                return count;
            }

            return 0;
        }

        internal void SetActiveAccessoryCount(Farmer who, int count)
        {
            who.modData[ModDataKeys.ACTIVE_ACCESSORIES_COUNT] = count.ToString();
        }

        internal string GetKeyForAccessoryId(int index)
        {
            return $"FashionSense.CustomAccessory.{index}.Id";
        }

        internal string GetKeyForAccessoryColor(int index)
        {
            return $"FashionSense.CustomAccessory.{index}.Color";
        }

        internal int GetAccessoryIndexById(Farmer who, string accessoryId)
        {
            for (int i = 0; i < GetActiveAccessoryCount(who); i++)
            {
                var idKey = GetKeyForAccessoryId(i);
                if (IsKeyValid(who, idKey) && who.modData[idKey] == accessoryId)
                {
                    return i;
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

        internal Color GetColorFromIndex(Farmer who, int index)
        {
            var colorKey = GetKeyForAccessoryColor(index);
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

        internal List<int> GetActiveAccessoryIndices(Farmer who)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < GetActiveAccessoryCount(who); i++)
            {
                if (IsKeyValid(who, GetKeyForAccessoryId(i), checkForValue: true))
                {
                    indices.Add(i);
                }
            }

            return indices;
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

        internal static string GetAnimationKeyString(AnimationKey animationKey)
        {
            switch (animationKey)
            {
                case AnimationKey.AnimationType:
                    return "Type";
                case AnimationKey.Iterator:
                    return "Iterator";
                case AnimationKey.StartingIndex:
                    return "StartingIndex";
                case AnimationKey.FrameDuration:
                    return "FrameDuration";
                case AnimationKey.ElapsedDuration:
                    return "ElapsedDuration";
                case AnimationKey.LightId:
                    return "Light.Id";
                case AnimationKey.FarmerFrame:
                    return "FarmerFrame";
            }

            return null;
        }

        internal string GetModDataKey(Farmer who, AnimationKey animationKey, int index)
        {
            var idKey = GetKeyForAccessoryId(index);
            if (IsKeyValid(who, idKey, checkForValue: true) is false)
            {
                return null;
            }

            var animationString = GetAnimationKeyString(animationKey);
            if (String.IsNullOrEmpty(animationString))
            {
                return null;
            }

            return $"FashionSense.Animation.Accessory.{index}.{animationString}";
        }

        internal void SetModData(Farmer who, int index, AnimationKey animationType, string value)
        {
            var modDataKey = GetModDataKey(who, animationType, index);
            if (String.IsNullOrEmpty(modDataKey))
            {
                return;
            }

            who.modData[modDataKey] = value;
        }

        internal string GetModData(Farmer who, int index, AnimationKey animationType)
        {
            var modDataKey = GetModDataKey(who, animationType, index);
            if (String.IsNullOrEmpty(modDataKey) || who.modData.ContainsKey(modDataKey) is false)
            {
                return null;
            }

            return who.modData[modDataKey];
        }

        internal void ResetAccessory(Farmer who, int index, int startingIndex = 0)
        {
            if (GetModDataKey(who, AnimationKey.AnimationType, index) is null)
            {
                return;
            }

            who.modData[GetModDataKey(who, AnimationKey.Iterator, index)] = "0";
            who.modData[GetModDataKey(who, AnimationKey.StartingIndex, index)] = startingIndex.ToString();
            who.modData[GetModDataKey(who, AnimationKey.FrameDuration, index)] = "0";
            who.modData[GetModDataKey(who, AnimationKey.ElapsedDuration, index)] = "0";
            who.modData[GetModDataKey(who, AnimationKey.FarmerFrame, index)] = "0";
            who.modData[GetModDataKey(who, AnimationKey.LightId, index)] = "0";
        }

        internal void ResetAccessory(int index, Farmer who, int duration, AnimationModel.Type animationType, bool ignoreAnimationType = false, int startingIndex = 0)
        {
            ResetAccessory(who, index, startingIndex: startingIndex);

            if (ignoreAnimationType is false)
            {
                who.modData[GetModDataKey(who, AnimationKey.AnimationType, index)] = animationType.ToString();
            }

            who.modData[GetModDataKey(who, AnimationKey.FrameDuration, index)] = duration.ToString();
            who.modData[GetModDataKey(who, AnimationKey.FarmerFrame, index)] = who.FarmerSprite.CurrentFrame.ToString();
        }

        internal void ResetAllAccessories(Farmer who)
        {
            for (int i = 0; i < GetActiveAccessoryCount(who); i++)
            {
                ResetAccessory(who, i);
            }
        }

        internal void HandleOldAccessoryFormat(Farmer player)
        {
            var accessoryIds = new List<string>();
            var accessoryColors = new List<string>();
            if (String.IsNullOrEmpty(ModDataKeys.CUSTOM_ACCESSORY_ID) is false)
            {
                accessoryIds.Add(player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]);
                player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID] = null;

                accessoryColors.Add(player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR]);
                player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR] = null;
            }
            if (String.IsNullOrEmpty(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID) is false)
            {
                accessoryIds.Add(player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]);
                player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID] = null;

                accessoryColors.Add(player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR]);
                player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR] = null;
            }
            if (String.IsNullOrEmpty(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID) is false)
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
            }
        }
    }
}
