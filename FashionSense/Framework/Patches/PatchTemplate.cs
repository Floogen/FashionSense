using StardewModdingAPI;

namespace FashionSense.Framework.Patches
{
    internal class PatchTemplate
    {
        internal static IMonitor _monitor;
        internal static IModHelper _helper;

        internal PatchTemplate(IMonitor modMonitor, IModHelper modHelper)
        {
            _monitor = modMonitor;
            _helper = modHelper;
        }

        internal static bool IsBAGIUsed()
        {
            return _helper.ModRegistry.IsLoaded("cat.betterartisangoodicons");
        }
    }
}
