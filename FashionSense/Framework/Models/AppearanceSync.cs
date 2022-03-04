using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class AppearanceSync
    {
        public AppearanceContentPack.Type TargetAppearanceType { get; set; }
        public AnimationModel.Type AnimationType { get; set; }
    }
}
