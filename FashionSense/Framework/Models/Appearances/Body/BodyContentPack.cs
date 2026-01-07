using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FashionSense.Framework.Models.Appearances.Body
{
    public class BodyContentPack : AppearanceContentPack
    {
        public bool HideEyes { get; set; }
        internal Texture2D EyesTexture { get; set; }
        internal List<Texture2D> EyesColorMaskTextures { get; set; }
        public BodyModel BackBody { get; set; }
        public BodyModel RightBody { get; set; }
        public BodyModel FrontBody { get; set; }
        public BodyModel LeftBody { get; set; }

        internal BodyModel GetBodyFromFacingDirection(int facingDirection)
        {
            BodyModel BodyModel = null;
            switch (facingDirection)
            {
                case 0:
                    BodyModel = BackBody;
                    break;
                case 1:
                    BodyModel = RightBody;
                    break;
                case 2:
                    BodyModel = FrontBody;
                    break;
                case 3:
                    BodyModel = LeftBody;
                    break;
            }

            return BodyModel;
        }

        internal override void LinkId()
        {
            if (BackBody is AppearanceModel backModel && backModel is not null)
            {
                backModel.Pack = this;
            }
            if (RightBody is AppearanceModel rightModel && rightModel is not null)
            {
                rightModel.Pack = this;
            }
            if (FrontBody is AppearanceModel frontModel && frontModel is not null)
            {
                frontModel.Pack = this;
            }
            if (LeftBody is AppearanceModel leftModel && leftModel is not null)
            {
                leftModel.Pack = this;
            }
        }

        internal override IEnumerable<AppearanceModel> GetAppearanceModels()
        {
            return new List<AppearanceModel>() { FrontBody, LeftBody, RightBody, BackBody };
        }
    }
}
