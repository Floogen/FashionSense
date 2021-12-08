using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class AppearanceModel
    {
        public Position StartingPosition { get; set; }
        public bool Flipped { get; set; }
        public bool RequireAnimationToFinish { get; set; }
        public bool DisableGrayscale { get; set; }
        public bool DisableSkinGrayscale { get; set; }
        public bool IsPrismatic { get; set; }
        public float PrismaticAnimationSpeedMultiplier { get; set; } = 1f;
        public List<int[]> ColorMasks { get; set; } = new List<int[]>();
        public int[] DarkestSkinToneMask { get; set; }
        public int[] MediumSkinToneMask { get; set; }
        public int[] LightestSkinToneMask { get; set; }
        public List<AnimationModel> UniformAnimation { get; set; } = new List<AnimationModel>();
        public List<AnimationModel> IdleAnimation { get; set; } = new List<AnimationModel>();
        public List<AnimationModel> MovementAnimation { get; set; } = new List<AnimationModel>();

        internal bool IsPlayerColorChoiceIgnored()
        {
            return DisableGrayscale || IsPrismatic;
        }

        internal int GetColorIndex(int[] colorArray, int position)
        {
            if (position >= colorArray.Length)
            {
                return 255;
            }

            return colorArray[position];
        }

        internal Color GetColor(int[] colorArray)
        {
            if (3 < colorArray.Length)
            {
                return new Color(GetColorIndex(colorArray, 0), GetColorIndex(colorArray, 1), GetColorIndex(colorArray, 2), GetColorIndex(colorArray, 3));
            }
            else
            {
                return new Color(GetColorIndex(colorArray, 0), GetColorIndex(colorArray, 1), GetColorIndex(colorArray, 2), 255);
            }
        }

        internal bool IsMaskedColor(Color color)
        {
            if (!HasColorMask())
            {
                return false;
            }

            foreach (Color maskedColor in ColorMasks.Select(c => new Color(c[0], c[1], c[2])))
            {
                if (maskedColor == color)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool IsSkinToneMaskColor(Color color)
        {
            if (!HasSkinToneMask())
            {
                return false;
            }

            if (LightestSkinToneMask is not null && color == new Color(GetColorIndex(LightestSkinToneMask, 0), GetColorIndex(LightestSkinToneMask, 1), GetColorIndex(LightestSkinToneMask, 2), GetColorIndex(LightestSkinToneMask, 3)))
            {
                return true;
            }
            else if (MediumSkinToneMask is not null && color == new Color(GetColorIndex(MediumSkinToneMask, 0), GetColorIndex(MediumSkinToneMask, 1), GetColorIndex(MediumSkinToneMask, 2), GetColorIndex(MediumSkinToneMask, 3)))
            {
                return true;
            }
            else if (DarkestSkinToneMask is not null && color == new Color(GetColorIndex(DarkestSkinToneMask, 0), GetColorIndex(DarkestSkinToneMask, 1), GetColorIndex(DarkestSkinToneMask, 2), GetColorIndex(DarkestSkinToneMask, 3)))
            {
                return true;
            }

            return false;
        }

        internal bool HasColorMask()
        {
            return ColorMasks.Count > 0;
        }

        internal bool HasSkinToneMask()
        {
            return DarkestSkinToneMask is not null || MediumSkinToneMask is not null || LightestSkinToneMask is not null;
        }

        internal bool HasUniformAnimation()
        {
            return UniformAnimation.Count > 0;
        }

        internal bool HasIdleAnimation()
        {
            return IdleAnimation.Count > 0;
        }

        internal bool HasMovementAnimation()
        {
            return MovementAnimation.Count > 0;
        }

        private AnimationModel GetAnimationData(List<AnimationModel> animation, int frame)
        {
            return animation.FirstOrDefault(a => a.Frame == frame);
        }

        internal AnimationModel GetUniformAnimationAtFrame(int frame)
        {
            return GetAnimationData(UniformAnimation, frame);
        }

        internal AnimationModel GetIdleAnimationAtFrame(int frame)
        {
            return GetAnimationData(IdleAnimation, frame);
        }

        internal AnimationModel GetMovementAnimationAtFrame(int frame)
        {
            return GetAnimationData(MovementAnimation, frame);
        }
    }
}
