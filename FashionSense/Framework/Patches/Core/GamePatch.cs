using FashionSense.Framework.Patches.Renderer;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Patches.Core
{
    internal class GamePatch : PatchTemplate
    {
        private readonly Type _entity = typeof(Game1);

        internal GamePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(Game1.drawPlayerHeldObject), new[] { typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPlayerHeldObjectPrefix)));
        }

        private static bool DrawPlayerHeldObjectPrefix(Game1 __instance, Farmer f)
        {
            if (DrawPatch.isUsingCustomDraw is true)
            {
                return false;
            }
            return true;
        }
    }
}
