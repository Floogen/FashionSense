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
    internal class DrawManager
    {
        private IMonitor _monitor;
        private int _facingDirection;

        public DrawManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public List<DrawData> SortModelsForDrawing(Farmer who, int facingDirection, List<AppearanceModel> models)
        {
            // Set the required variables
            _facingDirection = facingDirection;

            // Establish the rawDrawData list
            List<DrawData> rawDrawData = new List<DrawData>();

            // Add in DrawData for vanilla appearances, if models doesn't contain them
            AddVanillaDrawData(models, ref rawDrawData);

            // Add in existing models, defaulting to vanilla if certain conditions are not met
            foreach (var model in models)
            {
                switch (model)
                {
                    case PantsModel pantsModel:
                        AddPants(who, pantsModel, ref rawDrawData);
                        break;
                    case ShoesModel shoesModel:
                        AddShoes(who, shoesModel, ref rawDrawData);
                        break;
                    case ShirtModel shirtModel:
                        AddShirt(who, shirtModel, ref rawDrawData);
                        break;
                    case AccessoryModel accessoryModel:
                        AddAccessory(who, accessoryModel, ref rawDrawData);
                        break;
                    case HairModel hairModel:
                        AddHair(who, hairModel, ref rawDrawData);
                        break;
                    case SleevesModel sleevesModel:
                        AddSleeves(who, sleevesModel, ref rawDrawData);
                        break;
                    case HatModel hatModel:
                        AddHat(who, hatModel, ref rawDrawData);
                        break;
                }
            }

            // Establish the initial sorted order, assuming no conditional changes are required
            List<DrawData> sortedDrawData = new List<DrawData>()
            {
                rawDrawData.First(d => d.AppearanceType is AppearanceContentPack.Type.Pants),
                rawDrawData.First(d => d.AppearanceType is AppearanceContentPack.Type.Shoes),
                rawDrawData.First(d => d.AppearanceType is AppearanceContentPack.Type.Shirt),
                rawDrawData.First(d => d.AppearanceType is AppearanceContentPack.Type.Hair),
                rawDrawData.First(d => d.AppearanceType is AppearanceContentPack.Type.Sleeves),
                rawDrawData.First(d => d.AppearanceType is AppearanceContentPack.Type.Hat),
            };
            sortedDrawData.InsertRange(sortedDrawData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Shirt), rawDrawData.Where(d => d.AppearanceType is AppearanceContentPack.Type.Accessory));

            // Sort the models in the actual correct order
            foreach (var drawData in sortedDrawData.ToList())
            {
                // If the DrawData is using vanilla logic, skip any conditional checks
                if (drawData.IsVanilla)
                {
                    continue;
                }

                switch (drawData.AppearanceType)
                {
                    case AppearanceContentPack.Type.Pants:
                        SortPants(drawData, ref sortedDrawData);
                        break;
                    case AppearanceContentPack.Type.Shoes:
                        SortShoes(drawData, ref sortedDrawData);
                        break;
                    case AppearanceContentPack.Type.Shirt:
                        SortShirt(drawData, ref sortedDrawData);
                        break;
                    case AppearanceContentPack.Type.Accessory:
                        SortAccessory(drawData, ref sortedDrawData);
                        break;
                    case AppearanceContentPack.Type.Hair:
                        SortHair(drawData, ref sortedDrawData);
                        break;
                    case AppearanceContentPack.Type.Sleeves:
                        SortSleeves(drawData, ref sortedDrawData);
                        break;
                    case AppearanceContentPack.Type.Hat:
                        SortHat(drawData, ref sortedDrawData);
                        break;
                }
            }

            return sortedDrawData;
        }

        #region Conditional check methods
        private bool ShouldHideWhileSwimmingOrWearingBathingSuit(Farmer who, AppearanceModel model)
        {
            return (model.HideWhileWearingBathingSuit && who.bathingClothes.Value) || (model.HideWhileSwimming && who.swimming.Value);
        }

        private bool IsHatHidingHair(List<DrawData> rawDrawData)
        {
            return rawDrawData.Any(d => d.AppearanceModel is HatModel hatModel && hatModel.HideHair is true);
        }

        private bool AreSleevesForcedHidden(List<DrawData> rawDrawData)
        {
            return rawDrawData.Any(d => d.AppearanceModel is not null && d.AppearanceModel.HideSleeves is true);
        }
        #endregion

        private void AddVanillaDrawData(List<AppearanceModel> models, ref List<DrawData> rawDrawData)
        {
            if (models.Any(m => m is PantsModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Pants, null, isVanilla: true));
            }
            if (models.Any(m => m is ShoesModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Shoes, null, isVanilla: true));
            }
            if (models.Any(m => m is ShirtModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Shirt, null, isVanilla: true));
            }
            if (models.Any(m => m is AccessoryModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Accessory, null, isVanilla: true));
            }
            if (models.Any(m => m is HairModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Hair, null, isVanilla: true));
            }
            if (models.Any(m => m is SleevesModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Sleeves, null, isVanilla: true));
            }
            if (models.Any(m => m is HatModel) is false)
            {
                rawDrawData.Add(new DrawData(AppearanceContentPack.Type.Hat, null, isVanilla: true));
            }
        }
        private void MoveDrawDataItem(int index, DrawData drawData, ref List<DrawData> sourceList)
        {
            sourceList.Remove(drawData);
            sourceList.Insert(index, drawData);
        }

        #region Add methods for rawDrawData
        private void AddPants(Farmer who, PantsModel pantsModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Pants, pantsModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, pantsModel))
            {
                drawData.IsVanilla = true;
            }

            rawDrawData.Add(drawData);
        }

        private void AddShoes(Farmer who, ShoesModel shoesModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Shoes, shoesModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, shoesModel) || DrawPatch.ShouldHideLegs(who, _facingDirection))
            {
                drawData.IsVanilla = true;
            }

            rawDrawData.Add(drawData);
        }

        private void AddShirt(Farmer who, ShirtModel shirtModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Shirt, shirtModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, shirtModel))
            {
                drawData.IsVanilla = true;
            }

            rawDrawData.Add(drawData);
        }

        private void AddAccessory(Farmer who, AccessoryModel accessoryModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Accessory, accessoryModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, accessoryModel))
            {
                drawData.IsVanilla = true;
            }

            rawDrawData.Add(drawData);
        }

        private void AddHair(Farmer who, HairModel hairModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Hair, hairModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, hairModel))
            {
                if (IsHatHidingHair(rawDrawData))
                {
                    drawData.IsVanilla = true;
                }
            }

            rawDrawData.Add(drawData);
        }

        private void AddSleeves(Farmer who, SleevesModel sleevesModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Sleeves, sleevesModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, sleevesModel) || AreSleevesForcedHidden(rawDrawData))
            {
                drawData.IsVanilla = true;
            }

            rawDrawData.Add(drawData);
        }

        private void AddHat(Farmer who, HatModel hatModel, ref List<DrawData> rawDrawData)
        {
            var drawData = new DrawData(AppearanceContentPack.Type.Hat, hatModel);
            if (ShouldHideWhileSwimmingOrWearingBathingSuit(who, hatModel))
            {
                drawData.IsVanilla = true;
            }

            rawDrawData.Add(drawData);
        }
        #endregion

        #region Sort methods for sortedDrawData
        private void SortPants(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            // Pants have no conditional checks
        }

        private void SortShoes(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            var shoesModel = drawData.AppearanceModel as ShoesModel;
            if (shoesModel.DrawBeforePants)
            {
                MoveDrawDataItem(sortedDrawData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Pants), drawData, ref sortedDrawData);
            }
        }

        private void SortShirt(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            // Shirts have no conditional checks
        }

        private void SortAccessory(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            var accessoryModel = drawData.AppearanceModel as AccessoryModel;
            if (accessoryModel.DrawBeforeHair)
            {
                MoveDrawDataItem(sortedDrawData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Hair), drawData, ref sortedDrawData);
            }
            else if (accessoryModel.DrawAfterSleeves)
            {
                MoveDrawDataItem(sortedDrawData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Sleeves), drawData, ref sortedDrawData);
            }
            else if (accessoryModel.DrawAfterPlayer)
            {
                // TODO: Verify that DrawAfterPlayer is implemented correctly
                // Move to bottom of list
                sortedDrawData.Remove(drawData);
                sortedDrawData.Add(drawData);
            }
        }

        private void SortHair(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            // Hair has no conditional checks
        }

        private void SortSleeves(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            var sleevesModel = drawData.AppearanceModel as SleevesModel;
            if (sleevesModel.DrawBeforeShirt)
            {
                MoveDrawDataItem(sortedDrawData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Shirt), drawData, ref sortedDrawData);
            }
            else if (sleevesModel.DrawBeforeHair)
            {
                MoveDrawDataItem(sortedDrawData.FindIndex(d => d.AppearanceType is AppearanceContentPack.Type.Hair), drawData, ref sortedDrawData);
            }
        }

        private void SortHat(DrawData drawData, ref List<DrawData> sortedDrawData)
        {
            // Hat has no conditional checks
        }
        #endregion
    }
}
