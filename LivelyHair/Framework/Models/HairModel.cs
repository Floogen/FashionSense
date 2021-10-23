using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Models
{
    public class HairModel
    {
        public HeadModel HeadPosition { get; set; }
        public HairSize HairSize { get; set; }
        public bool Flipped { get; set; }
        public int[] ColorOverride { get; set; } = new int[3];
        public List<AnimationModel> IdleAnimation { get; set; } = new();
        public List<AnimationModel> MovementAnimation { get; set; } = new();

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
