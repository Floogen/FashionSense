using StardewModdingAPI;

namespace FashionSense.Framework
{
    public class ModConfig
    {
        public bool RequireHandMirrorInInventory { get; set; } = true;
        public SButton QuickMenuKey { get; set; }
        public bool AllowMannequinAnimations { get; set; } = true;
        public bool KeepHairWhenLoadingOutfits { get; set; } = false;
    }
}
