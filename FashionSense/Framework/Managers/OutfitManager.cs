using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FashionSense.Framework.Managers
{
    class OutfitManager
    {
        private IMonitor _monitor;

        public OutfitManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
        }

        public Outfit CreateOutfit(Farmer who, string name)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            // Create the outfit
            var outfit = new Outfit(who, name);

            // Add it to the current listing
            outfits.Add(outfit);

            // Serialize the changes
            who.modData[ModDataKeys.OUTFITS] = JsonSerializer.Serialize(outfits);

            return outfit;
        }

        public void DeleteOutfit(Farmer who, string name)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            if (!outfits.Any(o => o.Name.Equals(name, StringComparison.Ordinal)))
            {
                return;
            }
            outfits.RemoveAt(outfits.FindIndex(o => o.Name.Equals(name, StringComparison.Ordinal)));

            // Serialize the changes
            who.modData[ModDataKeys.OUTFITS] = JsonSerializer.Serialize(outfits);
        }

        public bool DoesOutfitExist(Farmer who, string name)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            return outfits.Any(o => o.Name.Equals(name, StringComparison.Ordinal));
        }

        public List<Outfit> GetOutfits(Farmer who)
        {
            if (!who.modData.ContainsKey(ModDataKeys.OUTFITS))
            {
                return new List<Outfit>();
            }

            return JsonSerializer.Deserialize<List<Outfit>>(who.modData[ModDataKeys.OUTFITS]);
        }

        public void RenameOutfit(Farmer who, string originalName, string currentName)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            if (!outfits.Any(o => o.Name.Equals(originalName, StringComparison.Ordinal)))
            {
                return;
            }
            outfits.First(o => o.Name.Equals(originalName, StringComparison.Ordinal)).Name = currentName;

            // Serialize the changes
            who.modData[ModDataKeys.OUTFITS] = JsonSerializer.Serialize(outfits);
        }

        public void SetOutfit(Farmer who, Outfit outfit)
        {
            who.modData[ModDataKeys.CUSTOM_HAIR_ID] = String.IsNullOrEmpty(outfit.HairId) ? "None" : outfit.HairId;
            who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID] = String.IsNullOrEmpty(outfit.AccessoryOneId) ? "None" : outfit.AccessoryOneId;
            who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID] = String.IsNullOrEmpty(outfit.AccessoryTwoId) ? "None" : outfit.AccessoryTwoId;
            who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID] = String.IsNullOrEmpty(outfit.AccessoryThreeId) ? "None" : outfit.AccessoryThreeId;
            who.modData[ModDataKeys.CUSTOM_HAT_ID] = String.IsNullOrEmpty(outfit.HatId) ? "None" : outfit.HatId;
            who.modData[ModDataKeys.CUSTOM_SHIRT_ID] = String.IsNullOrEmpty(outfit.ShirtId) ? "None" : outfit.ShirtId;
            who.modData[ModDataKeys.CUSTOM_SLEEVES_ID] = String.IsNullOrEmpty(outfit.SleevesId) ? "None" : outfit.SleevesId;
            who.modData[ModDataKeys.CUSTOM_PANTS_ID] = String.IsNullOrEmpty(outfit.PantsId) ? "None" : outfit.PantsId;
            who.modData[ModDataKeys.CUSTOM_SHOES_ID] = String.IsNullOrEmpty(outfit.ShoesId) ? "None" : outfit.ShoesId;

            who.changeHairColor(new Color() { PackedValue = uint.Parse(outfit.HairColor) });
            who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR] = outfit.AccessoryOneColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR] = outfit.AccessoryTwoColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR] = outfit.AccessoryThreeColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR] = outfit.HatColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR] = outfit.ShirtColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR] = outfit.SleevesColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR] = outfit.PantsColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR] = outfit.ShoesColor;

            FashionSense.SetSpriteDirty();

            // Attempt to reset any overridden textures
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_HAIR_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_HAT_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_PANTS_ID]);
            FashionSense.ResetTextureIfNecessary(who.modData[ModDataKeys.CUSTOM_SHOES_ID]);
        }

        public void OverrideOutfit(Farmer who, string name)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            if (!outfits.Any(o => o.Name.Equals(name, StringComparison.Ordinal)))
            {
                CreateOutfit(who, name);
                return;
            }
            outfits[outfits.FindIndex(o => o.Name.Equals(name, StringComparison.Ordinal))] = new Outfit(who, name);

            // Serialize the changes
            who.modData[ModDataKeys.OUTFITS] = JsonSerializer.Serialize(outfits);
        }
    }
}
