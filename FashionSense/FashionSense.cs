using FashionSense.Framework;
using FashionSense.Framework.External.ContentPatcher;
using FashionSense.Framework.Interfaces.API;
using FashionSense.Framework.Managers;
using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Body;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Patches;
using FashionSense.Framework.Patches.Core;
using FashionSense.Framework.Patches.Entities;
using FashionSense.Framework.Patches.GameLocations;
using FashionSense.Framework.Patches.Menus;
using FashionSense.Framework.Patches.Objects;
using FashionSense.Framework.Patches.Renderer;
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
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FashionSense
{
    public class FashionSense : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static IManifest modManifest;

        // Managers
        internal static AccessoryManager accessoryManager;
        internal static AnimationManager animationManager;
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;
        internal static ColorManager colorManager;
        internal static LayerManager layerManager;
        internal static MessageManager messageManager;
        internal static OutfitManager outfitManager;
        internal static TextureManager textureManager;

        // Utilities
        internal static ModConfig modConfig;
        internal static Api internalApi;
        internal static ConditionData conditionData;
        internal static Dictionary<string, ConditionGroup> conditionGroups;

        // Constants
        internal const int MAX_TRACKED_MILLISECONDS = 3600000;

        // Debugging flags
        private bool _displayFarmerFrames = false;
        private bool _displayMovementData = false;
        private bool _continuousReloading = false;
        private Vector2? _cachedPlayerPosition;
        private int _lastPlayerFrame = 0;
        private bool _isRecordingPlayerFrames = false;
        private int _currentRecordedPlayerFrameIndex = 0;
        private List<int> _recordedPlayerFrames = new List<int>();

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            modManifest = ModManifest;
            modConfig = modHelper.ReadConfig<ModConfig>();

            // Load managers
            accessoryManager = new AccessoryManager(monitor);
            animationManager = new AnimationManager(monitor);
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(modHelper);
            colorManager = new ColorManager(monitor);
            layerManager = new LayerManager(monitor);
            messageManager = new MessageManager(monitor, modHelper, ModManifest.UniqueID);
            outfitManager = new OutfitManager(monitor);
            textureManager = new TextureManager(monitor);

            // Load internal API
            internalApi = new Api(Monitor, textureManager, accessoryManager);

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
                new GameLocationPatch(monitor, modHelper).Apply(harmony);

                // Apply UI related patches
                new CharacterCustomizationPatch(monitor, modHelper).Apply(harmony);
                new LetterViewerMenuPatch(monitor, modHelper).Apply(harmony);
                new SaveFileSlotPatch(monitor, modHelper).Apply(harmony);
                new InventoryPagePatch(monitor, modHelper).Apply(harmony);

                // Apply entity related patches
                new FarmerPatch(monitor, modHelper).Apply(harmony);

                // Apply object related patches
                new ItemPatch(monitor, modHelper).Apply(harmony);
                new ObjectPatch(monitor, modHelper).Apply(harmony);
                new ColoredObjectPatch(monitor, modHelper).Apply(harmony);
                new MannequinPatch(monitor, modHelper).Apply(harmony);
                new FurniturePatch(monitor, modHelper).Apply(harmony);

                // Apply clothing related patches
                new ClothingPatch(monitor, modHelper).Apply(harmony);

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
            helper.ConsoleCommands.Add("fs_display_player_frames", "Displays debug info related to player's frames (FarmerSprite.CurrentFrame). Use again to disable. \n\nUsage: fs_display_player_frames", delegate { _displayFarmerFrames = !_displayFarmerFrames; });
            helper.ConsoleCommands.Add("fs_reload", "Reloads all Fashion Sense content packs. Can specify a manifest unique ID to only reload that pack.\n\nUsage: fs_reload [manifest_unique_id]", ReloadFashionSense);
            helper.ConsoleCommands.Add("fs_reload_continuous", "Debug usage only: reloads all Fashion Sense content packs every 2 seconds. Use the command again to stop the continuous reloading.\n\nUsage: fs_reload_continuous", delegate { _continuousReloading = !_continuousReloading; });
            helper.ConsoleCommands.Add("fs_add_mirror", "Gives you a Hand Mirror tool.\n\nUsage: fs_add_mirror", delegate { Game1.player.addItemToInventory(AssetManager.GetHandMirrorTool()); });
            helper.ConsoleCommands.Add("fs_freeze_self", "Locks yourself in place, which is useful for showcasing custom appearances. Use the command again to unfreeze yourself.\n\nUsage: fs_freeze_self", delegate { _ = _cachedPlayerPosition is null ? _cachedPlayerPosition = Game1.player.Position : _cachedPlayerPosition = null; });
            helper.ConsoleCommands.Add("fs_record_frames", "Records farmer frames that are played. Use the command again to stop recording.\n\nUsage: fs_record_frames", SetPlayerFrameRecording);
            helper.ConsoleCommands.Add("fs_play_next_frame", "Plays the next recorded frame.\n\nUsage: fs_play_next_frame", PlayNextFrame);
            helper.ConsoleCommands.Add("fs_clear_recorded_frames", "Clears the recorded frames.\n\nUsage: fs_clear_recorded_frames", delegate { Monitor.Log("Cleared recorded player frames!", LogLevel.Debug); _recordedPlayerFrames.Clear(); });

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID)
            {
                messageManager.HandleIncomingMessage(e);
            }
        }

        private void OnRendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (_displayMovementData)
            {
                conditionData.OnRendered(sender, e);
            }

            if (_displayFarmerFrames && Game1.player is not null && Game1.player.FarmerSprite.CurrentFrame != _lastPlayerFrame)
            {
                _lastPlayerFrame = Game1.player.FarmerSprite.CurrentFrame;
                Monitor.Log($"Farmer Frame: {_lastPlayerFrame}", LogLevel.Debug);
            }
            else if (_isRecordingPlayerFrames && Game1.player is not null && Game1.player.FarmerSprite.CurrentFrame != _recordedPlayerFrames.LastOrDefault())
            {
                _recordedPlayerFrames.Add(Game1.player.FarmerSprite.CurrentFrame);
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
                    FashionSense.UpdateElapsedDuration(farmer);

                    // Update movement trackers
                    conditionData.Update(farmer, Game1.currentGameTime);
                }
            }

            // Update elapsed durations when the player is using the SearchMenu
            if (Game1.activeClickableMenu is SearchMenu searchMenu && searchMenu is not null)
            {
                foreach (var fakeFarmer in searchMenu.fakeFarmers)
                {
                    FashionSense.UpdateElapsedDuration(fakeFarmer);
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
            foreach (var animationData in animationManager.GetAllAnimationData(e.Player).Where(a => string.IsNullOrEmpty(a.LightId) is false))
            {
                e.OldLocation.sharedLights.Remove(animationData.LightId);
            }
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && apiManager.HookIntoContentPatcher(Helper))
            {
                apiManager.GetContentPatcherApi().RegisterToken(ModManifest, "Appearance", new AppearanceToken());
            }

            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu") && apiManager.HookIntoGenericModConfigMenu(Helper))
            {
                apiManager.RegisterGenericModConfigMenu(Helper, ModManifest);
            }

            // Load any owned content packs
            this.LoadContentPacks();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsWorldReady is false || Game1.activeClickableMenu is not null || Game1.player is null)
            {
                return;
            }

            if (e.Button == modConfig.QuickMenuKey)
            {
                if (modConfig.RequireHandMirrorInInventory && Game1.player.Items.Any(i => i is not null && i.modData is not null && i.modData.ContainsKey(ModDataKeys.HAND_MIRROR_FLAG)) is false)
                {
                    Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("messages.warning.requires_hand_mirror"), 3));
                }
                else
                {
                    Game1.activeClickableMenu = new HandMirrorMenu();
                }
            }
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
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Tools"))
            {
                e.Edit(assetManager.AddToolData, AssetEditPriority.Early);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(assetManager.EditShopData, AssetEditPriority.Early);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(AssetManager.HAND_MIRROR_TEXTURE_PATH))
            {
                e.LoadFrom(assetManager.GetHandMirrorTexture, AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Hats"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    foreach (var hatModel in textureManager.GetAllAppearanceModels<HatContentPack>().Where(s => s.Item is not null))
                    {
                        data[hatModel.Item.Id] = $"{hatModel.Item.DisplayName}/{hatModel.Item.Description}/false/true//{hatModel.Item.DisplayName}";
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shirts"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ShirtData>().Data;
                    foreach (var shirtModel in textureManager.GetAllAppearanceModels<ShirtContentPack>().Where(s => s.Item is not null))
                    {
                        data[shirtModel.Item.Id] = new ShirtData()
                        {
                            DisplayName = shirtModel.Item.DisplayName,
                            Description = shirtModel.Item.Description,
                            Price = shirtModel.Item.Price
                        };
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Pants"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, PantsData>().Data;
                    foreach (var pantsModel in textureManager.GetAllAppearanceModels<PantsContentPack>().Where(s => s.Item is not null))
                    {
                        data[pantsModel.Item.Id] = new PantsData()
                        {
                            DisplayName = pantsModel.Item.DisplayName,
                            Description = pantsModel.Item.Description,
                            Price = pantsModel.Item.Price
                        };
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Boots"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    foreach (var shoesModel in textureManager.GetAllAppearanceModels<ShoesContentPack>().Where(s => s.Item is not null))
                    {
                        data[shoesModel.Item.Id] = $"{shoesModel.Item.DisplayName}/{shoesModel.Item.Description}/{shoesModel.Item.Price}/0/0/0/{shoesModel.Item.DisplayName}";
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/AppearanceData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<AppearanceContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/AccessoryData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<AccessoryContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/HatData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<HatContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/HairData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<HairContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/ShirtData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<ShirtContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/SleevesData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<SleevesContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/PantsData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<PantsContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/ShoesData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<ShoesContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd/FashionSense/BodyData"))
            {
                e.LoadFrom(() => textureManager.GetIdToAppearanceModels<BodyContentPack>(), AssetLoadPriority.High);
            }
        }

        private void OnAssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            var appearanceDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/AppearanceData"));
            if (appearanceDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, AppearanceContentPack>>(appearanceDataAsset));
            }

            var accessoryDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/AccessoryData"));
            if (accessoryDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, AccessoryContentPack>>(accessoryDataAsset), IApi.Type.Accessory);
            }

            var hatDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/HatData"));
            if (hatDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, HatContentPack>>(hatDataAsset), IApi.Type.Hat);
            }

            var hairDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/HairData"));
            if (hairDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, HairContentPack>>(hairDataAsset), IApi.Type.Hair);
            }

            var shirtDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/ShirtData"));
            if (shirtDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, ShirtContentPack>>(shirtDataAsset), IApi.Type.Shirt);
            }

            var sleevesDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/SleevesData"));
            if (sleevesDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, SleevesContentPack>>(sleevesDataAsset), IApi.Type.Sleeves);
            }

            var pantsDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/PantsData"));
            if (pantsDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, PantsContentPack>>(pantsDataAsset), IApi.Type.Pants);
            }

            var shoesDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/ShoesData"));
            if (shoesDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, ShoesContentPack>>(shoesDataAsset), IApi.Type.Shoes);
            }

            var bodyDataAsset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd/FashionSense/BodyData"));
            if (bodyDataAsset is not null)
            {
                textureManager.Sync(Helper.GameContent.Load<Dictionary<string, BodyContentPack>>(bodyDataAsset), IApi.Type.Player);
            }
        }

        private void OnSaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            // Reset Hand Mirror UI
            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = String.Empty;

            // Set the cached colors, if needed
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR, IApi.Type.Accessory, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR, IApi.Type.Accessory, 1);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR, IApi.Type.Accessory, 2);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_HAT_COLOR, IApi.Type.Hat, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR, IApi.Type.Shirt, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR, IApi.Type.Pants, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR, IApi.Type.Sleeves, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR, IApi.Type.Shoes, 0);
            SetCachedColor(ModDataKeys.UI_HAND_MIRROR_BODY_COLOR, IApi.Type.Player, 0);

            // Cache hair color, as previous versions (5.4 and below) did not utilize a ModData key for it
            colorManager.SetColor(Game1.player, AppearanceModel.GetColorKey(IApi.Type.Hair, 0), Game1.player.hairstyleColor.Value);

            // Reset the name of the internal shoe override pack
            if (textureManager.GetSpecificAppearanceModel<ShoesContentPack>(ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID) is ShoesContentPack shoePack && shoePack is not null)
            {
                shoePack.Name = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes");
                shoePack.PackName = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes");
            }

            // Reset the name of the internal shoe override pack
            if (textureManager.GetSpecificAppearanceModel<BodyContentPack>(ModDataKeys.INTERNAL_COLOR_OVERRIDE_BODY_ID) is BodyContentPack bodyPack && bodyPack is not null)
            {
                bodyPack.Name = modHelper.Translation.Get("ui.fashion_sense.color_override.body");
                bodyPack.PackName = modHelper.Translation.Get("ui.fashion_sense.color_override.body");
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
            EnsureKeyExists(ModDataKeys.CUSTOM_BODY_ID);
            EnsureKeyExists(ModDataKeys.ANIMATION_FACING_DIRECTION);

            // Handle the loading cached accessories
            LoadCachedAccessories(Game1.player);

            // Set sprite to dirty in order to refresh sleeves and other tied-in appearances
            SetSpriteDirty(Game1.player);

            // Load our Data/PeacefulEnd/FashionSense/AppearanceData
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, AppearanceContentPack>>("Data/PeacefulEnd/FashionSense/AppearanceData"));

            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, AccessoryContentPack>>("Data/PeacefulEnd/FashionSense/AccessoryData"), IApi.Type.Accessory);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, HatContentPack>>("Data/PeacefulEnd/FashionSense/HatData"), IApi.Type.Hat);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, HairContentPack>>("Data/PeacefulEnd/FashionSense/HairData"), IApi.Type.Hair);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, ShirtContentPack>>("Data/PeacefulEnd/FashionSense/ShirtData"), IApi.Type.Shirt);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, SleevesContentPack>>("Data/PeacefulEnd/FashionSense/SleevesData"), IApi.Type.Sleeves);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, PantsContentPack>>("Data/PeacefulEnd/FashionSense/PantsData"), IApi.Type.Pants);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, ShoesContentPack>>("Data/PeacefulEnd/FashionSense/ShoesData"), IApi.Type.Shoes);
            textureManager.Sync(Helper.GameContent.Load<Dictionary<string, BodyContentPack>>("Data/PeacefulEnd/FashionSense/BodyData"), IApi.Type.Player);

            // Check if we need to give a Hand Mirror at the start of the game
            if (SDate.Now().DaysSinceStart == 1 && Game1.player.modData.ContainsKey(ModDataKeys.STARTS_WITH_HAND_MIRROR))
            {
                Monitor.Log($"Giving the Hand Mirror to player {Game1.player.Name} via letter as they enabled STARTS_WITH_HAND_MIRROR");
                Game1.player.mailbox.Add(ModDataKeys.LETTER_HAND_MIRROR);
            }
            else if (Game1.player.modData.ContainsKey(ModDataKeys.HAS_CONVERTED_MIRROR_TO_TOOL) is false)
            {
                Monitor.Log("Converting hand mirror tools to data tools...", LogLevel.Debug);
                Utility.ForEachItemContext((in context) =>
                {
                    if (context.Item is not GenericTool genericTool || genericTool.modData.ContainsKey(ModDataKeys.HAND_MIRROR_FLAG) is false)
                    {
                        return true;
                    }
                    context.ReplaceItemWith(AssetManager.GetHandMirrorTool());
                    return true;
                });
                Game1.player.modData[ModDataKeys.HAS_CONVERTED_MIRROR_TO_TOOL] = true.ToString();
            }
        }

        public override object GetApi()
        {
            return internalApi;
        }

        private void ReloadFashionSense(string command, string[] args)
        {
            var packFilter = args.Length > 0 ? args[0] : null;
            this.LoadContentPacks(packId: packFilter);
        }

        private void SetPlayerFrameRecording(string command, string[] args)
        {
            if (_isRecordingPlayerFrames)
            {
                Monitor.Log($"Disabling player frame recording", LogLevel.Debug);
                _isRecordingPlayerFrames = false;
            }
            else
            {
                _recordedPlayerFrames.Clear();

                Monitor.Log($"Enabling player frame recording", LogLevel.Debug);
                _isRecordingPlayerFrames = true;
            }
        }

        private void PlayNextFrame(string command, string[] args)
        {
            if (_isRecordingPlayerFrames)
            {
                Monitor.Log($"Disabling player frame recording", LogLevel.Debug);
                _isRecordingPlayerFrames = false;
            }

            _currentRecordedPlayerFrameIndex = _recordedPlayerFrames.Count() > _currentRecordedPlayerFrameIndex + 1 ? _currentRecordedPlayerFrameIndex + 1 : 0;
            if (_recordedPlayerFrames.Count() > 0)
            {
                Game1.player.FarmerSprite.setCurrentFrame(_recordedPlayerFrames[_currentRecordedPlayerFrameIndex]);

                Monitor.Log($"Playing frame {_recordedPlayerFrames[_currentRecordedPlayerFrameIndex]}", LogLevel.Debug);
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
            Stopwatch collectiveLoadingStopwatch = Stopwatch.StartNew();

            // Clear the existing cache of AppearanceModels
            textureManager.Reset(packId);
            conditionGroups = new Dictionary<string, ConditionGroup>();

            // Clear the preset outfits
            outfitManager.ClearPresetOutfits();

            // Gather the content packs for Fashion Sense
            var contentPacks = Helper.ContentPacks.GetOwned().Where(c => String.IsNullOrEmpty(packId) is true || c.Manifest.UniqueID.Equals(packId, StringComparison.OrdinalIgnoreCase)).ToList();

            // Add our local pack
            contentPacks.Add(assetManager.GetLocalPack(update: true));

            // Load owned content packs
            foreach (IContentPack contentPack in contentPacks)
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
                    PackType = IApi.Type.Shoes,
                    PackId = ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID,
                    PackName = modHelper.Translation.Get("ui.fashion_sense.color_override.shoes"),
                    Id = ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID,
                    FrontShoes = new ShoesModel() { ShoesSize = new Size() },
                    BackShoes = new ShoesModel() { ShoesSize = new Size() },
                    LeftShoes = new ShoesModel() { ShoesSize = new Size() },
                    RightShoes = new ShoesModel() { ShoesSize = new Size() }
                });

                // Load Shoes
                Monitor.Log($"Loading shoes from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddShoesContentPacks(contentPack);

                // Add internal body pack for recoloring of vanilla body
                textureManager.AddAppearanceModel(new BodyContentPack()
                {
                    Author = "PeacefulEnd",
                    Owner = "PeacefulEnd",
                    Name = modHelper.Translation.Get("ui.fashion_sense.color_override.body"),
                    PackType = IApi.Type.Player,
                    PackId = ModDataKeys.INTERNAL_COLOR_OVERRIDE_BODY_ID,
                    PackName = modHelper.Translation.Get("ui.fashion_sense.color_override.body"),
                    Id = ModDataKeys.INTERNAL_COLOR_OVERRIDE_BODY_ID,
                    FrontBody = new BodyModel() { BodySize = new Size() },
                    BackBody = new BodyModel() { BodySize = new Size() },
                    LeftBody = new BodyModel() { BodySize = new Size() },
                    RightBody = new BodyModel() { BodySize = new Size() }
                });

                // Load Bodies
                Monitor.Log($"Loading bodies from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddBodiesContentPacks(contentPack);

                // Load Outfit Presets
                Monitor.Log($"Loading outfit presets from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                if (File.Exists(Path.Combine(contentPack.DirectoryPath, "preset_outfits.json")))
                {
                    var outfits = contentPack.ReadJsonFile<List<Outfit>>("preset_outfits.json");
                    foreach (var outfit in outfits)
                    {
                        if (string.IsNullOrEmpty(outfit.Author))
                        {
                            outfit.Author = contentPack.Manifest.Author;
                        }
                        outfit.Source = contentPack.Manifest.Name;
                        outfit.IsPreset = true;

                        outfitManager.AddPresetOutfit(outfit);
                    }
                }

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
                SetSpriteDirty(Game1.player);
            }

            collectiveLoadingStopwatch.Stop();
            monitor.Log($"Finished loading all content packs in {Math.Round(collectiveLoadingStopwatch.ElapsedMilliseconds / 1000f, 2)} seconds", LogLevel.Trace);
        }

        private static DirectoryInfo GetContentPackDirectory(IContentPack contentPack, string targetDirectoryName)
        {
            var contentPackDirectory = new DirectoryInfo(contentPack.DirectoryPath);

            foreach (var directory in contentPackDirectory.GetDirectories())
            {
                if (directory.Name.Equals(targetDirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
            }

            return new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, targetDirectoryName));
        }

        private void AddHairContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = GetContentPackDirectory(contentPack, "Hairs");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Hairs folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var hairFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add hairstyle from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Hair;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
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
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
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
                var directoryPath = GetContentPackDirectory(contentPack, "Accessories");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Accessories folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var accessoryFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add accessories from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Accessory;

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
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
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
                var directoryPath = GetContentPackDirectory(contentPack, "Hats");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Hats folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var hatFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add hats from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Hat;

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

                    // Handle ItemModel, if given
                    if (appearanceModel.Item is not null)
                    {
                        if (appearanceModel.Item.IsValid() is false)
                        {
                            Monitor.Log($"Unable to add hat for {appearanceModel.Name} from {contentPack.Manifest.Name}: Invalid Item property. Ensure that SpritePosition and SpriteSize are given.", LogLevel.Warn);
                            continue;
                        }

                        appearanceModel.SetItemData();
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "hat.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
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
                var directoryPath = GetContentPackDirectory(contentPack, "Shirts");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Shirts folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var shirtFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add shirts from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Shirt;

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

                    // Handle ItemModel, if given
                    if (appearanceModel.Item is not null)
                    {
                        if (appearanceModel.Item.IsValid() is false)
                        {
                            Monitor.Log($"Unable to add shirt for {appearanceModel.Name} from {contentPack.Manifest.Name}: Invalid Item property. Ensure that SpritePosition and SpriteSize are given.", LogLevel.Warn);
                            continue;
                        }

                        appearanceModel.SetItemData();
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "shirt.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
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
                var directoryPath = GetContentPackDirectory(contentPack, "Pants");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Pants folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var pantsFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add pants from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Pants;

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

                    // Handle ItemModel, if given
                    if (appearanceModel.Item is not null)
                    {
                        if (appearanceModel.Item.IsValid() is false)
                        {
                            Monitor.Log($"Unable to add pants for {appearanceModel.Name} from {contentPack.Manifest.Name}: Invalid Item property. Ensure that SpritePosition and SpriteSize are given.", LogLevel.Warn);
                            continue;
                        }

                        appearanceModel.SetItemData();
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "pants.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
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
                var directoryPath = GetContentPackDirectory(contentPack, "Sleeves");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Sleeves folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var sleevesFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add sleeves from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Sleeves;

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
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
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
                var directoryPath = GetContentPackDirectory(contentPack, "Shoes");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Shoes folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var shoesFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
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
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add shoes from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Shoes;

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

                    // Handle ItemModel, if given
                    if (appearanceModel.Item is not null)
                    {
                        if (appearanceModel.Item.IsValid() is false)
                        {
                            Monitor.Log($"Unable to add shoes for {appearanceModel.Name} from {contentPack.Manifest.Name}: Invalid Item property. Ensure that SpritePosition and SpriteSize are given.", LogLevel.Warn);
                            continue;
                        }

                        appearanceModel.SetItemData();
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "shoes.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading shoes from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        private void AddBodiesContentPacks(IContentPack contentPack)
        {
            try
            {
                var directoryPath = GetContentPackDirectory(contentPack, "Bodies");
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Bodies folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var bodiesFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories).OrderBy(d => d.Name);
                if (bodiesFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Bodies for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the folders
                foreach (var textureFolder in bodiesFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "body.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a body.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "body.json");

                    // Parse the model and assign it the content pack's owner
                    BodyContentPack appearanceModel = contentPack.ReadJsonFile<BodyContentPack>(modelPath);
                    appearanceModel.IsLocalPack = true;
                    appearanceModel.Author = contentPack.Manifest.Author;
                    appearanceModel.Owner = contentPack.Manifest.UniqueID;

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(appearanceModel.Name))
                    {
                        Monitor.Log($"Unable to add body from {appearanceModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the model type
                    appearanceModel.PackType = IApi.Type.Player;

                    // Set the PackName and Id
                    appearanceModel.PackName = contentPack.Manifest.Name;
                    appearanceModel.PackId = contentPack.Manifest.UniqueID;
                    appearanceModel.Id = String.Concat(appearanceModel.Owner, "/", appearanceModel.PackType, "/", appearanceModel.Name);

                    // Verify that a bodies with the name doesn't exist in this pack
                    if (textureManager.GetSpecificAppearanceModel<BodyContentPack>(appearanceModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add body from {contentPack.Manifest.Name}: This pack already contains a body with the name of {appearanceModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify that at least one BodyModel is given
                    if (appearanceModel.BackBody is null && appearanceModel.RightBody is null && appearanceModel.FrontBody is null && appearanceModel.LeftBody is null)
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: No body models given (FrontBody, BackBody, etc.)", LogLevel.Warn);
                        continue;
                    }

                    // Verify the Size model is not null foreach given direction
                    if (appearanceModel.FrontBody is not null && appearanceModel.FrontBody.BodySize is null)
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: FrontBody is missing the required property BodySize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.BackBody is not null && appearanceModel.BackBody.BodySize is null)
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: BackBody is missing the required property BodySize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.LeftBody is not null && appearanceModel.LeftBody.BodySize is null)
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: LeftBody is missing the required property BodySize", LogLevel.Warn);
                        continue;
                    }
                    if (appearanceModel.RightBody is not null && appearanceModel.RightBody.BodySize is null)
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: RightBody is missing the required property BodySize", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "body.png")))
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated body.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    appearanceModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "body.png")).Name);

                    // Verify we are given the eyes texture and if so, track it
                    if (appearanceModel.HideEyes is false && !File.Exists(Path.Combine(textureFolder.FullName, "eyes.png")))
                    {
                        Monitor.Log($"Unable to add body for {appearanceModel.Name} from {contentPack.Manifest.Name}: No associated eyes.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the eyes texture
                    appearanceModel.EyesTexture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, "eyes.png")).Name);

                    // Link the content pack's ID to the model
                    appearanceModel.LinkId();

                    // Track the model
                    textureManager.AddAppearanceModel(appearanceModel);

                    // Log it
                    //Monitor.Log(appearanceModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading body from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        internal static void UpdateElapsedDuration(Farmer who)
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

        internal static void SetCachedColor(string oldColorKey, IApi.Type type, int appearanceIndex)
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

        internal static void SetSpriteDirty(Farmer who, bool skipColorMaskRefresh = false)
        {
            var spriteDirty = modHelper.Reflection.GetField<bool>(who.FarmerRenderer, "_spriteDirty");
            spriteDirty.SetValue(true);
            var shirtDirty = modHelper.Reflection.GetField<bool>(who.FarmerRenderer, "_shirtDirty");
            shirtDirty.SetValue(true);
            var shoeDirty = modHelper.Reflection.GetField<bool>(who.FarmerRenderer, "_shoesDirty");
            shoeDirty.SetValue(true);
            var skinDirty = modHelper.Reflection.GetField<bool>(who.FarmerRenderer, "_skinDirty");
            skinDirty.SetValue(true);

            if (skipColorMaskRefresh is false)
            {
                FarmerRendererPatch.AreColorMasksPendingRefresh = true;
            }

            internalApi.OnSetSpriteDirtyTriggered(new EventArgs());
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
            else if (model.Pack.PackType is not IApi.Type.Accessory && animationManager.GetSpecificAnimationData(who, model.Pack.PackType) is AnimationData animationData)
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
