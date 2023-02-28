using FashionSense.Framework.Models;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class OutfitManager
    {
        private IMonitor _monitor;
        private string _sharedOutfitDataPath = Path.Combine("Data", "Outfits.json");

        public OutfitManager(IMonitor monitor)
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
            SerializeOutfits(who, outfits);

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
            SerializeOutfits(who, outfits);
        }

        public bool DoesOutfitExist(Farmer who, string name)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            return outfits.Any(o => o.Name.Equals(name, StringComparison.Ordinal));
        }

        public List<Outfit> GetOutfits(Farmer who)
        {
            List<Outfit> outfits = new List<Outfit>();
            if (who.modData.ContainsKey(ModDataKeys.OUTFITS))
            {
                outfits = JsonConvert.DeserializeObject<List<Outfit>>(who.modData[ModDataKeys.OUTFITS]);
            }

            // Add in the shared outfits
            foreach (Outfit outfit in GetSharedOutfits())
            {
                if (outfits.Any(o => o.Name.Equals(outfit.Name, StringComparison.Ordinal)) is false)
                {
                    outfits.Add(outfit);
                }
            }

            return outfits;
        }

        public List<Outfit> GetSharedOutfits()
        {
            var sharedOutfits = FashionSense.modHelper.Data.ReadJsonFile<List<Outfit>>(_sharedOutfitDataPath) ?? new List<Outfit>();
            foreach (var outfit in sharedOutfits)
            {
                outfit.IsGlobal = true;
            }

            return sharedOutfits;
        }

        public Outfit GetOutfit(Farmer who, string name)
        {
            if (DoesOutfitExist(who, name) is false)
            {
                return null;
            }

            return GetOutfits(who).First(o => o.Name.Equals(name, StringComparison.Ordinal));
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
            SerializeOutfits(who, outfits);
        }

        public void SetOutfitShareState(Farmer who, string name, bool shouldBeShared)
        {
            if (DoesOutfitExist(who, name) is false)
            {
                return;
            }

            var outfits = GetOutfits(who);
            outfits.First(o => o.Name.Equals(name, StringComparison.Ordinal)).IsBeingShared = shouldBeShared;

            // Serialize the changes
            SerializeOutfits(who, outfits);
        }

        public void UpdateSharedOutfits(Farmer who)
        {
            FashionSense.modHelper.Data.WriteJsonFile(_sharedOutfitDataPath, GetOutfits(who).Where(o => o.IsBeingShared).ToList());
        }

        public void SerializeOutfits(Farmer who, List<Outfit> outfits)
        {
            who.modData[ModDataKeys.OUTFITS] = JsonConvert.SerializeObject(outfits);

            UpdateSharedOutfits(who);
        }

        public void SetOutfit(Farmer who, Outfit outfit)
        {
            who.modData[ModDataKeys.CUSTOM_HAIR_ID] = String.IsNullOrEmpty(outfit.HairId) ? "None" : outfit.HairId;
            who.modData[ModDataKeys.CUSTOM_HAT_ID] = String.IsNullOrEmpty(outfit.HatId) ? "None" : outfit.HatId;
            who.modData[ModDataKeys.CUSTOM_SHIRT_ID] = String.IsNullOrEmpty(outfit.ShirtId) ? "None" : outfit.ShirtId;
            who.modData[ModDataKeys.CUSTOM_SLEEVES_ID] = String.IsNullOrEmpty(outfit.SleevesId) ? "None" : outfit.SleevesId;
            who.modData[ModDataKeys.CUSTOM_PANTS_ID] = String.IsNullOrEmpty(outfit.PantsId) ? "None" : outfit.PantsId;
            who.modData[ModDataKeys.CUSTOM_SHOES_ID] = String.IsNullOrEmpty(outfit.ShoesId) ? "None" : outfit.ShoesId;

            who.changeHairColor(new Color() { PackedValue = uint.Parse(outfit.HairColor) });
            who.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR] = outfit.HatColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR] = outfit.ShirtColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR] = outfit.SleevesColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR] = outfit.PantsColor;
            who.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR] = outfit.ShoesColor;

            // Handle any old outfit versions
            if (outfit.Version == 1)
            {
                who.modData[ModDataKeys.CUSTOM_ACCESSORY_ID] = String.IsNullOrEmpty(outfit.AccessoryOneId) ? "None" : outfit.AccessoryOneId;
                who.modData[ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID] = String.IsNullOrEmpty(outfit.AccessoryTwoId) ? "None" : outfit.AccessoryTwoId;
                who.modData[ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID] = String.IsNullOrEmpty(outfit.AccessoryThreeId) ? "None" : outfit.AccessoryThreeId;

                who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR] = outfit.AccessoryOneColor;
                who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR] = outfit.AccessoryTwoColor;
                who.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR] = outfit.AccessoryThreeColor;

                FashionSense.accessoryManager.HandleOldAccessoryFormat(Game1.player);
            }
            else if (outfit.AccessoryIds.Count > 0)
            {
                FashionSense.accessoryManager.SetAccessories(who, outfit.AccessoryIds, outfit.AccessoryColors);
            }

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

            // Set the current outfit ID
            who.modData[ModDataKeys.CURRENT_OUTFIT_ID] = outfit.Name;
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
            SerializeOutfits(who, outfits);
        }
    }
}
