using FashionSense.Framework.Patches.Renderer;
using FashionSense.Framework.Utilities;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace FashionSense.Framework.Patches.Core
{
    internal class GamePatch : PatchTemplate
    {
        private readonly System.Type _entity = typeof(Game1);

        internal GamePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(Game1.drawTool), new[] { typeof(Farmer), typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(DrawToolPrefix)));

            harmony.CreateReversePatcher(AccessTools.Method(_entity, nameof(Game1.IsRainingHere), new[] { typeof(GameLocation) }), new HarmonyMethod(GetType(), nameof(IsRainingHereReversePatch))).Patch();
            harmony.CreateReversePatcher(AccessTools.Method(_entity, nameof(Game1.IsSnowingHere), new[] { typeof(GameLocation) }), new HarmonyMethod(GetType(), nameof(IsSnowingHereReversePatch))).Patch();
        }

        private static bool DrawToolPrefix(Game1 __instance, Farmer f, int currentToolIndex)
        {
            if (f is null || f.CurrentTool is null || f.CurrentTool.modData.ContainsKey(ModDataKeys.HAND_MIRROR_FLAG) is false)
            {
                return true;
            }

            return false;
        }

        internal static bool IsRainingHereReversePatch(GameLocation location = null)
        {
            return false;
        }

        internal static bool IsSnowingHereReversePatch(GameLocation location = null)
        {
            return false;
        }
    }
}
