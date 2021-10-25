using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Models
{
    public class AppearanceModel
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        internal Texture2D Texture { get; set; }
        public HairModel BackHair { get; set; }
        public HairModel RightHair { get; set; }
        public HairModel FrontHair { get; set; }
        public HairModel LeftHair { get; set; }
    }
}
