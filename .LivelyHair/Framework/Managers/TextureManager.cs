using LivelyHair.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Managers
{
    class TextureManager
    {
        private IMonitor _monitor;
        internal Texture2D testTexture;
        private List<AppearanceModel> _appearanceTextures;

        public TextureManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _appearanceTextures = new List<AppearanceModel>();
        }

        public void AddAppearanceModel(AppearanceModel model)
        {
            if (_appearanceTextures.Any(t => t.Id == model.Id))
            {
                var replacementIndex = _appearanceTextures.IndexOf(_appearanceTextures.First(t => t.Id == model.Id));
                _appearanceTextures[replacementIndex] = model;
            }
            else
            {
                _appearanceTextures.Add(model);
            }
        }

        public AppearanceModel GetSpecificAppearanceModel(string appearanceId)
        {
            return _appearanceTextures.FirstOrDefault(t => String.Equals(t.Id, appearanceId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
