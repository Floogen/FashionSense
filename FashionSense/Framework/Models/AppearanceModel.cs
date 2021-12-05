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
        public bool IsPrismatic { get; set; }
        public float PrismaticAnimationSpeedMultiplier { get; set; } = 1f;
        public List<int[]> ColorMasks { get; set; } = new List<int[]>();
        public List<AnimationModel> UniformAnimation { get; set; } = new List<AnimationModel>();
        public List<AnimationModel> IdleAnimation { get; set; } = new List<AnimationModel>();
        public List<AnimationModel> MovementAnimation { get; set; } = new List<AnimationModel>();

        internal bool IsPlayerColorChoiceIgnored()
        {
            return DisableGrayscale || IsPrismatic;
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

        internal bool HasColorMask()
        {
            return ColorMasks.Count > 0;
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
