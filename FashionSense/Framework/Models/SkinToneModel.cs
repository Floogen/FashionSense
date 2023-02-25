using Microsoft.Xna.Framework;

namespace FashionSense.Framework.Models
{
    public class SkinToneModel
    {
        public int[] LightTone { get; set; }
        public int[] MediumTone { get; set; }
        public int[] DarkTone { get; set; }

        internal Color Lightest => AppearanceModel.GetColor(LightTone);
        internal Color Medium => AppearanceModel.GetColor(MediumTone);
        internal Color Darkest => AppearanceModel.GetColor(DarkTone);

        public SkinToneModel(Color lightest, Color medium, Color darkest)
        {
            LightTone = new int[4] { lightest.R, lightest.G, lightest.B, lightest.A };
            MediumTone = new int[4] { medium.R, medium.G, medium.B, medium.A };
            DarkTone = new int[4] { darkest.R, darkest.G, darkest.B, darkest.A };
        }
    }
}
