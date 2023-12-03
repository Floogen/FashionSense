using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Patches.Renderer;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FashionSense.Framework.Models
{
    public class Outfit
    {
        public string Name { get; set; }
        public string Author { get; set; }
        internal string Source { get; set; }
        internal bool IsPreset { get; set; }
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
        public Dictionary<IApi.Type, List<Color>> AppearanceToMaskColors { get; set; }

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

            AppearanceToMaskColors = new Dictionary<IApi.Type, List<Color>>();
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
                AppearanceToMaskColors[IApi.Type.Shoes] = new List<Color>() { FashionSense.colorManager.GetColor(who, AppearanceModel.GetColorKey(IApi.Type.Shoes)) };
            }

            // Set the author's name
            Author = who.Name;
        }

        private class HidePropertiesForExportResolver : DefaultContractResolver
        {
            private List<string> _ignoredProperties = new List<string>()
            {
                "IsBeingShared",
                "IsGlobal"
            };

            private bool IsPropertyObsolete(Type type, string propertyName)
            {
                var property = type.GetProperty(propertyName);
                var attributes = (ObsoleteAttribute[])property.GetCustomAttributes(typeof(ObsoleteAttribute), false);

                return (attributes != null && attributes.Length > 0);
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
                foreach (var property in properties)
                {
                    if (IsPropertyObsolete(type, property.UnderlyingName) || _ignoredProperties.Contains(property.UnderlyingName))
                    {
                        property.Ignored = true;
                    }
                }
                return properties;
            }
        }

        private bool IsIdValid(string id)
        {
            return string.IsNullOrEmpty(id) is false && id != "None";
        }

        internal string Export()
        {
            // Convert any old accessory appearances into the current format
            if (AccessoryIds is null)
            {
                AccessoryIds = new List<string>();
                AccessoryColors = new List<string>();
            }

            if (IsIdValid(AccessoryOneId))
            {
                AccessoryIds.Add(AccessoryOneId);
                AccessoryColors.Add(AccessoryOneColor);

                AccessoryOneId = null;
                AccessoryOneColor = null;
            }
            if (IsIdValid(AccessoryTwoId))
            {
                AccessoryIds.Add(AccessoryTwoId);
                AccessoryColors.Add(AccessoryTwoColor);

                AccessoryTwoId = null;
                AccessoryTwoColor = null;
            }
            if (IsIdValid(AccessoryThreeId))
            {
                AccessoryIds.Add(AccessoryThreeId);
                AccessoryColors.Add(AccessoryThreeColor);

                AccessoryThreeId = null;
                AccessoryThreeColor = null;
            }

            return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { ContractResolver = new HidePropertiesForExportResolver(), Formatting = Formatting.Indented });
        }

        internal List<string> GetMissingAppearanceIds()
        {
            List<string> missingAppearanceIds = new List<string>();

            Dictionary<string, AppearanceContentPack> appearanceIds = FashionSense.textureManager.GetIdToAppearanceModels();
            if (IsIdValid(HairId) && appearanceIds.ContainsKey(HairId) is false)
            {
                missingAppearanceIds.Add(HairId);
            }

            foreach (var appearanceId in appearanceIds.Keys)
            {
                if (IsIdValid(appearanceId) && appearanceIds.ContainsKey(appearanceId) is false)
                {
                    missingAppearanceIds.Add(appearanceId);
                }
            }

            if (IsIdValid(HatId) && appearanceIds.ContainsKey(HatId) is false)
            {
                missingAppearanceIds.Add(HatId);
            }
            if (IsIdValid(ShirtId) && appearanceIds.ContainsKey(ShirtId) is false)
            {
                missingAppearanceIds.Add(ShirtId);
            }
            if (IsIdValid(SleevesId) && appearanceIds.ContainsKey(SleevesId) is false)
            {
                missingAppearanceIds.Add(SleevesId);
            }
            if (IsIdValid(PantsId) && appearanceIds.ContainsKey(PantsId) is false)
            {
                missingAppearanceIds.Add(PantsId);
            }
            if (IsIdValid(ShoesId) && appearanceIds.ContainsKey(ShoesId) is false)
            {
                missingAppearanceIds.Add(ShoesId);
            }

            return missingAppearanceIds;
        }

        internal bool HasAllRequiredAppearances()
        {
            return GetMissingAppearanceIds().Count == 0;
        }
    }
}
