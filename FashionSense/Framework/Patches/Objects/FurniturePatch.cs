using FashionSense.Framework.Patches.Renderer;
using FashionSense.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FashionSense.Framework.Patches.Objects
{
    internal class FurniturePatch : PatchTemplate
    {
        private readonly System.Type _entity = typeof(Furniture);

        internal FurniturePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(Furniture.checkForAction), new[] { typeof(Farmer), typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(CheckForActionPostfix)));
        }


        private static void CheckForActionPostfix(Furniture __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity is true)
            {
                return;
            }

            if (__instance.HasContextTag("fashionsense_furniture"))
            {
                Game1.activeClickableMenu = new HandMirrorMenu();
                __result = true;
            }
        }
    }
}
