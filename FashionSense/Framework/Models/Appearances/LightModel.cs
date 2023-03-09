using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Generic.Random;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System.Collections.Generic;

namespace FashionSense.Framework.Models.Appearances
{
    public class LightModel
    {
        public int TextureId { get; set; } = 4;
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

        internal int GetTextureSource()
        {
            int source = TextureId;
            if (TextureId == 3)
            {
                source = 2;
            }
            else if (TextureId < 1 || TextureId > 8)
            {
                source = 1;
            }

            return source;
        }

        internal Color GetColor()
        {
            int alpha = 255;
            if (3 < Color.Length)
            {
                alpha = Color[3];
            }
            return new Color(byte.MaxValue - Color[0], byte.MaxValue - Color[1], byte.MaxValue - Color[2], alpha);
        }
    }
}
