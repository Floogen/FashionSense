using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Shirt
{
    public class ShirtModel : AppearanceModel
    {
        public Position BodyPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size ShirtSize { get; set; }
        public List<int[]> SleeveColors { get; set; }

        internal Color GetSleeveColor(int layer = 0)
        {
            Color sleeveColor = Color.White;
            if (SleeveColors.Count > layer)
            {
                sleeveColor = new Color(GetColorIndex(SleeveColors[layer], 0), GetColorIndex(SleeveColors[layer], 1), GetColorIndex(SleeveColors[layer], 2), GetColorIndex(SleeveColors[layer], 3));
            }
            else if (layer > 0)
            {
                return GetSleeveColor(layer - 1);
            }

            return sleeveColor;
        }

        internal bool HasSleeveColorAtLayer(int layer = 0)
        {
            if (SleeveColors is null)
            {
                return false;
            }

            return SleeveColors.Count > layer;
        }
    }
}
