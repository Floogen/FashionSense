using FashionSense.Framework.Models.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class AnimationModel
    {
        public enum Type
        {
            Unknown,
            Idle,
            Moving,
            Uniform
        }

        public int Frame { get; set; }
        public bool OverrideStartingIndex { get; set; }
        public LightModel Light { get; set; }
        public List<Condition> Conditions { get; set; } = new List<Condition>();
        public int Duration { get; set; } = 1000;
        public bool EndWhenFarmerFrameUpdates { get; set; }
        internal bool WasDisplayed { get; set; }

        internal bool HasCondition(Condition.Type type)
        {
            return Conditions.Any(c => c.Name == type);
        }

        internal Condition GetConditionByType(Condition.Type type)
        {
            return Conditions.FirstOrDefault(c => c.Name == type);
        }

        internal void Reset()
        {
            WasDisplayed = false;
        }
    }
}
