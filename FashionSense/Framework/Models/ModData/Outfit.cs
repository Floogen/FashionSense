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

        // Ids
        public string AccessoryOneId { get; set; }
        public string AccessoryTwoId { get; set; }
        public string AccessoryThreeId { get; set; }
        public string HairId { get; set; }
        public string HatId { get; set; }
        public string ShirtId { get; set; }
        public string SleevesId { get; set; }
        public string PantsId { get; set; }

        // Colors
        public string AccessoryOneColor { get; set; }
        public string AccessoryTwoColor { get; set; }
        public string AccessoryThreeColor { get; set; }
        public string HairColor { get; set; }
        public string HatColor { get; set; }
        public string ShirtColor { get; set; }
        public string SleevesColor { get; set; }
        public string PantsColor { get; set; }

        public Outfit()
        {

        }

        public Outfit(Farmer who, string name)
        {
            Name = name;

            HairId = who.modData[ModDataKeys.CUSTOM_HAIR_ID];
            AccessoryOneId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID];
            AccessoryTwoId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID];
            AccessoryThreeId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID];
            HatId = who.modData[ModDataKeys.CUSTOM_HAT_ID];
            ShirtId = who.modData[ModDataKeys.CUSTOM_SHIRT_ID];
            SleevesId = who.modData[ModDataKeys.CUSTOM_SLEEVES_ID];
            PantsId = who.modData[ModDataKeys.CUSTOM_PANTS_ID];

            HairColor = who.hairstyleColor.Value.PackedValue.ToString();
            AccessoryOneColor = who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR];
            AccessoryTwoColor = who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR];
            AccessoryThreeColor = who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR];
            HatColor = who.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR];
            ShirtColor = who.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR];
            SleevesColor = who.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR];
            PantsColor = who.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR];
        }
    }
}
