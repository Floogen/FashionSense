using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class AppearanceContentPack
    {
        internal string Owner { get; set; }
        internal string Author { get; set; }
        public string Name { get; set; }
        internal string Id { get; set; }
        internal Texture2D Texture { get; set; }
    }
}
