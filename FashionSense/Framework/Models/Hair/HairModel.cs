using FashionSense.Framework.Models.Generic;

namespace FashionSense.Framework.Models.Hair
{
    public class HairModel : AppearanceModel
    {
        public Position HeadPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size HairSize { get; set; }

        public override bool HideWhileSwimming { get; set; } = false;
        public override bool HideWhileWearingBathingSuit { get; set; } = false;
    }
}
