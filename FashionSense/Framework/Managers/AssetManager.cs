using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace FashionSense.Framework.Managers
{
    internal class AssetManager
    {
        internal string assetFolderPath;
        internal Dictionary<string, Texture2D> toolNames = new Dictionary<string, Texture2D>();

        // Tool textures
        private Texture2D _handMirrorTexture;

        // UI textures
        internal readonly Texture2D scissorsButtonTexture;
        internal readonly Texture2D accessoryButtonTexture;
        internal readonly Texture2D hatButtonTexture;
        internal readonly Texture2D shirtButtonTexture;
        internal readonly Texture2D pantsButtonTexture;
        internal readonly Texture2D sleevesAndShoesButtonTexture;
        internal readonly Texture2D sleevesButtonTexture;
        internal readonly Texture2D shoesButtonTexture;
        internal readonly Texture2D exportButton;

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the assets
            _handMirrorTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "HandMirror.png"));
            scissorsButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "HairButton.png"));
            accessoryButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "AccessoryButton.png"));
            hatButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "HatButton.png"));
            shirtButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "ShirtButton.png"));
            pantsButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "PantsButton.png"));
            sleevesButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "SleevesButton.png"));
            sleevesAndShoesButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "SleevesShoesButton.png"));
            shoesButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "ShoesButton.png"));
            exportButton = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "ExportButton.png"));

            // Setup toolNames
            toolNames.Add("HandMirror", _handMirrorTexture);
        }

        internal Texture2D GetHandMirrorTexture()
        {
            return _handMirrorTexture;
        }
    }
}
