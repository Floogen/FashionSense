using FashionSense.Framework.Patches.Renderer;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

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
            harmony.Patch(AccessTools.Method(_entity, nameof(Game1.drawTool), new[] { typeof(Farmer), typeof(int) }), transpiler: new HarmonyMethod(GetType(), nameof(DrawToolTranspiler)));

            harmony.CreateReversePatcher(AccessTools.Method(_entity, nameof(Game1.IsRainingHere), new[] { typeof(GameLocation) }), new HarmonyMethod(GetType(), nameof(IsRainingHereReversePatch))).Patch();
            harmony.CreateReversePatcher(AccessTools.Method(_entity, nameof(Game1.IsSnowingHere), new[] { typeof(GameLocation) }), new HarmonyMethod(GetType(), nameof(IsSnowingHereReversePatch))).Patch();
        }

        private static IEnumerable<CodeInstruction> DrawToolTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                var list = instructions.ToList();

                // Get the indices to insert at
                List<int> indices = new List<int>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].opcode == OpCodes.Call && list[i].operand is not null && list[i].operand.ToString().Contains("Max", System.StringComparison.OrdinalIgnoreCase))
                    {
                        indices.Add(i);
                    }
                }

                // Insert the changes at the specified indices
                foreach (var index in indices.OrderByDescending(i => i))
                {
                    list.Insert(index + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GamePatch), nameof(AdjustLayerDepthForHeldObjects))));
                }

                return list;
            }
            catch (System.Exception e)
            {
                _monitor.Log($"There was an issue modifying the instructions for StardewValley.Game1.drawTool: {e}", LogLevel.Error);
                return instructions;
            }
        }

        private static float AdjustLayerDepthForHeldObjects(float layerDepth)
        {
            return Game1.player.FacingDirection == 0 || DrawPatch.lastCustomLayerDepth is null ? layerDepth : DrawPatch.lastCustomLayerDepth.Value + 0.0001f;
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
