using FashionSense.Framework.Models.Appearances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.General
{
    internal class DrawData
    {
        public AppearanceContentPack.Type AppearanceType { get; set; }
        public AppearanceModel AppearanceModel { get; set; }
        public bool IsVanilla { get; set; }

        public DrawData(AppearanceContentPack.Type type, AppearanceModel model, bool isVanilla = false)
        {
            AppearanceType = type;
            AppearanceModel = model;
            IsVanilla = isVanilla;
        }
    }
}
