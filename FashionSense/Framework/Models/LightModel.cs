using FashionSense.Framework.Models.Generic;
using FashionSense.Framework.Models.Generic.Random;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Models
{
    public class LightModel
    {
        public int[] Color { get; set; }
        public object Radius { get; set; }
        public Position Position { get; set; }

        private float ParsedRadius { get; set; }

        internal float GetRadius(bool recalculateValue = false)
        {
            if (ParsedRadius is 0F || recalculateValue)
            {
                if (Radius is JObject modelContext)
                {
                    if (modelContext["RandomRange"] != null)
                    {
                        var randomRange = JsonConvert.DeserializeObject<RandomRange>(modelContext["RandomRange"].ToString());

                        ParsedRadius = Game1.random.Next(randomRange.Min, randomRange.Max);
                    }
                    else if (modelContext["RandomValue"] != null)
                    {
                        var randomValue = JsonConvert.DeserializeObject<List<float>>(modelContext["RandomValue"].ToString());
                        ParsedRadius = randomValue[Game1.random.Next(randomValue.Count)];
                    }
                }
                else
                {
                    ParsedRadius = (float)Radius;
                }
            }

            return ParsedRadius;
        }

        internal Color GetColor()
        {
            return new Color(Color[0], Color[1], Color[2]);
        }
    }
}
