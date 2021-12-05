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
        public enum Comparison
        {
            EqualTo,
            GreaterThan,
            LessThan
        }

        public enum Type
        {
            Unknown,
            MovementSpeed,
            MovementDuration,
            IsElapsedTimeMultipleOf,
            DidPreviousFrameDisplay,
            RidingHorse,
            InventoryItemCount,
            IsInventoryFull,
            IsDarkOut,
            IsRaining,
            IsWalking,
            IsRunning,
            IsEating,
            IsDrinking,
            IsFishing,
            IsUsingHeavyTool,
            IsUsingMilkPail,
            IsUsingShears,
            IsUsingPan,
            IsUsingScythe,
            IsUsingMeleeWeapon,
            IsUsingSlingshot,
            IsHarvesting,
            IsInMines,
            IsOutdoors,
            HealthLevel,
            StaminaLevel,
            IsSitting
        }

        public Type Name { get; set; }
        public object Value { get; set; }
        public Comparison Operator { get; set; } = Comparison.EqualTo;
        public bool Inverse { get; set; }
        public bool Independent { get; set; }

        private object ParsedValue { get; set; }
        private object Cache { get; set; }

        internal bool IsValid(bool booleanValue)
        {
            return IsValid(booleanValue, GetParsedValue<bool>(recalculateValue: false));
        }

        internal bool IsValid(bool booleanValue, bool comparisonValue)
        {
            var passed = (booleanValue == comparisonValue);
            if (Inverse)
            {
                passed = !passed;
            }

            return passed;
        }

        internal bool IsValid(long numericalValue)
        {
            var passed = false;
            var comparisonValue = GetParsedValue<long>(recalculateValue: false);
            switch (Operator)
            {
                case Comparison.EqualTo:
                    passed = (numericalValue == comparisonValue);
                    break;
                case Comparison.GreaterThan:
                    passed = (numericalValue > comparisonValue);
                    break;
                case Comparison.LessThan:
                    passed = (numericalValue < comparisonValue);
                    break;
            }
            if (Inverse)
            {
                passed = !passed;
            }

            return passed;
        }

        internal T GetParsedValue<T>(bool recalculateValue = false)
        {
            if (ParsedValue is null || recalculateValue)
            {
                if (Value is JObject modelContext)
                {
                    if (modelContext["RandomRange"] != null)
                    {
                        var randomRange = JsonConvert.DeserializeObject<RandomRange>(modelContext["RandomRange"].ToString());

                        ParsedValue = (T)Convert.ChangeType(Game1.random.Next(randomRange.Min, randomRange.Max), typeof(T));
                    }
                    else if (modelContext["RandomValue"] != null)
                    {
                        var randomValue = JsonConvert.DeserializeObject<List<object>>(modelContext["RandomValue"].ToString());
                        ParsedValue = (T)Convert.ChangeType(randomValue[Game1.random.Next(randomValue.Count)], typeof(T));
                    }
                }
                else
                {
                    ParsedValue = Value;
                }
            }

            return (T)ParsedValue;
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