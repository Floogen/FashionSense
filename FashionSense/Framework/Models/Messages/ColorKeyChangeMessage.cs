using Microsoft.Xna.Framework;

namespace FashionSense.Framework.Models.Messages
{
    public class ColorKeyChangeMessage
    {
        public long FarmerID { get; set; }
        public string ColorKey { get; set; }
        public Color ColorValue { get; set; }
    }
}
