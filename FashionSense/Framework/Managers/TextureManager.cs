using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Shirt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Managers
{
    internal class TextureManager
    {
        private IMonitor _monitor;
        private List<AppearanceContentPack> _appearanceTextures;

        private Dictionary<string, AppearanceContentPack> _idToModels;

        public TextureManager(IMonitor monitor)
        {
            _monitor = monitor;
            _appearanceTextures = new List<AppearanceContentPack>();

            _idToModels = new Dictionary<string, AppearanceContentPack>();
        }

        public void Reset(string packId = null)
        {
            if (String.IsNullOrEmpty(packId) is true)
            {
                _appearanceTextures.Clear();
                _idToModels.Clear();
            }
            else
            {
                _appearanceTextures = _appearanceTextures.Where(a => a.Owner.Equals(packId, StringComparison.OrdinalIgnoreCase) is false).ToList();
            }
        }

        public void AddAppearanceModel(AppearanceContentPack model)
        {
            if (string.IsNullOrEmpty(model.Id) is false && _appearanceTextures.Any(t => t.Id == model.Id && t.PackType == model.PackType))
            {
                var replacementIndex = _appearanceTextures.IndexOf(_appearanceTextures.First(t => t.Id == model.Id && t.PackType == model.PackType));
                _appearanceTextures[replacementIndex] = model;
            }
            else
            {
                if (model.IsLocalPack is false)
                {
                    if (AttemptHandleExternalPack(model) is false)
                    {
                        return;
                    }
                }

                _appearanceTextures.Add(model);
            }

            // Ensure AppearanceModel.StartingPosition is set
            foreach (var appearanceModel in model.GetAppearanceModels())
            {
                if (appearanceModel is null)
                {
                    continue;
                }

                if (appearanceModel.StartingPosition is null)
                {
                    appearanceModel.StartingPosition = new Position() { X = 0, Y = 0 };
                }
            }

            _idToModels[model.Id] = model;
        }

        private void HandleVanillaHatDimensions(HatContentPack pack, HatModel model, Rectangle spriteDimensions)
        {
            if (model.HatSize is null)
            {
                model.HatSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width };
            }

            if (model.StartingPosition is null)
            {
                if (pack.FrontHat == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 0 };
                }
                else if (pack.RightHat == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 20 };
                }
                else if (pack.LeftHat == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 40 };
                }
                else if (pack.BackHat == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 60 };
                }
            }
        }

        private void HandleVanillaShirtDimensions(ShirtContentPack pack, ShirtModel model, Rectangle spriteDimensions)
        {
            if (model.ShirtSize is null)
            {
                model.ShirtSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width };
            }

            if (model.StartingPosition is null)
            {
                if (pack.FrontShirt == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 0 };
                }
                else if (pack.RightShirt == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 8 };
                }
                else if (pack.LeftShirt == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 16 };
                }
                else if (pack.BackShirt == model)
                {
                    model.StartingPosition = new Position() { X = 0, Y = 24 };
                }
            }
        }

        private bool AttemptHandleExternalPack(AppearanceContentPack appearanceContentPack)
        {
            // Handle any appearances that may have been added externally
            if (string.IsNullOrEmpty(appearanceContentPack.PackId))
            {
                _monitor.Log($"Unable to add appearance from {appearanceContentPack.PackName}: Missing required field PackId", LogLevel.Warn);
                return false;
            }
            else if (string.IsNullOrEmpty(appearanceContentPack.FromItemId) && string.IsNullOrEmpty(appearanceContentPack.Name))
            {
                _monitor.Log($"Unable to add appearance from {appearanceContentPack.PackName}: Must give FromItemId or Name", LogLevel.Warn);
                return false;
            }
            else if (string.IsNullOrEmpty(appearanceContentPack.Id))
            {
                _monitor.Log($"Unable to add appearance from {appearanceContentPack.PackName}: Missing Id property", LogLevel.Warn);
                return false;
            }

            if (string.IsNullOrEmpty(appearanceContentPack.FromItemId) is false)
            {
                var itemData = ItemRegistry.GetData(appearanceContentPack.FromItemId);
                appearanceContentPack.Name = itemData.DisplayName;
                appearanceContentPack.TexturePath = itemData.TextureName;

                var spriteDimensions = itemData.GetSourceRect();
                if (appearanceContentPack is HatContentPack hatContentPack)
                {
                    if (hatContentPack.FrontHat is null)
                    {
                        hatContentPack.FrontHat = new HatModel() { StartingPosition = new Position() { X = 0, Y = 0 }, HatSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaHatDimensions(hatContentPack, hatContentPack.FrontHat, spriteDimensions);
                    }

                    if (hatContentPack.RightHat is null)
                    {
                        hatContentPack.RightHat = new HatModel() { StartingPosition = new Position() { X = 0, Y = 20 }, HatSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaHatDimensions(hatContentPack, hatContentPack.RightHat, spriteDimensions);
                    }

                    if (hatContentPack.LeftHat is null)
                    {
                        hatContentPack.LeftHat = new HatModel() { StartingPosition = new Position() { X = 0, Y = 40 }, HatSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaHatDimensions(hatContentPack, hatContentPack.LeftHat, spriteDimensions);
                    }

                    if (hatContentPack.BackHat is null)
                    {
                        hatContentPack.BackHat = new HatModel() { StartingPosition = new Position() { X = 0, Y = 60 }, HatSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaHatDimensions(hatContentPack, hatContentPack.BackHat, spriteDimensions);
                    }
                }
                else if (appearanceContentPack is ShirtContentPack shirtContentPack)
                {
                    if (shirtContentPack.FrontShirt is null)
                    {
                        shirtContentPack.FrontShirt = new ShirtModel() { StartingPosition = new Position() { X = 0, Y = 0 }, ShirtSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaShirtDimensions(shirtContentPack, shirtContentPack.FrontShirt, spriteDimensions);
                    }

                    if (shirtContentPack.RightShirt is null)
                    {
                        shirtContentPack.RightShirt = new ShirtModel() { StartingPosition = new Position() { X = 0, Y = 8 }, ShirtSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaShirtDimensions(shirtContentPack, shirtContentPack.RightShirt, spriteDimensions);
                    }

                    if (shirtContentPack.LeftShirt is null)
                    {
                        shirtContentPack.LeftShirt = new ShirtModel() { StartingPosition = new Position() { X = 0, Y = 16 }, ShirtSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaShirtDimensions(shirtContentPack, shirtContentPack.LeftShirt, spriteDimensions);
                    }

                    if (shirtContentPack.BackShirt is null)
                    {
                        shirtContentPack.BackShirt = new ShirtModel() { StartingPosition = new Position() { X = 0, Y = 24 }, ShirtSize = new Size() { Length = spriteDimensions.Height, Width = spriteDimensions.Width } };
                    }
                    else
                    {
                        HandleVanillaShirtDimensions(shirtContentPack, shirtContentPack.BackShirt, spriteDimensions);
                    }
                }
            }

            // Load in any missing properties (FS appearances added via Content Patcher)
            if (string.IsNullOrEmpty(appearanceContentPack.Owner))
            {
                appearanceContentPack.Owner = appearanceContentPack.PackId;
            }
            if (appearanceContentPack.Texture is null && string.IsNullOrEmpty(appearanceContentPack.TexturePath) is false)
            {
                appearanceContentPack.Texture = FashionSense.modHelper.GameContent.Load<Texture2D>(appearanceContentPack.TexturePath);
            }
            appearanceContentPack.LinkId();

            return true;
        }

        public void Sync<T>(Dictionary<string, T> models, IApi.Type packType = IApi.Type.Unknown) where T : AppearanceContentPack
        {
            foreach (var idModelPair in models)
            {
                var id = idModelPair.Key;
                var model = idModelPair.Value;

                if (model.IsLocalPack is false && model.PackType is IApi.Type.Unknown)
                {
                    model.PackType = packType;
                }

                // Ensure ID is set (may be blank for external pack, as it isn't mandatory to set due to entry path being used instead)
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = id;
                }

                AddAppearanceModel(model);
            }
        }

        public Dictionary<string, AppearanceContentPack> GetIdToAppearanceModels()
        {
            return _idToModels;
        }

        public Dictionary<string, T> GetIdToAppearanceModels<T>() where T : AppearanceContentPack
        {
            return _idToModels.Where(i => i.Value is T).ToDictionary(i => i.Key, i => (T)i.Value);
        }

        public List<AppearanceContentPack> GetAllAppearanceModels()
        {
            return _appearanceTextures.Where(t => t.IsLocked is false).ToList();
        }

        public List<T> GetAllAppearanceModels<T>() where T : AppearanceContentPack
        {
            return _appearanceTextures.OfType<T>().ToList();
        }

        public T GetSpecificAppearanceModel<T>(string appearanceId) where T : AppearanceContentPack
        {
            return (T)_appearanceTextures.FirstOrDefault(t => String.Equals(t.Id, appearanceId, StringComparison.OrdinalIgnoreCase) && t is T);
        }

        public AppearanceContentPack GetAppearanceContentPackByItemId(string itemId)
        {
            return _appearanceTextures.FirstOrDefault(t => t.Item is not null && t.Item.Id == itemId);
        }

        public AppearanceContentPack GetRandomAppearanceModel<T>()
        {
            var typedAppearanceModels = GetAllAppearanceModels().Where(m => m is T).ToList();
            if (typedAppearanceModels.Count() == 0)
            {
                return null;
            }

            var randomModelIndex = Game1.random.Next(typedAppearanceModels.Count());
            return typedAppearanceModels[randomModelIndex];
        }
    }
}
