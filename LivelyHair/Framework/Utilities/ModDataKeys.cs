using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Utilities
{
    public static class ModDataKeys
    {
        // Core keys
        internal const string CUSTOM_HAIR_ID = "LivelyHair.CustomHair.Id";

        // Tool related keys
        internal const string HAND_MIRROR_FLAG = "LivelyHair.Tools.HandMirror";

        // Animation related keys
        internal const string ANIMATION_ITERATOR = "LivelyHair.Animation.Iterator";
        internal const string ANIMATION_STARTING_INDEX = "LivelyHair.Animation.StartingIndex";
        internal const string ANIMATION_FRAME_DURATION = "LivelyHair.Animation.FrameDuration";
        internal const string ANIMATION_ELAPSED_DURATION = "LivelyHair.Animation.ElapsedDuration";
        internal const string ANIMATION_TYPE = "LivelyHair.Animation.Type";
        internal const string ANIMATION_FACING_DIRECTION = "LivelyHair.Animation.FacingDirection";
    }
}
