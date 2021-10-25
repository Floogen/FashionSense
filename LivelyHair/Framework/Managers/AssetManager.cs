using LivelyHair.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Managers
{
    internal class AssetManager : IAssetLoader
    {
        internal string assetFolderPath;
        internal Dictionary<string, Texture2D> toolNames = new Dictionary<string, Texture2D>();

        private Texture2D _handMirrorTexture;

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.Content.GetActualAssetKey(Path.Combine("Framework", "Assets"), ContentSource.ModFolder);

            // Load in the assets
            _handMirrorTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "HandMirror.png"));

            // Setup toolNames
            toolNames.Add("HandMirror", _handMirrorTexture);
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (toolNames.Any(n => asset.AssetNameEquals($"{ModDataKeys.TOOL_TOKEN_HEADER}{n.Key}")))
            {
                return true;
            }

            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (toolNames.Any(n => asset.AssetNameEquals($"{ModDataKeys.TOOL_TOKEN_HEADER}{n.Key}")))
            {
                return (T)(object)toolNames.First(n => asset.AssetNameEquals($"{ModDataKeys.TOOL_TOKEN_HEADER}{n.Key}")).Value;
            }

            return (T)(object)asset;
        }

        internal Texture2D GetHandMirrorTexture()
        {
            return _handMirrorTexture;
        }
    }
}
