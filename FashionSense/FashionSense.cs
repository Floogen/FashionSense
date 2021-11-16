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
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.External.ContentPatcher;
using FashionSense.Framework.Models.Hat;
using FashionSense.Framework.Models.Shirt;

namespace FashionSense
{
    public class FashionSense : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;
        internal static TextureManager textureManager;

        // Utilities
        internal static ConditionData conditionData;

        // Constants
        internal const int MAX_TRACKED_MILLISECONDS = 3600000;

        // Debugging flags
        private bool _displayMovementData = false;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(modHelper);
            textureManager = new TextureManager(monitor, modHelper);

            // Setup our utilities
            conditionData = new ConditionData();

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply appearance related patches
                new FarmerRendererPatch(monitor, modHelper).Apply(harmony);
                new DrawPatch(monitor, modHelper).Apply(harmony);

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
            helper.ConsoleCommands.Add("fs_display_movement", "Displays debug info related to player movement. Use again to disable. \n\nUsage: fs_display_movement", delegate { _displayMovementData = !_displayMovementData; });
            helper.ConsoleCommands.Add("fs_reload", "Reloads all Fashion Sense content packs.\n\nUsage: fs_reload", delegate { this.LoadContentPacks(); });

            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
            modHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            modHelper.Events.Display.Rendered += OnRendered;
        }

        private void OnRendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (_displayMovementData)
            {
                conditionData.OnRendered(sender, e);
            }
        }

        private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady)
            {
                // Update movement trackers
                conditionData.Update(Game1.player, Game1.currentGameTime);
            }

            // Update elapsed durations
            UpdateElapsedDuration(ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION);
            UpdateElapsedDuration(ModDataKeys.ANIMATION_ACCESSORY_ELAPSED_DURATION);
            UpdateElapsedDuration(ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ELAPSED_DURATION);
            UpdateElapsedDuration(ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ELAPSED_DURATION);
            UpdateElapsedDuration(ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION);
            UpdateElapsedDuration(ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION);
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && apiManager.HookIntoContentPatcher(Helper))
            {
                apiManager.GetContentPatcherApi().RegisterToken(ModManifest, "Appearance", new AppearanceToken());
            }

            // Load any owned content packs
            this.LoadContentPacks();
        }

        private void OnSaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            // Reset Hand Mirror UI
            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = String.Empty;

            // Set the cached colors, if needed
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_HAT_COLOR);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR);
        }

        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            EnsureKeyExists(ModDataKeys.CUSTOM_HAIR_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_HAT_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_SHIRT_ID);

            // Set sprite to dirty in order to refresh sleeves and other tied-in appearances
            SetSpriteDirty();
        }

        private void UpdateElapsedDuration(string durationKey)
        {
            if (Game1.player.modData.ContainsKey(durationKey))
            {
                var elapsedDuration = Int32.Parse(Game1.player.modData[durationKey]);
                if (elapsedDuration < MAX_TRACKED_MILLISECONDS)
                {
                    Game1.player.modData[durationKey] = (elapsedDuration + Game1.currentGameTime.ElapsedGameTime.Milliseconds).ToString();
                }
            }
        }

        private void SetCachedColor(string colorKey)
        {
            if (!Game1.player.modData.ContainsKey(colorKey))
            {
                Game1.player.modData[colorKey] = Game1.player.hairstyleColor.Value.PackedValue.ToString();
            }
        }

        private void EnsureKeyExists(string key)
        {
            if (!Game1.player.modData.ContainsKey(key))
            {
                Game1.player.modData[key] = null;
            }
        }

        private void LoadContentPacks()
        {
            // Clear the existing cache of AppearanceModels
            textureManager.Reset();

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Debug);

                // Load Hairs
                Monitor.Log($"Loading hairstyles from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddHairContentPacks(contentPack);

                // Load Accessories
                Monitor.Log($"Loading accessories from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddAccessoriesContentPacks(contentPack);

                // Load Hats
                Monitor.Log($"Loading hats from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddHatsContentPacks(contentPack);

                // Load Shirts
                Monitor.Log($"Loading shirts from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddShirtsContentPacks(contentPack);
            }

            if (Context.IsWorldReady)
            {
                SetSpriteDirty();
            }
        }

        private void AddHairContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Hairs"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Hairs folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var hairFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (hairFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Hairs for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
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
                    HairContentPack appearanceModel = contentPack.ReadJsonFile<HairContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add hairstyle from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Hair;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a hairstyle with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<HairContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add hairstyle from {contentPack.Manifest.Name}: This pack already contains a hairstyle with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

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
                Monitor.Log($"Error loading hairstyles from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        private void AddAccessoriesContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Accessories"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Accessories folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var accessoryFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (accessoryFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Accessories for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the accessories
                foreach (var textureFolder in accessoryFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "accessory.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a accessory.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "accessory.json");

                    // Parse the model and assign it the content pack's owner
                    AccessoryContentPack appearanceModel = contentPack.ReadJsonFile<AccessoryContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add accessories from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Accessory;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a accessory with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add accessory from {contentPack.Manifest.Name}: This pack already contains a accessory with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one AccessoryModel is given
                    if (appearanceModel.BackAccessory is null && appearanceModel.RightAccessory is null && appearanceModel.FrontAccessory is null && appearanceModel.LeftAccessory is null)
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: No accessory models given (FrontAccessory, BackAccessory, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "accessory.png")))
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated accessory.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, "accessory.png")));

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading accessories from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        private void AddHatsContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Hats"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Hats folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var hatFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (hatFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Hats for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the accessories
                foreach (var textureFolder in hatFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "hat.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a hat.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "hat.json");

                    // Parse the model and assign it the content pack's owner
                    HatContentPack appearanceModel = contentPack.ReadJsonFile<HatContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add hats from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Hat;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a hat with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<HatContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add hat from {contentPack.Manifest.Name}: This pack already contains a hat with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one HatModel is given
                    if (appearanceModel.BackHat is null && appearanceModel.RightHat is null && appearanceModel.FrontHat is null && appearanceModel.LeftHat is null)
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: No hat models given (FrontHat, BackHat, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "hat.png")))
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated hat.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, "hat.png")));

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading hats from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        private void AddShirtsContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Shirts"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Shirts folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var shirtFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (shirtFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Shirts for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the accessories
                foreach (var textureFolder in shirtFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "shirt.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a shirt.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "shirt.json");

                    // Parse the model and assign it the content pack's owner
                    ShirtContentPack appearanceModel = contentPack.ReadJsonFile<ShirtContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add shirts from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Shirt;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a shirt with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<ShirtContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add shirt from {contentPack.Manifest.Name}: This pack already contains a shirt with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one ShirtModel is given
                    if (appearanceModel.BackShirt is null && appearanceModel.RightShirt is null && appearanceModel.FrontShirt is null && appearanceModel.LeftShirt is null)
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: No shirt models given (FrontShirt, BackShirt, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "shirt.png")))
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated shirt.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, "shirt.png")));

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading shirts from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }
        internal static void SetSpriteDirty()
        {
            var spriteDirty = modHelper.Reflection.GetField<bool>(Game1.player.FarmerRenderer, "_spriteDirty");
            spriteDirty.SetValue(true);
            var shirtDirty = modHelper.Reflection.GetField<bool>(Game1.player.FarmerRenderer, "_shirtDirty");
            shirtDirty.SetValue(true);
        }

        internal static void ResetAnimationModDataFields(Farmer who, int duration, AnimationModel.Type animationType, int facingDirection, bool ignoreAnimationType = false)
        {
            who.modData[ModDataKeys.ANIMATION_HAIR_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_HAIR_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_HAIR_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_HAIR_ELAPSED_DURATION] = "0";

            who.modData[ModDataKeys.ANIMATION_ACCESSORY_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_ELAPSED_DURATION] = "0";

            who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_ELAPSED_DURATION] = "0";

            who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_ELAPSED_DURATION] = "0";

            who.modData[ModDataKeys.ANIMATION_HAT_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_HAT_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_HAT_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_HAT_ELAPSED_DURATION] = "0";

            who.modData[ModDataKeys.ANIMATION_SHIRT_ITERATOR] = "0";
            who.modData[ModDataKeys.ANIMATION_SHIRT_STARTING_INDEX] = "0";
            who.modData[ModDataKeys.ANIMATION_SHIRT_FRAME_DURATION] = duration.ToString();
            who.modData[ModDataKeys.ANIMATION_SHIRT_ELAPSED_DURATION] = "0";

            who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] = facingDirection.ToString();

            if (!ignoreAnimationType)
            {
                who.modData[ModDataKeys.ANIMATION_HAIR_TYPE] = animationType.ToString();
                who.modData[ModDataKeys.ANIMATION_ACCESSORY_TYPE] = animationType.ToString();
                who.modData[ModDataKeys.ANIMATION_ACCESSORY_SECONDARY_TYPE] = animationType.ToString();
                who.modData[ModDataKeys.ANIMATION_ACCESSORY_TERTIARY_TYPE] = animationType.ToString();
                who.modData[ModDataKeys.ANIMATION_HAT_TYPE] = animationType.ToString();
                who.modData[ModDataKeys.ANIMATION_SHIRT_TYPE] = animationType.ToString();
            }
        }
    }
}
