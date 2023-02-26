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
using FashionSense.Framework.Patches.Renderer;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class LayerManager
    {
        private IMonitor _monitor;
        private int _facingDirection;

        public LayerManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public List<LayerData> SortModelsForDrawing(Farmer who, int facingDirection, List<AppearanceModel> models)
        {
            // Set the required variables
            _facingDirection = facingDirection;

            // Establish the rawLayerData list
            List<LayerData> rawLayerData = new List<LayerData>();

            // Add in LayerData for vanilla appearances, if models doesn't contain them
            AddVanillaLayerData(models, ref rawLayerData);

            // Add in existing models, defaulting to vanilla if certain conditions are not met
            foreach (var model in models)
            {
                switch (model)
                {
                    case PantsModel pantsModel:
                        AddPants(who, pantsModel, ref rawLayerData);
                        break;
                    case ShoesModel shoesModel:
                        AddShoes(who, shoesModel, ref rawLayerData);
                        break;
                    case ShirtModel shirtModel:
                        AddShirt(who, shirtModel, ref rawLayerData);
                        break;
                    case AccessoryModel accessoryModel:
                        AddAccessory(who, accessoryModel, ref rawLayerData);
                        break;
                    case HairModel hairModel:
                        AddHair(who, hairModel, ref rawLayerData);
                        break;
                    case SleevesModel sleevesModel:
                        AddSleeves(who, sleevesModel, ref rawLayerData);
                        break;
                    case HatModel hatModel:
                        AddHat(who, hatModel, ref rawLayerData);
                        break;
                }
            }

            // Establish the initial sorted order, assuming no conditional changes are required
            List<LayerData> sortedLayerData = new List<LayerData>()
            {
                rawLayerData.First(d => d.AppearanceType is AppearanceContentPack.Type.Pants),
                rawLayerData.First(d => d.AppearanceType is AppearanceContentPack.Type.Shoes),
                rawLayerData.First(d => d.AppearanceType is AppearanceContentPack.Type.Shirt),
                rawLayerData.First(d => d.AppearanceType is AppearanceContentPack.Type.Hair),
                rawLayerData.First(d => d.AppearanceType is AppearanceContentPack.Type.Sleeves),
                rawLayerData.First(d => d.AppearanceType is AppearanceContentPack.Type.Hat),
            };
            sortedLayerData.InsertRange(sortedLayerData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Shirt), rawLayerData.Where(d => d.AppearanceType is AppearanceContentPack.Type.Accessory));

            // Sort the models in the actual correct order
            foreach (var LayerData in sortedLayerData.ToList())
            {
                // If the LayerData is using vanilla logic, skip any conditional checks
                if (LayerData.IsVanilla)
                {
                    continue;
                }

                switch (LayerData.AppearanceType)
                {
                    case AppearanceContentPack.Type.Pants:
                        SortPants(LayerData, ref sortedLayerData);
                        break;
                    case AppearanceContentPack.Type.Shoes:
                        SortShoes(LayerData, ref sortedLayerData);
                        break;
                    case AppearanceContentPack.Type.Shirt:
                        SortShirt(LayerData, ref sortedLayerData);
                        break;
                    case AppearanceContentPack.Type.Accessory:
                        SortAccessory(LayerData, ref sortedLayerData);
                        break;
                    case AppearanceContentPack.Type.Hair:
                        SortHair(LayerData, ref sortedLayerData);
                        break;
                    case AppearanceContentPack.Type.Sleeves:
                        SortSleeves(LayerData, ref sortedLayerData);
                        break;
                    case AppearanceContentPack.Type.Hat:
                        SortHat(LayerData, ref sortedLayerData);
                        break;
                }
            }

            return sortedLayerData;
        }

        #region Conditional check methods
        private bool ShouldHideWhileSwimmingOrWearingBathingSuit(Farmer who, AppearanceModel model)
        {
            return (model.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (model.HideWhileSwimming && who.swimming.Value);
        }

        private bool IsHatHidingHair(List<LayerData> rawLayerData)
        {
            return rawLayerData.Any(d => d.AppearanceModel is HatModel hatModel && hatModel.HideHair is true);
        }

        private bool AreSleevesForcedHidden(List<LayerData> rawLayerData)
        {
            return rawLayerData.Any(d => d.AppearanceModel is not null && d.AppearanceModel.HideSleeves is true);
        }
        #endregion

        private void AddVanillaLayerData(List<AppearanceModel> models, ref List<LayerData> rawLayerData)
        {
            if (models.Any(m => m is PantsModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Pants, null, isVanilla: true));
            }
            if (models.Any(m => m is ShoesModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Shoes, null, isVanilla: true));
            }
            if (models.Any(m => m is ShirtModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Shirt, null, isVanilla: true));
            }
            if (models.Any(m => m is AccessoryModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Accessory, null, isVanilla: true));
            }
            if (models.Any(m => m is HairModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Hair, null, isVanilla: true));
            }
            if (models.Any(m => m is SleevesModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Sleeves, null, isVanilla: true));
            }
            if (models.Any(m => m is HatModel) is false)
            {
                rawLayerData.Add(new LayerData(AppearanceContentPack.Type.Hat, null, isVanilla: true));
            }
        }
        private void MoveLayerDataItem(int index, LayerData LayerData, ref List<LayerData> sourceList)
        {
            sourceList.Remove(LayerData);
            sourceList.Insert(index, LayerData);
        }

        #region Add methods for rawLayerData
        private void AddPants(Farmer who, PantsModel pantsModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Pants, pantsModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, pantsModel))
            {
                LayerData.IsVanilla = true;
            }

            rawLayerData.Add(LayerData);
        }

        private void AddShoes(Farmer who, ShoesModel shoesModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Shoes, shoesModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, shoesModel) || DrawPatch.ShouldHideLegs(who, _facingDirection))
            {
                LayerData.IsVanilla = true;
            }

            rawLayerData.Add(LayerData);
        }

        private void AddShirt(Farmer who, ShirtModel shirtModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Shirt, shirtModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, shirtModel))
            {
                LayerData.IsVanilla = true;
            }

            rawLayerData.Add(LayerData);
        }

        private void AddAccessory(Farmer who, AccessoryModel accessoryModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Accessory, accessoryModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, accessoryModel))
            {
                LayerData.IsVanilla = true;
            }

            rawLayerData.Add(LayerData);
        }

        private void AddHair(Farmer who, HairModel hairModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Hair, hairModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, hairModel))
            {
                if (IsHatHidingHair(rawLayerData))
                {
                    LayerData.IsVanilla = true;
                }
            }

            rawLayerData.Add(LayerData);
        }

        private void AddSleeves(Farmer who, SleevesModel sleevesModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Sleeves, sleevesModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, sleevesModel) || AreSleevesForcedHidden(rawLayerData))
            {
                LayerData.IsVanilla = true;
            }

            rawLayerData.Add(LayerData);
        }

        private void AddHat(Farmer who, HatModel hatModel, ref List<LayerData> rawLayerData)
        {
            var LayerData = new LayerData(AppearanceContentPack.Type.Hat, hatModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, hatModel))
            {
                LayerData.IsVanilla = true;
            }

            rawLayerData.Add(LayerData);
        }
        #endregion

        #region Sort methods for sortedLayerData
        private void SortPants(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            // Pants have no conditional checks
        }

        private void SortShoes(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            var shoesModel = LayerData.AppearanceModel as ShoesModel;
            if (shoesModel.DrawBeforePants)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Pants), LayerData, ref sortedLayerData);
            }
        }

        private void SortShirt(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            // Shirts have no conditional checks
        }

        private void SortAccessory(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            var accessoryModel = LayerData.AppearanceModel as AccessoryModel;
            if (accessoryModel.DrawBeforeHair)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Hair), LayerData, ref sortedLayerData);
            }
            else if (accessoryModel.DrawAfterSleeves)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Sleeves), LayerData, ref sortedLayerData);
            }
            else if (accessoryModel.DrawAfterPlayer)
            {
                // TODO: Verify that DrawAfterPlayer is implemented correctly
                // Move to bottom of list
                sortedLayerData.Remove(LayerData);
                sortedLayerData.Add(LayerData);
            }
        }

        private void SortHair(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            // Hair has no conditional checks
        }

        private void SortSleeves(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            var sleevesModel = LayerData.AppearanceModel as SleevesModel;
            if (sleevesModel.DrawBeforeShirt)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Shirt), LayerData, ref sortedLayerData);
            }
            else if (sleevesModel.DrawBeforeHair)
            {
                MoveLayerDataItem(sortedLayerData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Hair), LayerData, ref sortedLayerData);
            }
        }

        private void SortHat(LayerData LayerData, ref List<LayerData> sortedLayerData)
        {
            // Hat has no conditional checks
        }
        #endregion
    }
}
