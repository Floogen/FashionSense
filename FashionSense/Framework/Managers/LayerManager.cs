using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class LayerManager
    {
        private IMonitor _monitor;
        private int _facingDirection;
        private List<AppearanceMetadata> _metadata;

        public LayerManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public List<LayerData> SortModelsForDrawing(Farmer who, int facingDirection, List<AppearanceMetadata> metadata)
        {
            // Set the required variables
            _facingDirection = facingDirection;
            _metadata = metadata;

            // Establish the models list
            List<AppearanceModel> models = metadata.Where(m => m.Model is not null).Select(m => m.Model).ToList();

            // Establish the rawLayerData list
            List<LayerData> rawLayerData = new List<LayerData>();

            // Add in LayerData for vanilla appearances, if models doesn't contain them
            AddVanillaLayerData(models, ref rawLayerData);

            // Add in existing models, defaulting to vanilla if certain conditions are not met
            foreach (var data in metadata)
            {
                switch (data.Model)
                {
                    case PantsModel pantsModel:
                        AddPants(who, pantsModel, data.Colors, ref rawLayerData);
                        break;
                    case ShoesModel shoesModel:
                        AddShoes(who, shoesModel, data.Colors, ref rawLayerData);
                        break;
                    case ShirtModel shirtModel:
                        AddShirt(who, shirtModel, data.Colors, ref rawLayerData);
                        break;
                    case SleevesModel sleevesModel:
                        AddSleeves(who, sleevesModel, data.Colors, ref rawLayerData);
                        break;
                    case AccessoryModel accessoryModel:
                        AddAccessory(who, accessoryModel, data.Colors, ref rawLayerData);
                        break;
                    case HairModel hairModel:
                        AddHair(who, hairModel, data.Colors, ref rawLayerData);
                        break;
                    case HatModel hatModel:
                        AddHat(who, hatModel, data.Colors, ref rawLayerData);
                        break;
                }
            }

            // Establish the initial sorted order, assuming no conditional changes are required
            List<LayerData> sortedLayerData = new List<LayerData>()
            {
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Player),
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Shoes),
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Pants),
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Shirt),
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Hair),
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Sleeves),
                rawLayerData.First(d => d.AppearanceType is IApi.Type.Hat),
            };
            sortedLayerData.InsertRange(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Hair) + 1, rawLayerData.Where(d => d.AppearanceType is IApi.Type.Accessory));

            // If facing backwards, move the sleeves to before the hair
            if (_facingDirection == 0)
            {
                var sleevesLayerData = sortedLayerData.Find(d => d.AppearanceType is IApi.Type.Sleeves);
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Hair) - 1, sleevesLayerData, ref sortedLayerData);
            }

            // Sort the models in the actual correct order
            foreach (var layerData in sortedLayerData.ToList())
            {
                // If the LayerData is using vanilla logic, skip any conditional checks
                if (layerData.IsVanilla)
                {
                    continue;
                }

                // Perform conditional sorting
                switch (layerData.AppearanceType)
                {
                    case IApi.Type.Player:
                        SortPlayer(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Pants:
                        SortPants(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Shoes:
                        SortShoes(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Shirt:
                        SortShirt(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Accessory:
                        SortAccessory(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Hair:
                        SortHair(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Sleeves:
                        SortSleeves(layerData, ref sortedLayerData);
                        break;
                    case IApi.Type.Hat:
                        SortHat(layerData, ref sortedLayerData);
                        break;
                }

                // Perform sorting for DrawOrder property
                var drawOrderOverride = layerData.AppearanceModel.DrawOrderOverride;
                if (drawOrderOverride is not null && drawOrderOverride.IsValid())
                {
                    MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType == drawOrderOverride.AppearanceType) + (drawOrderOverride.Preposition is DrawOrder.Order.After ? 1 : 0), layerData, ref sortedLayerData);
                }
            }

            /* Debugging block
            int index = 0;
            foreach (var layerData in sortedLayerData)
            {
                _monitor.LogOnce($"[{DateTime.Now.ToString("T")}] [{index}] {layerData.AppearanceType} ({(layerData.AppearanceModel is null ? string.Empty : layerData.AppearanceModel.Pack.Id)})", LogLevel.Debug);
                index++;
            }
            */

            return sortedLayerData;
        }

        private void AddVanillaLayerData(List<AppearanceModel> models, ref List<LayerData> rawLayerData)
        {
            rawLayerData.Add(new LayerData(IApi.Type.Player, null, isVanilla: true));
            if (models.Any(m => m is PantsModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Pants, null, isVanilla: true));
            }
            if (models.Any(m => m is ShoesModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Shoes, null, isVanilla: true));
            }
            if (models.Any(m => m is ShirtModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Shirt, null, isVanilla: true));
            }
            if (models.Any(m => m is SleevesModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Sleeves, null, isVanilla: true));
            }
            if (models.Any(m => m is AccessoryModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Accessory, null, isVanilla: true));
            }
            if (models.Any(m => m is HairModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Hair, null, isVanilla: true) { IsHidden = AppearanceHelpers.IsHatHidingHair(_metadata) });
            }
            if (models.Any(m => m is HatModel) is false)
            {
                rawLayerData.Add(new LayerData(IApi.Type.Hat, null, isVanilla: true));
            }
        }
        private void MoveLayerDataItem(int index, LayerData layerData, ref List<LayerData> sourceList)
        {
            sourceList.Remove(layerData);
            sourceList.Insert(index, layerData);
        }

        #region Add methods for rawLayerData
        private void AddPants(Farmer who, PantsModel pantsModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Pants, pantsModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, pantsModel))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }

        private void AddShoes(Farmer who, ShoesModel shoesModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Shoes, shoesModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, shoesModel) || AppearanceHelpers.ShouldHideLegs(who, _facingDirection))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }

        private void AddShirt(Farmer who, ShirtModel shirtModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Shirt, shirtModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, shirtModel))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }

        private void AddAccessory(Farmer who, AccessoryModel accessoryModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Accessory, accessoryModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, accessoryModel))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }

        private void AddHair(Farmer who, HairModel hairModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Hair, hairModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, hairModel) || AppearanceHelpers.IsHatHidingHair(_metadata))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }

        private void AddSleeves(Farmer who, SleevesModel sleevesModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Sleeves, sleevesModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, sleevesModel) || AppearanceHelpers.AreSleevesForcedHidden(_metadata))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }

        private void AddHat(Farmer who, HatModel hatModel, List<Color> colors, ref List<LayerData> rawLayerData)
        {
            var layerData = new LayerData(IApi.Type.Hat, hatModel);
            if (AppearanceHelpers.ShouldHideWhileSwimmingOrWearingBathingSuit(who, hatModel))
            {
                layerData.IsHidden = true;
            }
            layerData.Colors = colors;

            rawLayerData.Add(layerData);
        }
        #endregion

        #region Sort methods for sortedLayerData
        private void SortPlayer(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            // Player layer has no conditional checks
        }

        private void SortPants(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            // Pants have no conditional checks
        }

        private void SortShoes(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            var shoesModel = layerData.AppearanceModel as ShoesModel;
            if (shoesModel.DrawBeforePants)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Pants), layerData, ref sortedLayerData);
            }
        }

        private void SortShirt(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            // Shirts have no conditional checks
        }

        private void SortAccessory(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            var accessoryModel = layerData.AppearanceModel as AccessoryModel;
            if (accessoryModel.DrawAfterPlayer)
            {
                if (_facingDirection == 0)
                {
                    MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Hat), layerData, ref sortedLayerData);
                }
                else
                {
                    MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Hair) + 1, layerData, ref sortedLayerData);
                }
            }
            else if (accessoryModel.DrawBehindHead)
            {
                // If the player is facing backwards, place the accessory after the hat
                // Need to do this for backwards compatibility reasons, as packs that use DrawBeforeHair (DrawBehindHead) rely on this unintended behavior
                if (_facingDirection == 0)
                {
                    MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Hat), layerData, ref sortedLayerData);
                }
                else
                {
                    MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Player), layerData, ref sortedLayerData);
                }
            }
            else if (accessoryModel.DrawAfterSleeves)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Sleeves) + 1, layerData, ref sortedLayerData);
            }
            else if (_facingDirection == 0)
            {
                // If the player is facing backwards, place the accessory before the hair
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Hair), layerData, ref sortedLayerData);
            }
        }

        private void SortHair(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            // Hair has no conditional checks
        }

        private void SortSleeves(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            var sleevesModel = layerData.AppearanceModel as SleevesModel;
            if (sleevesModel.DrawBeforeShirt)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Shirt), layerData, ref sortedLayerData);
            }
            else if (sleevesModel.DrawBehindHead)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is IApi.Type.Player), layerData, ref sortedLayerData);
            }
        }

        private void SortHat(LayerData layerData, ref List<LayerData> sortedLayerData)
        {
            // Hat has no conditional checks
        }
        #endregion
    }
}
