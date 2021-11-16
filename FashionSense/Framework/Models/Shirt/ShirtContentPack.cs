using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Shirt
{
    public class ShirtContentPack : AppearanceContentPack
    {
        public ShirtModel BackShirt { get; set; }
        public ShirtModel RightShirt { get; set; }
        public ShirtModel FrontShirt { get; set; }
        public ShirtModel LeftShirt { get; set; }

        internal ShirtModel GetShirtFromFacingDirection(int facingDirection)
        {
            ShirtModel ShirtModel = null;
            switch (facingDirection)
            {
                case 0:
                    ShirtModel = BackShirt;
                    break;
                case 1:
                    ShirtModel = RightShirt;
                    break;
                case 2:
                    ShirtModel = FrontShirt;
                    break;
                case 3:
                    ShirtModel = LeftShirt;
                    break;
            }

            return ShirtModel;
        }
    }
}
