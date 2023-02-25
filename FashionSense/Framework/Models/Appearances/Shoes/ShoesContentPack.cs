namespace FashionSense.Framework.Models.Appearances.Shoes
{
    public class ShoesContentPack : AppearanceContentPack
    {
        public ShoesModel BackShoes { get; set; }
        public ShoesModel RightShoes { get; set; }
        public ShoesModel FrontShoes { get; set; }
        public ShoesModel LeftShoes { get; set; }

        internal ShoesModel GetShoesFromFacingDirection(int facingDirection)
        {
            ShoesModel ShoesModel = null;
            switch (facingDirection)
            {
                case 0:
                    ShoesModel = BackShoes;
                    break;
                case 1:
                    ShoesModel = RightShoes;
                    break;
                case 2:
                    ShoesModel = FrontShoes;
                    break;
                case 3:
                    ShoesModel = LeftShoes;
                    break;
            }

            return ShoesModel;
        }

        internal override void LinkId()
        {
            if (BackShoes is AppearanceModel backModel && backModel is not null)
            {
                backModel.Pack = this;
            }
            if (RightShoes is AppearanceModel rightModel && rightModel is not null)
            {
                rightModel.Pack = this;
            }
            if (FrontShoes is AppearanceModel frontModel && frontModel is not null)
            {
                frontModel.Pack = this;
            }
            if (LeftShoes is AppearanceModel leftModel && leftModel is not null)
            {
                leftModel.Pack = this;
            }
        }
    }
}
