using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivelyHair.Framework.Models
{
    public class AnimationModel
    {
        internal enum Type
        {
            Unknown,
            Idle,
            Moving
        }

        public int Frame { get; set; }
        public int RequiredMovementSpeed { get; set; } = -1;
        public int RequiredElapsedMovementMilliseconds { get; set; } = -1;
        public int Duration { get; set; } = 1000;
    }
}
