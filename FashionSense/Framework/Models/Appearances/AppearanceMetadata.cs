using Microsoft.Xna.Framework;

namespace FashionSense.Framework.Models.Appearances
{
    public class AppearanceMetadata
    {
        public AppearanceModel Model { get; set; }
        public Color Color { get; set; }

        public AppearanceMetadata(AppearanceModel model, Color color)
        {
            Model = model;
            Color = color;
        }
    }
}
