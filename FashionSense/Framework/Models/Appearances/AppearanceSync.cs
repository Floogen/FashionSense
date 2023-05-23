using FashionSense.Framework.Interfaces.API;

namespace FashionSense.Framework.Models.Appearances
{
    public class AppearanceSync
    {
        public IApi.Type TargetAppearanceType { get; set; }
        public AnimationModel.Type AnimationType { get; set; }
    }
}
