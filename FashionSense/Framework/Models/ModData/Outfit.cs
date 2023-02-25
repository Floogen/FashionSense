using FashionSense.Framework.Utilities;
using StardewValley;

namespace FashionSense.Framework.Models
{
    public class Outfit
    {
        public string Name { get; set; }

        // Ids
        public string AccessoryOneId { get; set; }
        public string AccessoryTwoId { get; set; }
        public string AccessoryThreeId { get; set; }
        public string AccessoryIds { get; set; }
        public string HairId { get; set; }
        public string HatId { get; set; }
        public string ShirtId { get; set; }
        public string SleevesId { get; set; }
        public string PantsId { get; set; }
        public string ShoesId { get; set; }

        // Colors
        public string AccessoryOneColor { get; set; }
        public string AccessoryTwoColor { get; set; }
        public string AccessoryThreeColor { get; set; }
        public string AccessoryColors { get; set; }
        public string HairColor { get; set; }
        public string HatColor { get; set; }
        public string ShirtColor { get; set; }
        public string SleevesColor { get; set; }
        public string PantsColor { get; set; }
        public string ShoesColor { get; set; }

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
            AccessoryIds = who.modData[ModDataKeys.OUTFIT_ACCESSORY_IDS];
            HatId = who.modData[ModDataKeys.CUSTOM_HAT_ID];
            ShirtId = who.modData[ModDataKeys.CUSTOM_SHIRT_ID];
            SleevesId = who.modData[ModDataKeys.CUSTOM_SLEEVES_ID];
            PantsId = who.modData[ModDataKeys.CUSTOM_PANTS_ID];
            ShoesId = who.modData[ModDataKeys.CUSTOM_SHOES_ID];

            HairColor = who.hairstyleColor.Value.PackedValue.ToString();
            AccessoryOneColor = who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR];
            AccessoryTwoColor = who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR];
            AccessoryThreeColor = who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR];
            AccessoryColors = who.modData[ModDataKeys.OUTFIT_ACCESSORY_COLORS];
            HatColor = who.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR];
            ShirtColor = who.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR];
            SleevesColor = who.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR];
            PantsColor = who.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR];
            ShoesColor = who.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR];
        }
    }
}
