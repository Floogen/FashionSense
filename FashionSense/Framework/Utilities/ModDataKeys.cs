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
        internal const string CUSTOM_ACCESSORY_ID = "FashionSense.CustomAccessory.Id";

        // Tool related keys
        internal const string HAND_MIRROR_FLAG = "FashionSense.Tools.HandMirror";

        // General animation related keys
        internal const string ANIMATION_FACING_DIRECTION = "FashionSense.Animation.FacingDirection";

        // Hair animation related keys
        internal const string ANIMATION_HAIR_TYPE = "FashionSense.Animation.Hair.Type";
        internal const string ANIMATION_HAIR_ITERATOR = "FashionSense.Animation.Hair.Iterator";
        internal const string ANIMATION_HAIR_STARTING_INDEX = "FashionSense.Animation.Hair.StartingIndex";
        internal const string ANIMATION_HAIR_FRAME_DURATION = "FashionSense.Animation.Hair.FrameDuration";
        internal const string ANIMATION_HAIR_ELAPSED_DURATION = "FashionSense.Animation.Hair.ElapsedDuration";

        // Accessory animation related keys
        internal const string ANIMATION_ACCESSORY_TYPE = "FashionSense.Animation.Accessory.Type";
        internal const string ANIMATION_ACCESSORY_ITERATOR = "FashionSense.Animation.Accessory.Iterator";
        internal const string ANIMATION_ACCESSORY_STARTING_INDEX = "FashionSense.Animation.Accessory.StartingIndex";
        internal const string ANIMATION_ACCESSORY_FRAME_DURATION = "FashionSense.Animation.Accessory.FrameDuration";
        internal const string ANIMATION_ACCESSORY_ELAPSED_DURATION = "FashionSense.Animation.Accessory.ElapsedDuration";
    }
}
