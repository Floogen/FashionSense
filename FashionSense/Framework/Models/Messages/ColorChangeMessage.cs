using Microsoft.Xna.Framework;

namespace FashionSense.Framework.Models.Messages
{
    public class ColorChangeMessage
    {
        public long FarmerID { get; }
        public string ColorKey { get; }
        public Color ColorValue { get; }

        public ColorChangeMessage(long farmerID, string colorKey, Color colorValue)
        {
            FarmerID = farmerID;
            ColorKey = colorKey;
            ColorValue = colorValue;
        }
    }
}
