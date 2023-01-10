using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Accessory
{
    internal class AccessoryData
    {
        // Animation data
        internal int Iterator { get; set; }
        internal int StartingIndex { get; set; }
        internal int FrameDuration { get; set; }
        internal int ElapsedDuration { get; set; }
        internal int LightId { get; set; }
        internal int FarmerFrame { get; set; }
        internal AnimationModel.Type AnimationType { get; set; }

        // Other data
        internal string Id { get; set; }
        internal Color Color { get; set; }

        public AccessoryData(string id)
        {
            Id = id;

            Reset();

            Color = Color.White;
        }

        public void Reset()
        {
            Iterator = 0;
            StartingIndex = 0;
            ElapsedDuration = 0;
        }

        public void Reset(AnimationModel.Type animationType, int frameDuration, int farmerFrame)
        {
            Reset();

            AnimationType = animationType;
            FrameDuration = frameDuration;
            FarmerFrame = farmerFrame;
        }
    }
}
