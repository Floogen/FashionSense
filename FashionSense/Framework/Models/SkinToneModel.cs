using FashionSense.Framework.Models.Generic;
using FashionSense.Framework.Models.Generic.Random;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class SkinToneModel
    {
        public Color Lightest { get; set; }
        public Color Medium { get; set; }
        public Color Darkest { get; set; }
    }
}
