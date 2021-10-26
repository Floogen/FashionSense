using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Utilities
{
    public static class ModDataKeys
    {
        // Core keys
        internal const string CUSTOM_HAIR_ID = "FashionSense.CustomHair.Id";

        // Tool related keys
        internal const string HAND_MIRROR_FLAG = "FashionSense.Tools.HandMirror";

        // Animation related keys
        internal const string ANIMATION_ITERATOR = "FashionSense.Animation.Iterator";
        internal const string ANIMATION_STARTING_INDEX = "FashionSense.Animation.StartingIndex";
        internal const string ANIMATION_FRAME_DURATION = "FashionSense.Animation.FrameDuration";
        internal const string ANIMATION_ELAPSED_DURATION = "FashionSense.Animation.ElapsedDuration";
        internal const string ANIMATION_TYPE = "FashionSense.Animation.Type";
        internal const string ANIMATION_FACING_DIRECTION = "FashionSense.Animation.FacingDirection";
    }
}
