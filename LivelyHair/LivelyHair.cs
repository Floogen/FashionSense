using HarmonyLib;
using LivelyHair.Framework.Managers;
using LivelyHair.Framework.Models;
using LivelyHair.Framework.Patches.Renderer;
using LivelyHair.Framework.Patches.ShopLocations;
using LivelyHair.Framework.Patches.Tools;
using LivelyHair.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LivelyHair
{
    public class LivelyHair : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static AssetManager assetManager;
        internal static TextureManager textureManager;

        // Utilities
        internal static MovementData movementData;

        // Debugging flags
        private bool _displayMovementData = false;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            assetManager = new AssetManager(modHelper);
            textureManager = new TextureManager(monitor, modHelper);

            // Setup our utilities
            movementData = new MovementData();

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply hair related patches
                new FarmerRendererPatch(monitor, modHelper).Apply(harmony);
                new ToolPatch(monitor, modHelper).Apply(harmony);
                new SeedShopPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("lh_display_movement", "Displays debug info related to player movement. Use again to disable. \n\nUsage: lh_display_movement", delegate { _displayMovementData = !_displayMovementData; });
            helper.ConsoleCommands.Add("lh_reload", "Reloads all Lively Hair content packs.\n\nUsage: lh_reload", delegate { this.LoadContentPacks(); });

            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
            modHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            modHelper.Events.Display.Rendered += OnRendered;
        }

        private void OnRendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (_displayMovementData)
            {
                movementData.OnRendered(sender, e);
            }
        }

        private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            movementData.Update(Game1.player, Game1.currentGameTime);
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Load any owned content packs
            this.LoadContentPacks();
        }

        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Game1.player.modData["LivelyHair.CustomHair.Id"] = "ExampleAuthor.ExampleLivelyAppearancesPack/Long Hair";
        }

        private void LoadContentPacks()
        {
            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading textures from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Debug);

                try
                {
                    var hairFolders = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Hairs")).GetDirectories("*", SearchOption.AllDirectories);
                    if (hairFolders.Count() == 0)
                    {
                        Monitor.Log($"No sub-folders found under Textures for the content pack {contentPack.Manifest.Name}!", LogLevel.Warn);
                        continue;
                    }

                    // Load in the hairs
                    foreach (var textureFolder in hairFolders)
                    {
                        if (!File.Exists(Path.Combine(textureFolder.FullName, "hair.json")))
                        {
                            if (textureFolder.GetDirectories().Count() == 0)
                            {
                                Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a hair.json under {textureFolder.Name}!", LogLevel.Warn);
                            }

                            continue;
                        }

                        var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                        var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "hair.json");

                        // Parse the model and assign it the content pack's owner
                        AppearanceModel appearanceModel = contentPack.ReadJsonFile<AppearanceModel>(modelPath);
                        appearanceModel.Owner = contentPack.Manifest.UniqueID;

                        if (String.IsNullOrEmpty(appearanceModel.Name))
                        {
                            Monitor.Log($"Unable to add hair texture for {appearanceModel.Owner}: Missing the Name property!", LogLevel.Warn);
                            continue;
                        }
                        // Set the ModelName and TextureId
                        appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.Name);

                        // Verify we are given a texture and if so, track it
                        if (!File.Exists(Path.Combine(textureFolder.FullName, "hair.png")))
                        {
                            Monitor.Log($"Unable to add hair texture for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated hair.png given", LogLevel.Warn);
                            continue;
                        }

                        // Load in the texture
                        appearanceModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, "hair.png")));

                        // Track the model
                        textureManager.AddAppearanceModel(appearanceModel);

                        // Log it
                        Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error loading content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
                }
            }
        }
    }
}
