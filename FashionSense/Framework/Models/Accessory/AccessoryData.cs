using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FashionSense.Framework.Managers.AccessoryManager;

namespace FashionSense.Framework.Models.Accessory
{
    internal class AccessoryData
    {
        // Other data
        internal string Id { get; set; }
        internal Color Color { get; set; }

        public AccessoryData(string id)
        {
            Id = id;

            Color = Color.White;
        }
    }
}
