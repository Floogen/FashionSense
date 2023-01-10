using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FashionSense.Framework.Managers
{
    class AccessoryManager
    {
        private IMonitor _monitor;
        private List<AccessoryData> _accessoryData;

        public AccessoryManager(IMonitor monitor)
        {
            _monitor = monitor;
            _accessoryData = new List<AccessoryData>();
        }

        internal void SetAccessories(string accessories)
        {
            _accessoryData = new List<AccessoryData>();

            try
            {
                foreach (string accessoryId in JsonConvert.DeserializeObject<List<string>>(accessories))
                {
                    _accessoryData.Add(new AccessoryData(accessoryId));
                }
            }
            catch (Exception)
            {
                // TODO: Display error
            }
        }

        internal void AddAccessory(string accessoryId, int index = -1)
        {
            if (index >= 0 && _accessoryData.Count > index)
            {
                _accessoryData[index].Id = accessoryId;
                _accessoryData[index].Reset();
            }
            else
            {
                _accessoryData.Add(new AccessoryData(accessoryId));
            }
        }

        internal string GetAccessoryIdByIndex(int index)
        {
            if (_accessoryData.Count < index)
            {
                return null;
            }

            return _accessoryData[index].Id;
        }

        internal AccessoryData GetAccessoryDataByIndex(int index)
        {
            if (_accessoryData.Count < index)
            {
                return null;
            }

            return _accessoryData[index];
        }

        internal List<AccessoryData> GetAccessoryData()
        {
            return _accessoryData;
        }

        internal void HandleOldAccessoryFormat(Farmer player)
        {
            var accessories = new List<string>();
            if (String.IsNullOrEmpty(ModDataKeys.CUSTOM_ACCESSORY_ID) is false)
            {
                accessories.Add(Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]);
                Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID] = null;
            }
            if (String.IsNullOrEmpty(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID) is false)
            {
                accessories.Add(Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]);
                Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID] = null;
            }
            if (String.IsNullOrEmpty(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID) is false)
            {
                accessories.Add(Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID]);
                Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID] = null;
            }

            // If any old accessories were detected, import them
            if (accessories.Count > 0)
            {
                Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID] = JsonConvert.SerializeObject(accessories);
            }
        }
    }
}
