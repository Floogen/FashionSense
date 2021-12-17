using FashionSense.Framework.Models.Generic;
using FashionSense.Framework.Models.Generic.Random;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class Outfit
    {
        public string Name { get; set; }
        public string AccessoryOneId { get; set; }
        public string AccessoryTwoId { get; set; }
        public string AccessoryThreeId { get; set; }
        public string HairId { get; set; }
        public string HatId { get; set; }
        public string ShirtId { get; set; }
        public string SleevesId { get; set; }
        public string PantsId { get; set; }

        public Outfit()
        {

        }

        public Outfit(Farmer who, string name)
        {
            Name = name;
            AccessoryOneId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID];
            AccessoryTwoId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID];
            AccessoryThreeId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID];
            HairId = who.modData[ModDataKeys.CUSTOM_HAIR_ID];
            HatId = who.modData[ModDataKeys.CUSTOM_HAT_ID];
            ShirtId = who.modData[ModDataKeys.CUSTOM_SHIRT_ID];
            SleevesId = who.modData[ModDataKeys.CUSTOM_SLEEVES_ID];
            PantsId = who.modData[ModDataKeys.CUSTOM_PANTS_ID];
        }
    }
}
