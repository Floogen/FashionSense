using FashionSense.Framework.Interfaces.API;

namespace FashionSense.Framework.Models.Appearances.Generic
{
    public class DrawOrder
    {
        public enum Order
        {
            Unknown,
            Before,
            After
        }

        public Order Preposition { get; set; }
        public IApi.Type AppearanceType { get; set; }

        public bool IsValid()
        {
            return Preposition is not Order.Unknown && AppearanceType is not IApi.Type.Unknown;
        }
    }
}
