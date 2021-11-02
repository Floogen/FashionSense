using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Accessory
{
    public class AccessoryContentPack : AppearanceContentPack
    {
        public AccessoryModel BackAccessory { get; set; }
        public AccessoryModel RightAccessory { get; set; }
        public AccessoryModel FrontAccessory { get; set; }
        public AccessoryModel LeftAccessory { get; set; }

        internal AccessoryModel GetAccessoryFromFacingDirection(int facingDirection)
        {
            AccessoryModel AccessoryModel = null;
            switch (facingDirection)
            {
                case 0:
                    AccessoryModel = BackAccessory;
                    break;
                case 1:
                    AccessoryModel = RightAccessory;
                    break;
                case 2:
                    AccessoryModel = FrontAccessory;
                    break;
                case 3:
                    AccessoryModel = LeftAccessory;
                    break;
            }

            return AccessoryModel;
        }
    }
}
