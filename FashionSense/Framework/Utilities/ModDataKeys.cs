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
        internal const string CUSTOM_ACCESSORY_SECONDARY_ID = "FashionSense.CustomAccessory.Secondary.Id";
        internal const string CUSTOM_ACCESSORY_TERTIARY_ID = "FashionSense.CustomAccessory.Tertiary.Id";
        internal const string CUSTOM_HAT_ID = "FashionSense.CustomHat.Id";
        internal const string CUSTOM_SHIRT_ID = "FashionSense.CustomShirt.Id";
        internal const string CUSTOM_PANTS_ID = "FashionSense.CustomPants.Id";

        // UI related keys
        internal const string UI_HAND_MIRROR_FILTER_BUTTON = "FashionSense.UI.HandMirror.SelectedFilterButton";
        internal const string UI_HAND_MIRROR_ACCESSORY_COLOR = "FashionSense.UI.HandMirror.Color.Accessory";
        internal const string UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR = "FashionSense.UI.HandMirror.Color.Accessory.Secondary";
        internal const string UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR = "FashionSense.UI.HandMirror.Color.Accessory.Tertiary";
        internal const string UI_HAND_MIRROR_HAT_COLOR = "FashionSense.UI.HandMirror.Color.Hat";
        internal const string UI_HAND_MIRROR_SHIRT_COLOR = "FashionSense.UI.HandMirror.Color.Shirt";
        internal const string UI_HAND_MIRROR_PANTS_COLOR = "FashionSense.UI.HandMirror.Color.Pants";

        // Tool related keys
        internal const string HAND_MIRROR_FLAG = "FashionSense.Tools.HandMirror";

        // General animation related keys
        internal const string ANIMATION_FACING_DIRECTION = "FashionSense.Animation.FacingDirection";
        internal const string ANIMATION_FARMER_FRAME = "FashionSense.Animation.FarmerFrame";

        // Hair animation related keys
        internal const string ANIMATION_HAIR_TYPE = "FashionSense.Animation.Hair.Type";
        internal const string ANIMATION_HAIR_ITERATOR = "FashionSense.Animation.Hair.Iterator";
        internal const string ANIMATION_HAIR_STARTING_INDEX = "FashionSense.Animation.Hair.StartingIndex";
        internal const string ANIMATION_HAIR_FRAME_DURATION = "FashionSense.Animation.Hair.FrameDuration";
        internal const string ANIMATION_HAIR_ELAPSED_DURATION = "FashionSense.Animation.Hair.ElapsedDuration";
        internal const string ANIMATION_HAIR_LIGHT_ID = "FashionSense.Animation.Hair.Light.Id";

        // Accessory animation related keys
        internal const string ANIMATION_ACCESSORY_TYPE = "FashionSense.Animation.Accessory.Type";
        internal const string ANIMATION_ACCESSORY_ITERATOR = "FashionSense.Animation.Accessory.Iterator";
        internal const string ANIMATION_ACCESSORY_STARTING_INDEX = "FashionSense.Animation.Accessory.StartingIndex";
        internal const string ANIMATION_ACCESSORY_FRAME_DURATION = "FashionSense.Animation.Accessory.FrameDuration";
        internal const string ANIMATION_ACCESSORY_ELAPSED_DURATION = "FashionSense.Animation.Accessory.ElapsedDuration";
        internal const string ANIMATION_ACCESSORY_LIGHT_ID = "FashionSense.Animation.Accessory.Light.Id";

        // Accessory animation related keys
        internal const string ANIMATION_ACCESSORY_SECONDARY_TYPE = "FashionSense.Animation.Accessory.Secondary.Type";
        internal const string ANIMATION_ACCESSORY_SECONDARY_ITERATOR = "FashionSense.Animation.Accessory.Secondary.Iterator";
        internal const string ANIMATION_ACCESSORY_SECONDARY_STARTING_INDEX = "FashionSense.Animation.Accessory.Secondary.StartingIndex";
        internal const string ANIMATION_ACCESSORY_SECONDARY_FRAME_DURATION = "FashionSense.Animation.Accessory.Secondary.FrameDuration";
        internal const string ANIMATION_ACCESSORY_SECONDARY_ELAPSED_DURATION = "FashionSense.Animation.Accessory.Secondary.ElapsedDuration";
        internal const string ANIMATION_ACCESSORY_SECONDARY_LIGHT_ID = "FashionSense.Animation.Accessory.Secondary.Light.Id";

        // Accessory animation related keys
        internal const string ANIMATION_ACCESSORY_TERTIARY_TYPE = "FashionSense.Animation.Accessory.Tertiary.Type";
        internal const string ANIMATION_ACCESSORY_TERTIARY_ITERATOR = "FashionSense.Animation.Accessory.Tertiary.Iterator";
        internal const string ANIMATION_ACCESSORY_TERTIARY_STARTING_INDEX = "FashionSense.Animation.Accessory.Tertiary.StartingIndex";
        internal const string ANIMATION_ACCESSORY_TERTIARY_FRAME_DURATION = "FashionSense.Animation.Accessory.Tertiary.FrameDuration";
        internal const string ANIMATION_ACCESSORY_TERTIARY_ELAPSED_DURATION = "FashionSense.Animation.Accessory.Tertiary.ElapsedDuration";
        internal const string ANIMATION_ACCESSORY_TERTIARY_LIGHT_ID = "FashionSense.Animation.Accessory.Tertiary.Light.Id";

        // Hat animation related keys
        internal const string ANIMATION_HAT_TYPE = "FashionSense.Animation.Hat.Type";
        internal const string ANIMATION_HAT_ITERATOR = "FashionSense.Animation.Hat.Iterator";
        internal const string ANIMATION_HAT_STARTING_INDEX = "FashionSense.Animation.Hat.StartingIndex";
        internal const string ANIMATION_HAT_FRAME_DURATION = "FashionSense.Animation.Hat.FrameDuration";
        internal const string ANIMATION_HAT_ELAPSED_DURATION = "FashionSense.Animation.Hat.ElapsedDuration";
        internal const string ANIMATION_HAT_LIGHT_ID = "FashionSense.Animation.Hat.Light.Id";

        // Shirt animation related keys
        internal const string ANIMATION_SHIRT_TYPE = "FashionSense.Animation.Shirt.Type";
        internal const string ANIMATION_SHIRT_ITERATOR = "FashionSense.Animation.Shirt.Iterator";
        internal const string ANIMATION_SHIRT_STARTING_INDEX = "FashionSense.Animation.Shirt.StartingIndex";
        internal const string ANIMATION_SHIRT_FRAME_DURATION = "FashionSense.Animation.Shirt.FrameDuration";
        internal const string ANIMATION_SHIRT_ELAPSED_DURATION = "FashionSense.Animation.Shirt.ElapsedDuration";
        internal const string ANIMATION_SHIRT_LIGHT_ID = "FashionSense.Animation.Shirt.Light.Id";

        // Pants animation related keys
        internal const string ANIMATION_PANTS_TYPE = "FashionSense.Animation.Pants.Type";
        internal const string ANIMATION_PANTS_ITERATOR = "FashionSense.Animation.Pants.Iterator";
        internal const string ANIMATION_PANTS_STARTING_INDEX = "FashionSense.Animation.Pants.StartingIndex";
        internal const string ANIMATION_PANTS_FRAME_DURATION = "FashionSense.Animation.Pants.FrameDuration";
        internal const string ANIMATION_PANTS_ELAPSED_DURATION = "FashionSense.Animation.Pants.ElapsedDuration";
        internal const string ANIMATION_PANTS_LIGHT_ID = "FashionSense.Animation.Pants.Light.Id";
    }
}
