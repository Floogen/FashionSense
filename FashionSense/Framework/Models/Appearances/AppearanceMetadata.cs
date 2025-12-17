using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FashionSense.Framework.Models.Appearances
{
    public class AppearanceMetadata
    {
        public AppearanceModel Model { get; set; }
        public List<Color> Colors { get; set; }

        public AppearanceMetadata(AppearanceModel model, List<Color> colors)
        {
            Model = model;
            Colors = colors;
        }

        public bool IsValid()
        {
            return Model is not null && Model.Pack is not null && Model.Pack.Texture is not null;
        }
    }
}
