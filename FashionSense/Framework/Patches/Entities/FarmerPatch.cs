using HarmonyLib;
using FashionSense.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using FashionSense.Framework.Models.Shirt;

namespace FashionSense.Framework.Patches.Entities
{
    internal class FarmerPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Farmer);

        internal FarmerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.PropertySetter(_object, nameof(Farmer.FacingDirection)), postfix: new HarmonyMethod(GetType(), nameof(FaceDirectionPostfix)));
        }

        private static void FaceDirectionPostfix(Farmer __instance)
        {
            ShirtModel shirtModel = null;
            if (__instance.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(__instance.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
            {
                shirtModel = sPack.GetShirtFromFacingDirection(__instance.FacingDirection);
            }

            if (shirtModel is null || !__instance.modData.ContainsKey(ModDataKeys.ANIMATION_FACING_DIRECTION) || __instance.modData[ModDataKeys.ANIMATION_FACING_DIRECTION] == __instance.FacingDirection.ToString())
            {
                return;
            }


            FashionSense.SetSpriteDirty();
        }
    }
}
