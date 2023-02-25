using FashionSense.Framework.Models.Appearances.Generic;

namespace FashionSense.Framework.Models.Appearances.Pants
{
    public class PantsModel : AppearanceModel
    {
        public Position BodyPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size PantsSize { get; set; }
        public bool HideLegs { get; set; }
        public bool HideShadow { get; set; }
    }
}
