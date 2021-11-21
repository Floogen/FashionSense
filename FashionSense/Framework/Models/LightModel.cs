using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class LightModel
    {
        public int[] Color { get; set; }
        public float Radius { get; set; }
        public Position Position { get; set; }
        public int PulseSpeed { get; set; }
        public float PulseMinRadius { get; set; }

        internal Color GetColor()
        {
            return new Color(Color[0], Color[1], Color[2]);
        }
    }
}
