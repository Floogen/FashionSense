using FashionSense.Framework.Models.Appearances;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.General
{
    internal class LayerData
    {
        public AppearanceContentPack.Type AppearanceType { get; set; }
        public AppearanceModel AppearanceModel { get; set; }
        public Color Color { get; set; }
        public bool IsVanilla { get; set; }

        public LayerData(AppearanceContentPack.Type type, AppearanceModel model, bool isVanilla = false)
        {
            AppearanceType = type;
            AppearanceModel = model;
            IsVanilla = isVanilla;
        }
    }
}
