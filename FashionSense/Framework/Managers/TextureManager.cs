using FashionSense.Framework.Models;
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

        public TextureManager(IMonitor monitor)
        {
            _monitor = monitor;
            _appearanceTextures = new List<AppearanceContentPack>();
        }

        public void Reset(string packId = null)
        {
            if (String.IsNullOrEmpty(packId) is true)
            {
                _appearanceTextures.Clear();
            }
            else
            {
                _appearanceTextures = _appearanceTextures.Where(a => a.Owner.Equals(packId, StringComparison.OrdinalIgnoreCase) is false).ToList();
            }
        }

        public void AddAppearanceModel(AppearanceContentPack model)
        {
            if (_appearanceTextures.Any(t => t.Id == model.Id && t.PackType == model.PackType))
            {
                var replacementIndex = _appearanceTextures.IndexOf(_appearanceTextures.First(t => t.Id == model.Id && t.PackType == model.PackType));
                _appearanceTextures[replacementIndex] = model;
            }
            else
            {
                _appearanceTextures.Add(model);
            }
        }

        public List<AppearanceContentPack> GetAllAppearanceModels()
        {
            return _appearanceTextures;
        }

        public List<T> GetAllAppearanceModels<T>() where T : AppearanceContentPack
        {
            return _appearanceTextures.Where(t => t is T) as List<T>;
        }

        public T GetSpecificAppearanceModel<T>(string appearanceId) where T : AppearanceContentPack
        {
            return (T)_appearanceTextures.FirstOrDefault(t => String.Equals(t.Id, appearanceId, StringComparison.OrdinalIgnoreCase) && t is T);
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
