using FashionSense.Framework.Models.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using static FashionSense.Framework.Interfaces.API.IApi;
using static StardewValley.FarmerSprite;

namespace FashionSense.Framework.Models.Appearances.Generic
{
    public class DrawTool : IDrawTool
    {
        public Farmer Farmer { get; init; }
        public SpriteBatch SpriteBatch { get; init; }
        public FarmerRenderer FarmerRenderer { get; init; }
        public Texture2D BaseTexture { get; init; }
        public Rectangle FarmerSourceRectangle { get; init; }
        public AnimationFrame AnimationFrame { get; init; }
        public bool IsDrawingForUI { get; init; }
        public Color OverrideColor { get; init; }
        public Color AppearanceColor { get; set; }
        public Vector2 Position { get; init; }
        public Vector2 Origin { get; init; }
        public Vector2 PositionOffset { get; init; }
        public int FacingDirection { get; init; }
        public int CurrentFrame { get; init; }
        public float Scale { get; init; }
        public float Rotation { get; init; }
        public float LayerDepthSnapshot { get; set; }

        internal void SetAppearanceColor(LayerData layer)
        {
            if (layer.AppearanceModel is null || layer.Colors is null)
            {
                AppearanceColor = Color.White;
                return;
            }

            var modelColor = layer.Colors.Count == 0 ? Color.White : layer.Colors[0];
            if (layer.AppearanceModel.DisableGrayscale)
            {
                modelColor = Color.White;
            }
            else if (layer.AppearanceModel.IsPrismatic)
            {
                modelColor = Utility.GetPrismaticColor(speedMultiplier: layer.AppearanceModel.PrismaticAnimationSpeedMultiplier);
            }

            AppearanceColor = modelColor;
        }
    }
}
