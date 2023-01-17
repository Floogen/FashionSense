using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static StardewValley.HouseRenovation;

namespace FashionSense.Framework.Managers
{
    class AccessoryManager
    {
        private IMonitor _monitor;
        private List<AccessoryData> _accessoryData;

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
            _accessoryData = new List<AccessoryData>();
        }

        internal void SetAccessories(string accessories, string colors)
        {
            _accessoryData = new List<AccessoryData>();

            try
            {
                List<string> colorsParsed = JsonConvert.DeserializeObject<List<string>>(colors);

                int index = 0;
                foreach (string accessoryId in JsonConvert.DeserializeObject<List<string>>(accessories))
                {
                    AddAccessory(accessoryId, index);
                    _accessoryData[index].Color = new Color(uint.Parse(colorsParsed[index]));

                    index++;
                }
            }
            catch (Exception)
            {
                // TODO: Display error
            }
        }

        internal void AddAccessory(string accessoryId, int index = -1)
        {
            if (_accessoryData.ElementAtOrDefault(index) is not null)
            {
                _accessoryData[index].Id = accessoryId;
                ResetAccessory(index);
            }
            else
            {
                _accessoryData.Add(new AccessoryData(accessoryId));
            }
        }

        internal void RemoveAccessory(int index)
        {
            if (_accessoryData.ElementAtOrDefault(index) is not null)
            {
                _accessoryData.RemoveAt(index);
            }
        }

        internal int GetAccessoryIndexById(string accessoryId)
        {
            return _accessoryData.FindIndex(d => d.Id == accessoryId);
        }

        internal string GetAccessoryIdByIndex(int index)
        {
            if (_accessoryData.ElementAtOrDefault(index) is null)
            {
                return null;
            }

            return _accessoryData[index].Id;
        }

        internal AccessoryData GetAccessoryDataById(string accessoryId)
        {
            return _accessoryData.FirstOrDefault(d => d.Id == accessoryId);
        }

        internal AccessoryData GetAccessoryDataByIndex(int index)
        {
            if (_accessoryData.ElementAtOrDefault(index) is null)
            {
                return null;
            }

            return _accessoryData[index];
        }

        internal List<AccessoryData> GetAccessoryData()
        {
            return _accessoryData;
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

        internal string GetModDataKey(AnimationKey animationKey, int index)
        {
            if (_accessoryData.ElementAtOrDefault(index) is null)
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

        internal void SetModData(int index, AnimationKey animationType, string value)
        {
            var modDataKey = GetModDataKey(animationType, index);
            if (String.IsNullOrEmpty(modDataKey))
            {
                return;
            }

            Game1.player.modData[modDataKey] = value;
        }

        internal string GetModData(int index, AnimationKey animationType)
        {
            var modDataKey = GetModDataKey(animationType, index);
            if (String.IsNullOrEmpty(modDataKey) || Game1.player.modData.ContainsKey(modDataKey) is false)
            {
                return null;
            }

            return Game1.player.modData[modDataKey];
        }

        internal void ResetAccessory(int index, int startingIndex = 0)
        {
            Game1.player.modData[GetModDataKey(AnimationKey.AnimationType, index)] = AnimationModel.Type.Unknown.ToString();
            Game1.player.modData[GetModDataKey(AnimationKey.Iterator, index)] = "0";
            Game1.player.modData[GetModDataKey(AnimationKey.StartingIndex, index)] = startingIndex.ToString();
            Game1.player.modData[GetModDataKey(AnimationKey.FrameDuration, index)] = "0";
            Game1.player.modData[GetModDataKey(AnimationKey.ElapsedDuration, index)] = "0";
            Game1.player.modData[GetModDataKey(AnimationKey.FarmerFrame, index)] = "0";
            Game1.player.modData[GetModDataKey(AnimationKey.LightId, index)] = "0";
        }

        internal void ResetAccessory(int index, Farmer who, int duration, AnimationModel.Type animationType, bool ignoreAnimationType = false, int startingIndex = 0)
        {
            ResetAccessory(index, startingIndex: startingIndex);

            if (ignoreAnimationType is false)
            {
                Game1.player.modData[GetModDataKey(AnimationKey.AnimationType, index)] = animationType.ToString();
            }

            Game1.player.modData[GetModDataKey(AnimationKey.FrameDuration, index)] = duration.ToString();
            Game1.player.modData[GetModDataKey(AnimationKey.FarmerFrame, index)] = who.FarmerSprite.CurrentFrame.ToString();
        }

        internal void ResetAllAccessories()
        {
            int index = 0;
            foreach (var accessory in _accessoryData)
            {
                ResetAccessory(index);
                index++;
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
                player.modData[ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID] = JsonConvert.SerializeObject(accessoryIds);
                player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLLECTIVE_COLOR] = JsonConvert.SerializeObject(accessoryColors);
            }
        }
    }
}
