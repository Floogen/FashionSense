using FashionSense.Framework.Models.Appearances;
using Microsoft.Xna.Framework;

namespace FashionSense.Framework.Models.General
{
    internal class LayerData
    {
        public AppearanceContentPack.Type AppearanceType { get; set; }
        public AppearanceModel AppearanceModel { get; set; }
        public Color Color { get; set; }
        public bool IsVanilla { get; set; }
        public bool IsHidden { get; set; }

        public LayerData(AppearanceContentPack.Type type, AppearanceModel model, bool isVanilla = false)
        {
            AppearanceType = type;
            AppearanceModel = model;
            IsVanilla = isVanilla;
        }
    }
}
