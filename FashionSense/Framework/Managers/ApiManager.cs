using FashionSense.Framework.Interfaces;
using StardewModdingAPI;

namespace FashionSense.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IContentPatcherApi _contentPatcherApi;
        private IGenericModConfigMenuApi _genericModConfigMenuApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoContentPatcher(IModHelper helper)
        {
            _contentPatcherApi = helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");

            if (_contentPatcherApi is null)
            {
                _monitor.Log("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
            return true;
        }

        public IContentPatcherApi GetContentPatcherApi()
        {
            return _contentPatcherApi;
        }

        internal bool HookIntoGenericModConfigMenu(IModHelper helper)
        {
            _genericModConfigMenuApi = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (_genericModConfigMenuApi is null)
            {
                _monitor.Log("Failed to hook into spacechase0.GenericModConfigMenu.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into spacechase0.GenericModConfigMenu.", LogLevel.Debug);
            return true;
        }

        internal void RegisterGenericModConfigMenu(IModHelper helper, IManifest modManifest)
        {
            _genericModConfigMenuApi.Register(modManifest, () => ResetConfig(), () => helper.WriteConfig(FashionSense.modConfig));

            _genericModConfigMenuApi.AddSectionTitle(modManifest, () => helper.Translation.Get("config.general.title"));

            _genericModConfigMenuApi.AddBoolOption(modManifest, () => FashionSense.modConfig.RequireHandMirrorInInventory, value => FashionSense.modConfig.RequireHandMirrorInInventory = value, () => helper.Translation.Get("config.general.require_hand_mirror.name"), () => helper.Translation.Get("config.general.require_hand_mirror.description"));
            _genericModConfigMenuApi.AddKeybind(modManifest, () => FashionSense.modConfig.QuickMenuKey, value => FashionSense.modConfig.QuickMenuKey = value, () => helper.Translation.Get("config.general.shortcut_key.name"), () => helper.Translation.Get("config.general.shortcut_key.description"));

            _genericModConfigMenuApi.AddBoolOption(modManifest, () => FashionSense.modConfig.AllowMannequinAnimations, value => FashionSense.modConfig.AllowMannequinAnimations = value, () => helper.Translation.Get("config.general.allow_mannequin_animations.name"), () => helper.Translation.Get("config.general.allow_mannequin_animations.description"));
            _genericModConfigMenuApi.AddBoolOption(modManifest, () => FashionSense.modConfig.KeepHairWhenLoadingOutfits, value => FashionSense.modConfig.KeepHairWhenLoadingOutfits = value, () => helper.Translation.Get("config.general.keep_hair_when_loading_outfits.name"), () => helper.Translation.Get("config.general.keep_hair_when_loading_outfits.description"));
        }

        private static void ResetConfig()
        {
            FashionSense.modConfig = new ModConfig();
        }
    }
}
