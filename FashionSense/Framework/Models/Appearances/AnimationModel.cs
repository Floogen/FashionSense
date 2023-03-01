using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Generic.Random;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Models.Appearances
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
        public object Duration { get; set; } = 1000;
        private int? _duration { get; set; }
        public bool EndWhenFarmerFrameUpdates { get; set; }
        internal bool WasDisplayed { get; set; }

        internal int GetDuration(bool recalculateValue = false)
        {
            if (_duration is null || recalculateValue)
            {
                if (Duration is JObject modelContext)
                {
                    if (modelContext["RandomRange"] != null)
                    {
                        var randomRange = JsonConvert.DeserializeObject<RandomRange>(modelContext["RandomRange"].ToString());

                        _duration = Game1.random.Next(randomRange.Min, randomRange.Max);
                    }
                    else if (modelContext["RandomValue"] != null)
                    {
                        var randomValue = JsonConvert.DeserializeObject<List<int>>(modelContext["RandomValue"].ToString());
                        _duration = randomValue[Game1.random.Next(randomValue.Count)];
                    }
                }
                else
                {
                    _duration = Convert.ToInt32(Duration);
                }
            }

            return _duration.Value;
        }

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
