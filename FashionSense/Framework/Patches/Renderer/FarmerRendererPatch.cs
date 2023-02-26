using FashionSense.Framework.Managers;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Patches.Renderer
{
    internal class FarmerRendererPatch : PatchTemplate
    {
        internal static bool AreColorMasksPendingRefresh = false;
        private readonly Type _entity = typeof(FarmerRenderer);

        internal FarmerRendererPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.drawHairAndAccesories), new[] { typeof(SpriteBatch), typeof(int), typeof(Farmer), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(float), typeof(Color), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawHairAndAccesoriesPrefix)));
            harmony.Patch(AccessTools.Method(_entity, nameof(FarmerRenderer.drawMiniPortrat), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DrawMiniPortratPrefix)));
        }


        private static bool DrawHairAndAccesoriesPrefix(FarmerRenderer __instance, bool ___isDrawingForUI, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_ACCESSORY_ID) && !who.modData.ContainsKey(ModDataKeys.CUSTOM_HAT_ID))
            {
                return true;
            }

            return false;
        }

        private static bool DrawMiniPortratPrefix(FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who)
        {
            if (!who.modData.ContainsKey(ModDataKeys.CUSTOM_HAIR_ID) || ___baseTexture is null || ___baseTexture.IsDisposed)
            {
                return true;
            }

            var hairPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(who.modData[ModDataKeys.CUSTOM_HAIR_ID]);
            if (hairPack is null)
            {
                return true;
            }

            // This is in the vanilla code, which for some reason is always 2 instead of relying on facingDirection's initial value
            facingDirection = 2;

            HairModel hairModel = hairPack.GetHairFromFacingDirection(facingDirection);
            if (hairModel is null)
            {
                return true;
            }
            Rectangle sourceRect = new Rectangle(hairModel.StartingPosition.X, hairModel.StartingPosition.Y, hairModel.HairSize.Width, hairModel.HairSize.Length);

            // Execute recolor
            DrawPatch.ExecuteRecolorActionsReversePatch(__instance, who);

            // Get the hairs current color
            var hairColor = who.hairstyleColor.Value;
            if (hairModel.DisableGrayscale)
            {
                hairColor = Color.White;
            }
            else if (hairModel.IsPrismatic)
            {
                hairColor = Utility.GetPrismaticColor(speedMultiplier: hairModel.PrismaticAnimationSpeedMultiplier);
            }

            // Get hair metadata
            HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);

            // Vanilla logic to determine player's head position (though largely useless as it always executes facingDirection == 2)
            bool flip = false;
            int yOffset = 0;
            int feature_y_offset = 0;
            switch (facingDirection)
            {
                case 0:
                    yOffset = 64;
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[12];
                    break;
                case 3:
                    if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                    {
                        yOffset = 96;
                    }
                    else
                    {
                        yOffset = 32;
                    }
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[6];
                    break;
                case 1:
                    yOffset = 32;
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[6];
                    break;
                case 2:
                    yOffset = 0;
                    feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[0];
                    break;
            }
            feature_y_offset -= who.isMale ? 1 : 0;

            // Draw the player's face, then the custom hairstyle
            b.Draw(___baseTexture, position, new Rectangle(0, yOffset, 16, who.isMale ? 15 : 16), Color.White, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

            // Draw the hair
            float hair_draw_layer = 2.2E-05f;
            b.Draw(hairPack.Texture, position + new Vector2(0f, feature_y_offset * 4), sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, hairModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);

            if (hairModel.HasColorMask())
            {
                DrawManager.DrawColorMask(b, hairPack, hairModel, FarmerRendererPatch.AreColorMasksPendingRefresh, position + new Vector2(0f, feature_y_offset * 4) * scale / 4f, sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, layerDepth + hair_draw_layer + 0.01E-05f);
            }
            if (hairModel.HasSkinToneMask())
            {
                // Get skin tone
                var skinTone = DrawPatch.GetSkinTone(___farmerTextureManager, ___baseTexture, null, ___skin, ____sickFrame);
                DrawManager.DrawSkinToneMask(b, hairPack, hairModel, skinTone, FarmerRendererPatch.AreColorMasksPendingRefresh, position + new Vector2(0f, feature_y_offset * 4) * scale / 4f, sourceRect, hairColor, 0f, new Vector2(hairModel.HeadPosition.X, hairModel.HeadPosition.Y), scale, layerDepth + hair_draw_layer + 0.01E-05f);
            }

            return false;
        }
    }
}
