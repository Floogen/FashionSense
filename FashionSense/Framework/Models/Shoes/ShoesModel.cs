using FashionSense.Framework.Models.Generic;

namespace FashionSense.Framework.Models.Shoes
{
    public class ShoesModel : AppearanceModel
    {
        public Position BodyPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size ShoesSize { get; set; }
        public bool DrawBeforePants { get; set; }
    }
}
