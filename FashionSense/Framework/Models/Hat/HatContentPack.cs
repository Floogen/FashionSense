using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Hat
{
    public class HatContentPack : AppearanceContentPack
    {
        public HatModel BackHat { get; set; }
        public HatModel RightHat { get; set; }
        public HatModel FrontHat { get; set; }
        public HatModel LeftHat { get; set; }

        internal HatModel GetHatFromFacingDirection(int facingDirection)
        {
            HatModel HatModel = null;
            switch (facingDirection)
            {
                case 0:
                    HatModel = BackHat;
                    break;
                case 1:
                    HatModel = RightHat;
                    break;
                case 2:
                    HatModel = FrontHat;
                    break;
                case 3:
                    HatModel = LeftHat;
                    break;
            }

            return HatModel;
        }
    }
}
