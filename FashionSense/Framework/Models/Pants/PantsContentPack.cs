using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Pants
{
    public class PantsContentPack : AppearanceContentPack
    {
        public PantsModel BackPants { get; set; }
        public PantsModel RightPants { get; set; }
        public PantsModel FrontPants { get; set; }
        public PantsModel LeftPants { get; set; }

        // Make a PantsModel variable for each of the following (including directional variation, where applicable):
        // Eating (1 direction)
        // Drinking (1)
        // Fishing (4)
        // Pickaxe / Axe / Hoe (4)
        // Scythe (4)
        // Slingshot (4)
        // Riding (4)
        // Harvesting (4)
        // Running (4)
        // Walking (4)


        internal PantsModel GetPantsFromFacingDirection(int facingDirection)
        {
            PantsModel PantsModel = null;
            switch (facingDirection)
            {
                case 0:
                    PantsModel = BackPants;
                    break;
                case 1:
                    PantsModel = RightPants;
                    break;
                case 2:
                    PantsModel = FrontPants;
                    break;
                case 3:
                    PantsModel = LeftPants;
                    break;
            }

            return PantsModel;
        }
    }
}
