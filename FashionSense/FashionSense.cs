using FashionSense.Framework.External.ContentPatcher;
using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Managers;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Patches.Core;
using FashionSense.Framework.Patches.Entities;
using FashionSense.Framework.Patches.GameLocations;
using FashionSense.Framework.Patches.Menus;
using FashionSense.Framework.Patches.Objects;
using FashionSense.Framework.Patches.Renderer;
using FashionSense.Framework.Patches.ShopLocations;
using FashionSense.Framework.Patches.Tools;
using FashionSense.Framework.UI;
using FashionSense.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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
        internal static AccessoryManager accessoryManager;
        internal static AnimationManager animationManager;
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;
        internal static ColorManager colorManager;
        internal static LayerManager layerManager;
        internal static OutfitManager outfitManager;
        internal static TextureManager textureManager;

        // Utilities
        internal static ConditionData conditionData;
        internal static Dictionary<string, ConditionGroup> conditionGroups;

        // Constants
        internal const int MAX_TRACKED_MILLISECONDS = 3600000;

        // Debugging flags
        private bool _displayMovementData = false;
        private bool _continuousReloading = false;
        private Vector2? _cachedPlayerPosition;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            accessoryManager = new AccessoryManager(monitor);
            animationManager = new AnimationManager(monitor);
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(modHelper);
            colorManager = new ColorManager(monitor);
            layerManager = new LayerManager(monitor);
            outfitManager = new OutfitManager(monitor);
            textureManager = new TextureManager(monitor);

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
                new GameLocationPatch(monitor, modHelper).Apply(harmony);

                // Apply UI related patches
                new CharacterCustomizationPatch(monitor, modHelper).Apply(harmony);
                new LetterViewerMenuPatch(monitor, modHelper).Apply(harmony);
                new SaveFileSlotPatch(monitor, modHelper).Apply(harmony);

                // Apply entity related patches
                new CharacterPatch(monitor, modHelper).Apply(harmony);

                // Apply object related patches
                new ObjectPatch(monitor, modHelper).Apply(harmony);

                // Apply core related patches
                new GamePatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("fs_display_movement", "Displays debug info related to player movement. Use again to disable. \n\nUsage: fs_display_movement", delegate { _displayMovementData = !_displayMovementData; });
            helper.ConsoleCommands.Add("fs_reload", "Reloads all Fashion Sense content packs. Can specify a manifest unique ID to only reload that pack.\n\nUsage: fs_reload [manifest_unique_id]", ReloadFashionSense);
            helper.ConsoleCommands.Add("fs_reload_continuous", "Debug usage only: reloads all Fashion Sense content packs every 2 seconds. Use the command again to stop the continuous reloading.\n\nUsage: fs_reload_continuous", delegate { _continuousReloading = !_continuousReloading; });
            helper.ConsoleCommands.Add("fs_add_mirror", "Gives you a Hand Mirror tool.\n\nUsage: fs_add_mirror", delegate { Game1.player.addItemToInventory(SeedShopPatch.GetHandMirrorTool()); });
            helper.ConsoleCommands.Add("fs_freeze_self", "Locks yourself in place, which is useful for showcasing custom appearances. Use the command again to unfreeze yourself.\n\nUsage: fs_freeze_self", delegate { _ = _cachedPlayerPosition is null ? _cachedPlayerPosition = Game1.player.Position : _cachedPlayerPosition = null; });

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.Rendered += OnRendered;
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
                if (_continuousReloading && e.IsMultipleOf(120))
                {
                    this.LoadContentPacks(true);
                }

                // Update elapsed durations for the player
                foreach (var farmer in Game1.getAllFarmers())
                {
                    UpdateElapsedDuration(farmer);

                    // Update movement trackers
                    conditionData.Update(farmer, Game1.currentGameTime);
                }
            }

            // Update elapsed durations when the player is using the SearchMenu
            if (Game1.activeClickableMenu is SearchMenu searchMenu && searchMenu is not null)
            {
                foreach (var fakeFarmer in searchMenu.fakeFarmers)
                {
                    UpdateElapsedDuration(fakeFarmer);
                }
            }

            // Check if fs_freeze_self is active
            if (_cachedPlayerPosition is not null)
            {
                Game1.MasterPlayer.Position = _cachedPlayerPosition.Value;
            }
        }

        private void OnWarped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            // Remove old lights
            foreach (var animationData in animationManager.GetAllAnimationData(e.Player).Where(a => a.LightId is not null))
            {
                e.OldLocation.sharedLights.Remove(animationData.LightId.Value);
            }
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

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data[ModDataKeys.LETTER_HAND_MIRROR] = modHelper.Translation.Get("letters.hand_mirror");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/PeacefulEnd/FashionSense/AppearanceData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels(), AssetLoadPriority.High);
            }
        }

        private void OnAssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            var asset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/AppearanceData"));
            if (asset is null)
            {
                return;
            }

            // Force load the changes
            _ = Helper.GameContent.Load<Dictionary<string, AppearanceContentPack>>(asset);
        }

        private void OnSaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            // Reset Hand Mirror UI
            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = String.Empty;

            // Set the cached colors, if needed
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR, AppearanceContentPack.Type.Accessory, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR, AppearanceContentPack.Type.Accessory, 1);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR, AppearanceContentPack.Type.Accessory, 2);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_HAT_COLOR, AppearanceContentPack.Type.Hat, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR, AppearanceContentPack.Type.Shirt, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR, AppearanceContentPack.Type.Pants, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR, AppearanceContentPack.Type.Sleeves, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR, AppearanceContentPack.Type.Shoes, 0);

            // Cache hair color, as previous versions (5.4 and below) did not utilize a ModData key for it
            colorManager.SetColor(Game1.player, AppearanceModel.GetColorKey(AppearanceContentPack.Type.Hair, 0), Game1.player.hairstyleColor.Value);

            // Reset the name of the internal shoe override pack
            if (textureManager.GetSpecificAppearanceModel<ShoesContentPack>(ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID) is ShoesContentPack shoePack && shoePack is not null)
            {
                shoePack.Name = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes");
                shoePack.PackName = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes");
            }
        }

        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            EnsureKeyExists(ModDataKeys.CUSTOM_HAIR_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_HAT_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_SHIRT_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_PANTS_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_SLEEVES_ID);
            EnsureKeyExists(ModDataKeys.CUSTOM_SHOES_ID);
            EnsureKeyExists(ModDataKeys.ANIMATION_FACING_DIRECTION);

            // Handle the loading cached accessories
            LoadCachedAccessories(Game1.player);

            // Set sprite to dirty in order to refresh sleeves and other tied-in appearances
            SetSpriteDirty();

            // Load our Data/PeacefulEnd/FashionSense/AppearanceData
            _ = Helper.GameContent.Load<Dictionary<string, AppearanceContentPack>>("Data/PeacefulEnd/FashionSense/AppearanceData");

            // Check if we need to give a Hand Mirror at the start of the game
            if (SDate.Now().DaysSinceStart == 1 && Game1.player.modData.ContainsKey(ModDataKeys.STARTS_WITH_HAND_MIRROR))
            {
                Monitor.Log($"Giving the Hand Mirror to player {Game1.player.Name} via letter as they enabled STARTS_WITH_HAND_MIRROR");
                Game1.player.mailbox.Add(ModDataKeys.LETTER_HAND_MIRROR);
            }
        }

        public override object GetApi()
        {
            return new Api(Monitor, textureManager, accessoryManager);
        }

        private void ReloadFashionSense(string command, string[] args)
        {
            var packFilter = args.Length > 0 ? args[0] : null;
            this.LoadContentPacks(packId: packFilter);
        }

        private void UpdateElapsedDuration(Farmer who)
        {
            foreach (var animationData in animationManager.GetAllAnimationData(who))
            {
                var elapsedDuration = animationData.ElapsedDuration;
                if (elapsedDuration < MAX_TRACKED_MILLISECONDS)
                {
                    animationData.ElapsedDuration = (elapsedDuration + Game1.currentGameTime.ElapsedGameTime.Milliseconds);
                }
            }
        }

        private void EnsureKeyExists(string key)
        {
            if (!Game1.player.modData.ContainsKey(key))
            {
                Game1.player.modData[key] = null;
            }
        }

        private void LoadContentPacks(bool silent = false, string packId = null)
        {
            // Clear the existing cache of AppearanceModels
            textureManager.Reset(packId);
            conditionGroups = new Dictionary<string, ConditionGroup>();

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned().Where(c => String.IsNullOrEmpty(packId) is true || c.Manifest.UniqueID.Equals(packId, StringComparison.OrdinalIgnoreCase)))
            {
                Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

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

                // Load Pants
                Monitor.Log($"Loading pants from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddPantsContentPacks(contentPack);

                // Load Sleeves
                Monitor.Log($"Loading sleeves from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddSleevesContentPacks(contentPack);

                // Add internal shoe pack for recoloring of vanilla shoes
                textureManager.AddAppearanceModel(new ShoesContentPack()
                {
                    Author = "PeacefulEnd",
                    Owner = "PeacefulEnd",
                    Name = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes"),
                    PackType = AppearanceContentPack.Type.Shoes,
                    PackName = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes"),
                    Id = ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID,
                    FrontShoes = new ShoesModel(),
                    BackShoes = new ShoesModel(),
                    LeftShoes = new ShoesModel(),
                    RightShoes = new ShoesModel()
                });

                // Load Shoes
                Monitor.Log($"Loading shoes from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddShoesContentPacks(contentPack);

                // Load in Condition Groups
                if (File.Exists(Path.Combine(contentPack.DirectoryPath, "conditions.json")))
                {
                    var conditions = contentPack.ReadJsonFile<Dictionary<string, List<Condition>>>("conditions.json");
                    foreach (var condition in conditions)
                    {
                        conditionGroups[$"{contentPack.Manifest.UniqueID}.{condition.Key}".ToLower()] = new ConditionGroup() { Conditions = condition.Value };
                    }
                }
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
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name); ;

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

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontHair is not null && appearanceModel.FrontHair.HairSize is null)
                    {
                        Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontHair is missing the required property HairSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackHair is not null && appearanceModel.BackHair.HairSize is null)
                    {
                        Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackHair is missing the required property HairSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftHair is not null && appearanceModel.LeftHair.HairSize is null)
                    {
                        Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftHair is missing the required property HairSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightHair is not null && appearanceModel.RightHair.HairSize is null)
                    {
                        Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightHair is missing the required property HairSize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "hair.png")))
                    {
                        Monitor.Log($"Unable to add hairstyle for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated hair.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "hair.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

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
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
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

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontAccessory is not null && appearanceModel.FrontAccessory.AccessorySize is null)
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontAccessory is missing the required property AccessorySize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackAccessory is not null && appearanceModel.BackAccessory.AccessorySize is null)
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackAccessory is missing the required property AccessorySize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftAccessory is not null && appearanceModel.LeftAccessory.AccessorySize is null)
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftAccessory is missing the required property AccessorySize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightAccessory is not null && appearanceModel.RightAccessory.AccessorySize is null)
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightAccessory is missing the required property AccessorySize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "accessory.png")))
                    {
                        Monitor.Log($"Unable to add accessory for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated accessory.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "accessory.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

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
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
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

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontHat is not null && appearanceModel.FrontHat.HatSize is null)
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontHat is missing the required property HatSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackHat is not null && appearanceModel.BackHat.HatSize is null)
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackHat is missing the required property HatSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftHat is not null && appearanceModel.LeftHat.HatSize is null)
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftHat is missing the required property HatSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightHat is not null && appearanceModel.RightHat.HatSize is null)
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightHat is missing the required property HatSize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "hat.png")))
                    {
                        Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated hat.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "hat.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

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
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
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

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontShirt is not null && appearanceModel.FrontShirt.ShirtSize is null)
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontShirt is missing the required property ShirtSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackShirt is not null && appearanceModel.BackShirt.ShirtSize is null)
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackShirt is missing the required property ShirtSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftShirt is not null && appearanceModel.LeftShirt.ShirtSize is null)
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftShirt is missing the required property ShirtSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightShirt is not null && appearanceModel.RightShirt.ShirtSize is null)
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightShirt is missing the required property ShirtSize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "shirt.png")))
                    {
                        Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated shirt.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "shirt.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

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


        private void AddPantsContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Pants"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Pants folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var pantsFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (pantsFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Pants for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the accessories
                foreach (var textureFolder in pantsFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "pants.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a pants.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "pants.json");

                    // Parse the model and assign it the content pack's owner
                    PantsContentPack appearanceModel = contentPack.ReadJsonFile<PantsContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add pants from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Pants;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a pants with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<PantsContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add pants from {contentPack.Manifest.Name}: This pack already contains a pants with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one PantsModel is given
                    if (appearanceModel.BackPants is null && appearanceModel.RightPants is null && appearanceModel.FrontPants is null && appearanceModel.LeftPants is null)
                    {
                        Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: No pants models given (FrontPants, BackPants, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontPants is not null && appearanceModel.FrontPants.PantsSize is null)
                    {
                        Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontPants is missing the required property PantsSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackPants is not null && appearanceModel.BackPants.PantsSize is null)
                    {
                        Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackPants is missing the required property PantsSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftPants is not null && appearanceModel.LeftPants.PantsSize is null)
                    {
                        Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftPants is missing the required property PantsSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightPants is not null && appearanceModel.RightPants.PantsSize is null)
                    {
                        Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightPants is missing the required property PantsSize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "pants.png")))
                    {
                        Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated pants.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "pants.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading pants from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }


        private void AddSleevesContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Sleeves"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Sleeves folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var sleevesFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (sleevesFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Sleeves for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the accessories
                foreach (var textureFolder in sleevesFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "sleeves.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a sleeves.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "sleeves.json");

                    // Parse the model and assign it the content pack's owner
                    SleevesContentPack appearanceModel = contentPack.ReadJsonFile<SleevesContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add sleeves from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Sleeves;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a sleeves with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<SleevesContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add sleeves from {contentPack.Manifest.Name}: This pack already contains a sleeves with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one SleevesModel is given
                    if (appearanceModel.BackSleeves is null && appearanceModel.RightSleeves is null && appearanceModel.FrontSleeves is null && appearanceModel.LeftSleeves is null)
                    {
                        Monitor.Log($"Unable to add sleeves for {appearanceModel.Name} from {contentPack.Manifest.Name}: No sleeves models given (FrontSleeves, BackSleeves, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontSleeves is not null && appearanceModel.FrontSleeves.SleevesSize is null)
                    {
                        Monitor.Log($"Unable to add sleeves for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontSleeves is missing the required property SleevesSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackSleeves is not null && appearanceModel.BackSleeves.SleevesSize is null)
                    {
                        Monitor.Log($"Unable to add sleeves for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackSleeves is missing the required property SleevesSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftSleeves is not null && appearanceModel.LeftSleeves.SleevesSize is null)
                    {
                        Monitor.Log($"Unable to add sleeves for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftSleeves is missing the required property SleevesSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightSleeves is not null && appearanceModel.RightSleeves.SleevesSize is null)
                    {
                        Monitor.Log($"Unable to add sleeves for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightSleeves is missing the required property SleevesSize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "sleeves.png")))
                    {
                        Monitor.Log($"Unable to add sleeves for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated sleeves.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "sleeves.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading sleeves from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }


        private void AddShoesContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Shoes"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Shoes folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var shoesFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (shoesFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Shoes for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the accessories
                foreach (var textureFolder in shoesFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "shoes.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a shoes.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "shoes.json");

                    // Parse the model and assign it the content pack's owner
                    ShoesContentPack appearanceModel = contentPack.ReadJsonFile<ShoesContentPack>(modelPath);
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add shoes from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = AppearanceContentPack.Type.Shoes;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a shoes with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<ShoesContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add shoes from {contentPack.Manifest.Name}: This pack already contains a shoes with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one ShoesModel is given
                    if (appearanceModel.BackShoes is null && appearanceModel.RightShoes is null && appearanceModel.FrontShoes is null && appearanceModel.LeftShoes is null)
                    {
                        Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: No shoes models given (FrontShoes, BackShoes, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontShoes is not null && appearanceModel.FrontShoes.ShoesSize is null)
                    {
                        Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontShoes is missing the required property ShoesSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackShoes is not null && appearanceModel.BackShoes.ShoesSize is null)
                    {
                        Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackShoes is missing the required property ShoesSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftShoes is not null && appearanceModel.LeftShoes.ShoesSize is null)
                    {
                        Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftShoes is missing the required property ShoesSize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightShoes is not null && appearanceModel.RightShoes.ShoesSize is null)
                    {
                        Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightShoes is missing the required property ShoesSize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "shoes.png")))
                    {
                        Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated shoes.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "shoes.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading shoes from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        internal static void SetCachedColor(string oldColorKey, AppearanceContentPack.Type type, int appearanceIndex)
        {
            var actualColorKey = AppearanceModel.GetColorKey(type, appearanceIndex);
            if (Game1.player.modData.ContainsKey(oldColorKey))
            {
                Game1.player.modData[actualColorKey] = Game1.player.modData[oldColorKey];
                Game1.player.modData.Remove(oldColorKey);
            }
            else if (Game1.player.modData.ContainsKey(actualColorKey) is false)
            {
                Game1.player.modData[actualColorKey] = Game1.player.hairstyleColor.Value.PackedValue.ToString();
            }

            colorManager.SetColor(Game1.player, actualColorKey, Game1.player.modData[actualColorKey]);
        }

        internal static void SetSpriteDirty()
        {
            var spriteDirty = modHelper.Reflection.GetField<bool>(Game1.player.FarmerRenderer, "_spriteDirty");
            spriteDirty.SetValue(true);
            var shirtDirty = modHelper.Reflection.GetField<bool>(Game1.player.FarmerRenderer, "_shirtDirty");
            shirtDirty.SetValue(true);
            var shoeDirty = modHelper.Reflection.GetField<bool>(Game1.player.FarmerRenderer, "_shoesDirty");
            shoeDirty.SetValue(true);

            FarmerRendererPatch.AreColorMasksPendingRefresh = true;
        }

        internal static bool ResetTextureIfNecessary(string appearanceId)
        {
            // See if we need to reset the texture (i.e. it has been overriden by the API and not using the shouldOverridePersist parameter)
            return ResetTextureIfNecessary(textureManager.GetSpecificAppearanceModel<AppearanceContentPack>(appearanceId));
        }

        internal static bool ResetTextureIfNecessary(AppearanceContentPack appearancePack)
        {
            if (appearancePack is null)
            {
                return false;
            }
            else if (appearancePack.IsTextureDirty)
            {
                appearancePack.ResetTexture();
            }

            return true;
        }

        internal static void ResetAnimationModDataFields(Farmer who, int duration, AnimationModel.Type animationType, int facingDirection, bool ignoreAnimationType = false, AppearanceModel model = null)
        {
            // Reset all apperances animation data if given model is null, otherwise reset the specific model id
            if (model is null)
            {
                foreach (var animationData in animationManager.GetAllAnimationData(who))
                {
                    animationData.Reset(duration, who.FarmerSprite.CurrentFrame, ignoreAnimationType is true ? animationData.Type : animationType);
                }

                // Resetting facing direction, though only if model is null
                who.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] = facingDirection.ToString();
            }
            else if (model.Pack.PackType is not AppearanceContentPack.Type.Accessory && animationManager.GetSpecificAnimationData(who, model.Pack.PackType) is AnimationData animationData)
            {
                animationData?.Reset(duration, who.FarmerSprite.CurrentFrame, ignoreAnimationType is true ? animationData.Type : animationType);
            }
            else if (model is AccessoryModel accessoryModel && accessoryModel is not null)
            {
                var accessoryIndex = accessoryManager.GetAccessoryIndexById(who, accessoryModel.Pack.Id);
                if (accessoryIndex != -1)
                {
                    accessoryManager.ResetAccessory(accessoryIndex, who, duration, animationType, ignoreAnimationType);
                }
            }
        }

        internal static void LoadCachedAccessories(Farmer farmer)
        {
            if (accessoryManager.HandleOldAccessoryFormat(farmer) is false && farmer.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID) && String.IsNullOrEmpty(farmer.modData[ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID]) is false)
            {
                try
                {
                    List<string> accessoryIds = JsonConvert.DeserializeObject<List<string>>(farmer.modData[ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID]);
                    List<string> accessoryColors = JsonConvert.DeserializeObject<List<string>>(farmer.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLLECTIVE_COLOR]);
                    if (accessoryIds is not null && accessoryColors is not null)
                    {
                        accessoryManager.SetAccessories(farmer, accessoryIds, accessoryColors);
                    }
                }
                catch (Exception ex)
                {
                    monitor.Log($"Failed to load accessory data for {farmer.Name}, see the log for details.", LogLevel.Warn);
                    monitor.Log($"Failed to load accessory data for {farmer.Name}: {ex}", LogLevel.Trace);
                }
            }
        }
    }
}
