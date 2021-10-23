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

        public TextureManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            LoadTestTextures(helper);
        }

        private void LoadTestTextures(IModHelper helper)
        {
            testTexture = helper.Content.Load<Texture2D>("Framework/Assets/Tests/test_hair.png");
        }
    }
}
