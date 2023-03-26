using FashionSense.Framework.Models.Appearances.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSense.Framework.Utilities
{
    class ConditionData
    {
        private Dictionary<Farmer, float> _farmerToMovementDuration = new Dictionary<Farmer, float>();
        private Dictionary<Farmer, float> _farmerToElapsedMilliseconds = new Dictionary<Farmer, float>();

        internal bool IsMovingFastEnough(Farmer who, long requiredMovementSpeed)
        {
            return GetMovementSpeed(who) >= requiredMovementSpeed;
        }

        internal bool IsMovingLongEnough(Farmer who, long requiredMovementDuration)
        {
            return GetMovementDuration(who) >= requiredMovementDuration;
        }

        internal bool IsElapsedTimeMultipleOf(Farmer who, Condition condition, bool probe)
        {
            var elapsedMilliseconds = GetElapsedMilliseconds(who);
            if (elapsedMilliseconds > condition.GetCache<float>() + condition.GetParsedValue<float>(!probe) || condition.GetCache<float>() > elapsedMilliseconds)
            {
                if (!probe)
                {
                    condition.SetCache(elapsedMilliseconds);
                }

                return true;
            }

            return false;
        }

        internal bool IsPlayerMoving(Farmer who)
        {
            return GetMovementDuration(who) > 0;
        }

        internal bool IsRunning(Farmer who)
        {
            return Math.Abs(GetMovementSpeed(who) - 5f) < Math.Abs(GetMovementSpeed(who) - 2f) && !who.bathingClothes.Value && !who.onBridge.Value;
        }

        internal int GetActualPlayerInventoryCount(Farmer who)
        {
            return who.Items.Where(o => o != null).Count();
        }

        internal long GetMovementSpeed(Farmer who)
        {
            var movementSpeed = (long)who.getMovementSpeed();
            if (!who.isMoving() || who.UsingTool)
            {
                movementSpeed = 0;
            }

            return movementSpeed;
        }

        internal float GetMovementDuration(Farmer who)
        {
            if (_farmerToMovementDuration.ContainsKey(who) is false)
            {
                _farmerToMovementDuration[who] = 0;
            }

            return _farmerToMovementDuration[who];
        }

        internal float GetElapsedMilliseconds(Farmer who)
        {
            if (_farmerToElapsedMilliseconds.ContainsKey(who) is false)
            {
                _farmerToElapsedMilliseconds[who] = 0;
            }

            return _farmerToElapsedMilliseconds[who];
        }

        internal void Update(Farmer who, GameTime time)
        {
            var elapsedMilliseconds = GetElapsedMilliseconds(who);
            if (elapsedMilliseconds > FashionSense.MAX_TRACKED_MILLISECONDS)
            {
                elapsedMilliseconds = 0;
            }
            _farmerToElapsedMilliseconds[who] = (elapsedMilliseconds + (float)time.ElapsedGameTime.TotalMilliseconds);

            _farmerToMovementDuration[who] = (GetMovementDuration(who) + (float)time.ElapsedGameTime.TotalMilliseconds);
            if (GetMovementSpeed(who) == 0)
            {
                _farmerToMovementDuration[who] = 0;
            }
        }

        internal void OnRendered(object sender, RenderedEventArgs e)
        {
            Utility.drawTextWithColoredShadow(e.SpriteBatch, $"Movement Speed: {GetMovementSpeed(Game1.player)}\nDuration: {GetMovementDuration(Game1.player)}", Game1.smallFont, new Vector2(10, 10), Color.LawnGreen, Color.Black, 1);
        }
    }
}
