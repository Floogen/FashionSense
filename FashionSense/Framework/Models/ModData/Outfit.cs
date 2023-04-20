using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Patches.Renderer;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FashionSense.Framework.Models
{
    public class Outfit
    {
        public string Name { get; set; }
        public int Version { get; set; } = 1;
        public bool IsBeingShared { get; set; }
        public bool IsGlobal { get; set; }

        // Ids
        [Obsolete("No longer used as of Fashion Sense v5, use AccessoryIds instead.")]
        public string AccessoryOneId { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5, use AccessoryIds instead.")]
        public string AccessoryTwoId { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5, use AccessoryIds instead.")]
        public string AccessoryThreeId { get; set; }
        public List<string> AccessoryIds { get; set; }
        public string HairId { get; set; }
        public string HatId { get; set; }
        public string ShirtId { get; set; }
        public string SleevesId { get; set; }
        public string PantsId { get; set; }
        public string ShoesId { get; set; }

        // Colors
        [Obsolete("No longer used as of Fashion Sense v5, use AccessoryColors instead.")]
        public string AccessoryOneColor { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5, use AccessoryColors instead.")]
        public string AccessoryTwoColor { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5, use AccessoryColors instead.")]
        public string AccessoryThreeColor { get; set; }
        public List<string> AccessoryColors { get; set; }
        public string HairColor { get; set; }

        [Obsolete("No longer used as of Fashion Sense v5.5, use AppearanceToMaskColors instead.")]
        public string HatColor { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5.5, use AppearanceToMaskColors instead.")]
        public string ShirtColor { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5.5, use AppearanceToMaskColors instead.")]
        public string SleevesColor { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5.5, use AppearanceToMaskColors instead.")]
        public string PantsColor { get; set; }
        [Obsolete("No longer used as of Fashion Sense v5.5, use AppearanceToMaskColors instead.")]
        public string ShoesColor { get; set; }
        public Dictionary<AppearanceContentPack.Type, List<Color>> AppearanceToMaskColors { get; set; }

        public Outfit()
        {

        }

        public Outfit(Farmer who, string name)
        {
            Name = name;
            Version = 3;

            HairId = who.modData[ModDataKeys.CUSTOM_HAIR_ID];
            AccessoryOneId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID];
            AccessoryTwoId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID];
            AccessoryThreeId = who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID];
            AccessoryIds = FashionSense.accessoryManager.GetActiveAccessoryIds(who);
            HatId = who.modData[ModDataKeys.CUSTOM_HAT_ID];
            ShirtId = who.modData[ModDataKeys.CUSTOM_SHIRT_ID];
            SleevesId = who.modData[ModDataKeys.CUSTOM_SLEEVES_ID];
            PantsId = who.modData[ModDataKeys.CUSTOM_PANTS_ID];
            ShoesId = who.modData[ModDataKeys.CUSTOM_SHOES_ID];

            HairColor = who.hairstyleColor.Value.PackedValue.ToString();
            AccessoryColors = FashionSense.accessoryManager.GetActiveAccessoryColorValues(who);

            AppearanceToMaskColors = new Dictionary<AppearanceContentPack.Type, List<Color>>();
            foreach (var metadata in DrawPatch.GetCurrentlyEquippedModels(who, who.FacingDirection))
            {
                if (metadata is null)
                {
                    continue;
                }

                AppearanceToMaskColors[metadata.Model.Pack.PackType] = metadata.Colors;
            }

            // Add manual handling for the "Override Shoe Color" artificial ShoePack
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) && who.modData[ModDataKeys.CUSTOM_SHOES_ID] == ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID)
            {
                AppearanceToMaskColors[AppearanceContentPack.Type.Shoes] = new List<Color>() { FashionSense.colorManager.GetColor(who, AppearanceModel.GetColorKey(AppearanceContentPack.Type.Shoes)) };
            }
        }
    }
}
