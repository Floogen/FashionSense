using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace FashionSense.Framework.Managers
{
    internal class ColorManager
    {
        private IMonitor _monitor;
        private Dictionary<Farmer, Dictionary<string, Color>> _farmerToColorIdToColorValue;

        public ColorManager(IMonitor monitor)
        {
            _monitor = monitor;
            _farmerToColorIdToColorValue = new Dictionary<Farmer, Dictionary<string, Color>>();
        }

        public Color GetColor(Farmer who, string colorKey)
        {
            Color colorValue = who.hairstyleColor.Value;
            if (_farmerToColorIdToColorValue.ContainsKey(who) is true && _farmerToColorIdToColorValue[who].ContainsKey(colorKey) is true)
            {
                colorValue = _farmerToColorIdToColorValue[who][colorKey];
            }

            return colorValue;
        }

        public void SetColor(Farmer who, string colorKey, Color colorValue)
        {
            if (_farmerToColorIdToColorValue.ContainsKey(who) is false)
            {
                _farmerToColorIdToColorValue[who] = new Dictionary<string, Color>();
            }

            who.modData[colorKey] = colorValue.PackedValue.ToString();
            _farmerToColorIdToColorValue[who][colorKey] = colorValue;
        }
    }
}
