namespace FashionSense.Framework.Models.Appearances.Pants
{
    public class PantsContentPack : AppearanceContentPack
    {
        public PantsModel BackPants { get; set; }
        public PantsModel RightPants { get; set; }
        public PantsModel FrontPants { get; set; }
        public PantsModel LeftPants { get; set; }

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

        internal override void LinkId()
        {
            if (BackPants is AppearanceModel backModel && backModel is not null)
            {
                backModel.Pack = this;
            }
            if (RightPants is AppearanceModel rightModel && rightModel is not null)
            {
                rightModel.Pack = this;
            }
            if (FrontPants is AppearanceModel frontModel && frontModel is not null)
            {
                frontModel.Pack = this;
            }
            if (LeftPants is AppearanceModel leftModel && leftModel is not null)
            {
                leftModel.Pack = this;
            }
        }
    }
}
