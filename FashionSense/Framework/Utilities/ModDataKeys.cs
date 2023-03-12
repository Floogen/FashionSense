namespace FashionSense.Framework.Utilities
{
    public static class ModDataKeys
    {
        // Core keys
        internal const string CUSTOM_HAIR_ID = "FashionSense.CustomHair.Id";
        internal const string CUSTOM_ACCESSORY_ID = "FashionSense.CustomAccessory.Id";
        internal const string CUSTOM_ACCESSORY_SECONDARY_ID = "FashionSense.CustomAccessory.Secondary.Id";
        internal const string CUSTOM_ACCESSORY_TERTIARY_ID = "FashionSense.CustomAccessory.Tertiary.Id";
        internal const string CUSTOM_ACCESSORY_COLLECTIVE_ID = "FashionSense.CustomAccessory.Collective.Id";
        internal const string CUSTOM_HAT_ID = "FashionSense.CustomHat.Id";
        internal const string CUSTOM_SHIRT_ID = "FashionSense.CustomShirt.Id";
        internal const string CUSTOM_PANTS_ID = "FashionSense.CustomPants.Id";
        internal const string CUSTOM_SLEEVES_ID = "FashionSense.CustomSleeves.Id";
        internal const string CUSTOM_SHOES_ID = "FashionSense.CustomShoes.Id";

        // UI related keys
        internal const string UI_HAND_MIRROR_FILTER_BUTTON = "FashionSense.UI.HandMirror.SelectedFilterButton";
        internal const string UI_HAND_MIRROR_ACCESSORY_COLOR = "FashionSense.UI.HandMirror.Color.Accessory";
        internal const string UI_HAND_MIRROR_ACCESSORY_SECONDARY_COLOR = "FashionSense.UI.HandMirror.Color.Accessory.Secondary";
        internal const string UI_HAND_MIRROR_ACCESSORY_TERTIARY_COLOR = "FashionSense.UI.HandMirror.Color.Accessory.Tertiary";
        internal const string UI_HAND_MIRROR_ACCESSORY_COLLECTIVE_COLOR = "FashionSense.UI.HandMirror.Color.Accessory.Collective";
        internal const string UI_HAND_MIRROR_HAT_COLOR = "FashionSense.UI.HandMirror.Color.Hat";
        internal const string UI_HAND_MIRROR_SHIRT_COLOR = "FashionSense.UI.HandMirror.Color.Shirt";
        internal const string UI_HAND_MIRROR_PANTS_COLOR = "FashionSense.UI.HandMirror.Color.Pants";
        internal const string UI_HAND_MIRROR_SLEEVES_COLOR = "FashionSense.UI.HandMirror.Color.Sleeves";
        internal const string UI_HAND_MIRROR_SHOES_COLOR = "FashionSense.UI.HandMirror.Color.Shoes";

        // Tool related keys
        internal const string HAND_MIRROR_FLAG = "FashionSense.Tools.HandMirror";

        // Outfit related keys
        internal const string OUTFITS = "FashionSense.Outfit.Collection";
        internal const string CURRENT_OUTFIT_ID = "FashionSense.Outfit.CurrentId";

        // Etc
        internal const string STARTS_WITH_HAND_MIRROR = "FashionSense.StartsWithHandMirror";
        internal const string INTERNAL_COLOR_OVERRIDE_SHOE_ID = "Override Shoe Color";
        internal const string LETTER_HAND_MIRROR = "FashionSense.Mail.HandMirror";
        internal const string ACTIVE_ACCESSORIES_COUNT = $"FashionSense.Counters.ActiveAccessories";

        // Condition related keys
        internal const string MOVEMENT_DURATION_MILLISECONDS = "FashionSense.ConditionData.MovementDurationMilliseconds";
        internal const string ELAPSED_MILLISECONDS = "FashionSense.ConditionData.ElapsedMilliseconds";

        // General animation related keys
        internal const string ANIMATION_FACING_DIRECTION = "FashionSense.Animation.FacingDirection";
    }
}
