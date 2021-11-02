using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Hair
{
    public class HairModel
    {
        public Position StartingPosition { get; set; }
        public Position HeadPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size HairSize { get; set; }
        public bool Flipped { get; set; }
        public bool RequireAnimationToFinish { get; set; }
        public bool DisableGrayscale { get; set; }
        public List<AnimationModel> IdleAnimation { get; set; } = new List<AnimationModel>();
        public List<AnimationModel> MovementAnimation { get; set; } = new List<AnimationModel>();

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
