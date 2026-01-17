using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;
using StardewValley.Tools;
using System.Collections.Generic;
using System.IO;

namespace FashionSense.Framework.Managers
{
    internal class AssetManager
    {
        internal string assetFolderPath;
        internal const string HAND_MIRROR_ID = "PeacefulEnd.FashionSense_HandMirror";
        internal const string HAND_MIRROR_TOOL_ID = $"(T){HAND_MIRROR_ID}";
        internal const string HAND_MIRROR_TEXTURE_PATH = "FashionSense/Textures/HandMirror";

        // UI textures
        internal readonly Texture2D scissorsButtonTexture;
        internal readonly Texture2D accessoryButtonTexture;
        internal readonly Texture2D hatButtonTexture;
        internal readonly Texture2D shirtButtonTexture;
        internal readonly Texture2D pantsButtonTexture;
        internal readonly Texture2D sleevesAndShoesButtonTexture;
        internal readonly Texture2D sleevesButtonTexture;
        internal readonly Texture2D shoesButtonTexture;
        internal readonly Texture2D bodyButtonTexture;
        internal readonly Texture2D exportButton;

        // Appearances
        internal IContentPack localPack;

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the UI assets
            scissorsButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "HairButton.png"));
            accessoryButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "AccessoryButton.png"));
            hatButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "HatButton.png"));
            shirtButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "ShirtButton.png"));
            pantsButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "PantsButton.png"));
            sleevesButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "SleevesButton.png"));
            sleevesAndShoesButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "SleevesShoesButton.png"));
            shoesButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "ShoesButton.png"));
            bodyButtonTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "BodyButton.png"));
            exportButton = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "UI", "ExportButton.png"));
        }

        internal IContentPack GetLocalPack(bool update = false)
        {
            if (localPack is null || update is true)
            {
                localPack = FashionSense.modHelper.ContentPacks.CreateTemporary(Path.Combine(FashionSense.modHelper.DirectoryPath, "Framework", "Assets", "Local Pack"), "PeacefulEnd.FashionSense.LocalPack", "FS - Local Pack", "The local appearance pack for the Fashion Sense framework.", FashionSense.modManifest.Author, FashionSense.modManifest.Version);
            }
            return localPack;
        }

        internal string GetHandMirrorAssetPath()
        {
            return Path.Combine(assetFolderPath, "HandMirror.png");
        }

        internal void AddToolData(IAssetData gameToolData)
        {
            IDictionary<string, ToolData> toolData = gameToolData.AsDictionary<string, ToolData>().Data;

            string fullId = $"PeacefulEnd.FashionSense_HandMirror";
            ToolData newToolData = new()
            {
                ClassName = "GenericTool",
                Name = fullId,
                SalePrice = 750,
                DisplayName = FashionSense.modHelper.Translation.Get("tools.name.hand_mirror"),
                Description = FashionSense.modHelper.Translation.Get("tools.description.hand_mirror"),
                Texture = HAND_MIRROR_TEXTURE_PATH,
                ModData = new()
                {
                    [ModDataKeys.HAND_MIRROR_FLAG] = true.ToString()
                }
            };

            toolData[fullId] = newToolData;
        }

        internal void EditShopData(IAssetData gameShopData)
        {
            IDictionary<string, ShopData> shopData = gameShopData.AsDictionary<string, ShopData>().Data;
            if (shopData.ContainsKey("SeedShop"))
            {
                shopData["SeedShop"].Items.Add(new ShopItemData() { Id = HAND_MIRROR_TOOL_ID, ItemId = HAND_MIRROR_TOOL_ID });
            }
        }

        internal static GenericTool GetHandMirrorTool()
        {
            return ItemRegistry.Create<GenericTool>(HAND_MIRROR_TOOL_ID);
        }
    }
}
