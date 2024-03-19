using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
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
        private List<Outfit> _presetOutfits;

        public OutfitManager(IMonitor monitor)
        {
            _monitor = monitor;
            _presetOutfits = new List<Outfit>();
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

        public void AddOutfit(Farmer who, Outfit outfit)
        {
            // Get the current outfits
            var outfits = GetOutfits(who);

            // Add it to the current listing
            outfit.Name = GetUniqueOutfitName(outfits, outfit.Name);
            outfits.Add(outfit);

            // Serialize the changes
            SerializeOutfits(who, outfits);
        }

        public void AddPresetOutfit(Outfit outfit)
        {
            outfit.Name = GetUniqueOutfitName(_presetOutfits, outfit.Name);
            _presetOutfits.Add(outfit);
        }

        public void ClearPresetOutfits()
        {
            _presetOutfits.Clear();
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

        public bool DoesOutfitExist(Farmer who, string name, bool usePresets = false)
        {
            // Get the current outfits
            var outfits = GetOutfits(who, usePresets);

            return outfits.Any(o => o.Name.Equals(name, StringComparison.Ordinal));
        }

        public List<Outfit> GetOutfits(Farmer who, bool usePresets = false)
        {
            if (usePresets)
            {
                return _presetOutfits;
            }

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

        public List<Outfit> GetPresetOutfits()
        {
            return _presetOutfits.OrderBy(o => o.Source).ThenBy(o => o.Name).ToList();
        }

        public Outfit GetOutfit(Farmer who, string name, bool usePresets = false)
        {
            if (DoesOutfitExist(who, name, usePresets) is false)
            {
                return null;
            }

            return usePresets ? GetPresetOutfits().First(o => o.Name.Equals(name, StringComparison.Ordinal)) : GetOutfits(who).First(o => o.Name.Equals(name, StringComparison.Ordinal));
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

        public void ClearOutfit(Farmer who)
        {
            who.modData[ModDataKeys.CUSTOM_HAIR_ID] = "None";
            who.modData[ModDataKeys.CUSTOM_HAT_ID] = "None";
            who.modData[ModDataKeys.CUSTOM_SHIRT_ID] = "None";
            who.modData[ModDataKeys.CUSTOM_SLEEVES_ID] = "None";
            who.modData[ModDataKeys.CUSTOM_PANTS_ID] = "None";
            who.modData[ModDataKeys.CUSTOM_SHOES_ID] = "None";

            FashionSense.accessoryManager.ClearAccessories(who);
        }

        public void SetOutfit(Farmer who, Outfit outfit)
        {
            who.modData[ModDataKeys.CUSTOM_HAIR_ID] = String.IsNullOrEmpty(outfit.HairId) ? "None" : outfit.HairId;
            who.modData[ModDataKeys.CUSTOM_HAT_ID] = String.IsNullOrEmpty(outfit.HatId) ? "None" : outfit.HatId;
            who.modData[ModDataKeys.CUSTOM_SHIRT_ID] = String.IsNullOrEmpty(outfit.ShirtId) ? "None" : outfit.ShirtId;
            who.modData[ModDataKeys.CUSTOM_SLEEVES_ID] = String.IsNullOrEmpty(outfit.SleevesId) ? "None" : outfit.SleevesId;
            who.modData[ModDataKeys.CUSTOM_PANTS_ID] = String.IsNullOrEmpty(outfit.PantsId) ? "None" : outfit.PantsId;
            who.modData[ModDataKeys.CUSTOM_SHOES_ID] = String.IsNullOrEmpty(outfit.ShoesId) ? "None" : outfit.ShoesId;

            // Handle old outfits without ColorMaskLayers
            who.changeHairColor(new Color() { PackedValue = uint.Parse(outfit.HairColor) });
            if (outfit.Version < 3)
            {
                who.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR] = outfit.HatColor;
                who.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR] = outfit.ShirtColor;
                who.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR] = outfit.SleevesColor;
                who.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR] = outfit.PantsColor;
                who.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR] = outfit.ShoesColor;

                FashionSense.SetCachedColor(ModDataKeys.UI_HAND_MIRROR_HAT_COLOR, IApi.Type.Hat, 0);
                FashionSense.SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR, IApi.Type.Shirt, 0);
                FashionSense.SetCachedColor(ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR, IApi.Type.Pants, 0);
                FashionSense.SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR, IApi.Type.Sleeves, 0);
                FashionSense.SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR, IApi.Type.Shoes, 0);

                // Cache hair color, as previous versions (5.4 and below) did not utilize a ModData key for it
                FashionSense.colorManager.SetColor(Game1.player, AppearanceModel.GetColorKey(IApi.Type.Hair, 0), outfit.HairColor);
            }
            else
            {
                foreach (var data in outfit.AppearanceToMaskColors.Where(d => d.Key is not IApi.Type.Accessory))
                {
                    for (int x = 0; x < data.Value.Count; x++)
                    {
                        FashionSense.colorManager.SetColor(who, AppearanceModel.GetColorKey(data.Key, maskLayerIndex: x), data.Value[x]);
                    }
                }
            }

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

                List<Color> accessoryColorMasks = new List<Color>();
                if (outfit.AppearanceToMaskColors.Any(d => d.Key is IApi.Type.Accessory))
                {
                    accessoryColorMasks = outfit.AppearanceToMaskColors.First(d => d.Key is IApi.Type.Accessory).Value;
                }

                int accessoryCountOffset = 0;
                foreach (int index in FashionSense.accessoryManager.GetActiveAccessoryIndices(who))
                {
                    var accessoryKey = FashionSense.accessoryManager.GetAccessoryIdByIndex(who, index);
                    if (FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(accessoryKey) is AccessoryContentPack aPack && aPack != null)
                    {
                        AccessoryModel accessoryModel = aPack.GetAccessoryFromFacingDirection(who.FacingDirection);
                        if (accessoryModel is null)
                        {
                            continue;
                        }

                        try
                        {
                            if (accessoryModel.ColorMaskLayers.Count > 0)
                            {
                                for (int x = 0; x < accessoryModel.ColorMaskLayers.Count; x++)
                                {
                                    FashionSense.colorManager.SetColor(who, AppearanceModel.GetColorKey(IApi.Type.Accessory, appearanceIndex: index, maskLayerIndex: x), accessoryColorMasks[accessoryCountOffset]);

                                    accessoryCountOffset += 1;
                                }
                            }
                            else
                            {
                                FashionSense.colorManager.SetColor(who, AppearanceModel.GetColorKey(IApi.Type.Accessory, appearanceIndex: index, maskLayerIndex: 0), accessoryColorMasks[accessoryCountOffset]);
                                accessoryCountOffset += 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log errors
                            continue;
                        }
                    }
                }
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

        private string GetUniqueOutfitName(List<Outfit> outfits, string outfitName)
        {
            if (outfits.Any(o => o.Name == outfitName))
            {
                return GetUniqueOutfitName(outfits, outfitName + " (Copy)");
            }

            return outfitName;
        }
    }
}
