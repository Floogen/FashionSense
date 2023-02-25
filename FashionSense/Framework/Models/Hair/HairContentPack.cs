namespace FashionSense.Framework.Models.Hair
{
    public class HairContentPack : AppearanceContentPack
    {
        public HairModel BackHair { get; set; }
        public HairModel RightHair { get; set; }
        public HairModel FrontHair { get; set; }
        public HairModel LeftHair { get; set; }

        internal HairModel GetHairFromFacingDirection(int facingDirection)
        {
            HairModel hairModel = null;
            switch (facingDirection)
            {
                case 0:
                    hairModel = BackHair;
                    break;
                case 1:
                    hairModel = RightHair;
                    break;
                case 2:
                    hairModel = FrontHair;
                    break;
                case 3:
                    hairModel = LeftHair;
                    break;
            }

            return hairModel;
        }

        internal override void LinkId()
        {
            if (BackHair is AppearanceModel backModel && backModel is not null)
            {
                backModel.Pack = this;
            }
            if (RightHair is AppearanceModel rightModel && rightModel is not null)
            {
                rightModel.Pack = this;
            }
            if (FrontHair is AppearanceModel frontModel && frontModel is not null)
            {
                frontModel.Pack = this;
            }
            if (LeftHair is AppearanceModel leftModel && leftModel is not null)
            {
                leftModel.Pack = this;
            }
        }
    }
}
