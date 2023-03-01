namespace FashionSense.Framework.Models.Appearances.Hat
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

        internal override void LinkId()
        {
            if (BackHat is AppearanceModel backModel && backModel is not null)
            {
                backModel.Pack = this;
            }
            if (RightHat is AppearanceModel rightModel && rightModel is not null)
            {
                rightModel.Pack = this;
            }
            if (FrontHat is AppearanceModel frontModel && frontModel is not null)
            {
                frontModel.Pack = this;
            }
            if (LeftHat is AppearanceModel leftModel && leftModel is not null)
            {
                leftModel.Pack = this;
            }
        }
    }
}
