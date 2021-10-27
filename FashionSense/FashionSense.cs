using HarmonyLib;
using FashionSense.Framework.Managers;
using FashionSense.Framework.Models;
using FashionSense.Framework.Patches.Menus;
using FashionSense.Framework.Patches.Renderer;
using FashionSense.Framework.Patches.ShopLocations;
using FashionSense.Framework.Patches.Tools;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FashionSense
{
    public class FashionSense : Mod
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

                // Apply tool related patches
                new ToolPatch(monitor, modHelper).Apply(harmony);
                new SeedShopPatch(monitor, modHelper).Apply(harmony);

                // Apply UI related patches
                new CharacterCustomizationPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("fs_display_movement", "Displays debug info related to player movement. Use again to disable. \n\nUsage: lh_display_movement", delegate { _displayMovementData = !_displayMovementData; });
            helper.ConsoleCommands.Add("fs_reload", "Reloads all Fashion Sense content packs.\n\nUsage: lh_reload", delegate { this.LoadContentPacks(); });

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
            if (!Game1.player.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID))
            {
                Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID] = null;
            }
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
                        Monitor.Log($"No sub-folders found under Textures for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Load in the hairs
                    foreach (var textureFolder in hairFolders)
                    {
                        if (!File.Exists(Path.Combine(textureFolder.FullName, "hair.json")))
                        {
                            if (textureFolder.GetDirectories().Count() == 0)
                            {
                                Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a hair.json under {textureFolder.Name}", LogLevel.Warn);
                            }

                            continue;
                        }

                        var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                        var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "hair.json");

                        // Parse the model and assign it the content pack's owner
                        AppearanceModel appearanceModel = contentPack.ReadJsonFile<AppearanceModel>(modelPath);
                        appearanceModel.Author = contentPack.Manifest.Author;
                        appearanceModel.Owner = contentPack.Manifest.UniqueID;

                        // Verify the required Name property is set
                        if (String.IsNullOrEmpty(appearanceModel.Name))
                        {
                            Monitor.Log($"Unable to add hairstyle for {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                            continue;
                        }
                        // Set the ModelName and TextureId
                        appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.Name);

                        // Verify that at least one HairModel is given
                        if (appearanceModel.BackHair is null && appearanceModel.RightHair is null && appearanceModel.FrontHair is null && appearanceModel.LeftHair is null)
                        {
                            Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: No hair models given (FrontHair, BackHair, etc.)", LogLevel.Warn);
                            continue;
                        }

                        // Verify we are given a texture and if so, track it
                        if (!File.Exists(Path.Combine(textureFolder.FullName, "hair.png")))
                        {
                            Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated hair.png given", LogLevel.Warn);
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

        internal static void ResetAnimationModDataFields(Farmer who, int duration, AnimationModel.Type animationType, int facingDirection)
        {
            who.modData[ModDataKeys.ANIMATION_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_ELAPSED_DURATION] = "0";
            who.modData[ModDataKeys.ANIMATION_TYPE] = animationType.ToString();
            who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] = facingDirection.ToString();
        }
    }
}
