using FashionSense.Framework.Models.Appearances;

namespace FashionSense.Framework.Models.General
{
    internal class AnimationData
    {
        public AnimationModel.Type Type { get; set; }
        public int Iterator { get; set; }
        public int StartingIndex { get; set; }
        public int FrameDuration { get; set; }
        public int ElapsedDuration { get; set; }
        public int? LightId { get; set; }
        public int FarmerFrame { get; set; }

        internal void Reset(int frameDuration, int farmerFrame)
        {
            Iterator = 0;
            StartingIndex = 0;
            FrameDuration = frameDuration;
            ElapsedDuration = 0;
            FarmerFrame = farmerFrame;
        }

        internal void Reset(int frameDuration, int farmerFrame, AnimationModel.Type animationType)
        {
            Type = animationType;

            Reset(frameDuration, farmerFrame);
        }
    }
}
