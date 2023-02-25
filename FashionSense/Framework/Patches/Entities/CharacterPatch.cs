using HarmonyLib;
using FashionSense.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;
using System;
using Microsoft.Xna.Framework;
using FashionSense.Framework.Models.Pants;

namespace FashionSense.Framework.Patches.Entities
{
    internal class CharacterPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Character);

        internal CharacterPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Character.GetShadowOffset), null), postfix: new HarmonyMethod(GetType(), nameof(GetShadowOffsetPostfix)));
        }

        private static void GetShadowOffsetPostfix(Character __instance, ref Vector2 __result)
        {
            if (__instance is not Farmer)
            {
                return;
            }

            // Get the pants model, if applicable
            PantsModel pantsModel = null;
            if (__instance.modData.ContainsKey(ModDataKeys.CUSTOM_PANTS_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(__instance.modData[ModDataKeys.CUSTOM_PANTS_ID]) is PantsContentPack pPack && pPack != null)
            {
                pantsModel = pPack.GetPantsFromFacingDirection(__instance.FacingDirection);
            }

            if (pantsModel is null || !pantsModel.HideShadow)
            {
                return;
            }

            // Not exactly the preferred solution, but only other viable route would be to transpile Game1._draw to change how _farmerShadows is iterated
            __result = new Vector2(0, Int32.MinValue);
        }
    }
}
