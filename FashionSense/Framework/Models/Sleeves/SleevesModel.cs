using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Sleeves
{
    public class SleevesModel : AppearanceModel
    {
        public Position BodyPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size SleevesSize { get; set; }
        public bool DrawBeforeShirt { get; set; }
        public bool DrawBeforeHair { get; set; }
        public bool UseShirtColors { get; set; }
    }
}
