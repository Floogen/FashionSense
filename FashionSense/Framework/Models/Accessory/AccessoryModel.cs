using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models.Accessory
{
    public class AccessoryModel : AppearanceModel
    {
        internal enum Type
        {
            Unknown,
            Primary,
            Secondary,
            Tertiary
        }

        public Position HeadPosition { get; set; } = new Position() { X = 0, Y = 0 };
        public Size AccessorySize { get; set; }
        public bool DrawBeforeHair { get; set; }
        public bool DrawAfterPlayer { get; set; }
        public bool DrawAfterSleeves { get; set; }

        public override bool HideWhileSwimming { get; set; } = false;
        public override bool HideWhileWearingBathingSuit { get; set; } = false;

        // Old property, has been renamed to DrawAfterPlayer
        [Obsolete("Has been renamed to DrawAfterPlayer.")]
        public bool DrawBeforePlayer { set { DrawAfterPlayer = value; } }

        internal Type Priority { get; set; }
    }
}
