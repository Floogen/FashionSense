using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FashionSense.Framework.Models.Generic.Random;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;

namespace FashionSense.Framework.Models.Generic
{
    public class Condition
    {
        public enum Type
        {
            Unknown,
            MovementSpeed,
            MovementDuration,
            IsElapsedTimeMultipleOf,
            DidPreviousFrameDisplay,
            RidingHorse
        }

        public Type Name { get; set; }
        public object Value { get; set; }
        private object Cache { get; set; }
        public bool Independent { get; set; }

        internal T GetParsedValue<T>()
        {
            if (Value is JObject modelContext)
            {
                if (modelContext["RandomRange"] != null)
                {
                    var randomRange = JsonConvert.DeserializeObject<RandomRange>(modelContext["RandomRange"].ToString());

                    return (T)Convert.ChangeType(Game1.random.Next(randomRange.Min, randomRange.Max), typeof(T));
                }

                return (T)Value;
            }

            return (T)Value;
        }

        internal T GetCache<T>()
        {
            if (Cache is null)
            {
                return default;
            }

            return (T)Cache;
        }

        internal void SetCache(object value)
        {
            Cache = value;
        }
    }
}