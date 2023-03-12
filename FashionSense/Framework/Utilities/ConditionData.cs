using FashionSense.Framework.Models.Appearances.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Linq;

namespace FashionSense.Framework.Utilities
{
    class ConditionData
    {
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
            if (elapsedMilliseconds > condition.GetCache<float>() + condition.GetParsedValue<long>(!probe) || condition.GetCache<float>() > elapsedMilliseconds)
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

        internal double GetMovementDuration(Farmer who)
        {
            if (who.modData.ContainsKey(ModDataKeys.MOVEMENT_DURATION_MILLISECONDS) is false)
            {
                who.modData[ModDataKeys.MOVEMENT_DURATION_MILLISECONDS] = "0";
            }

            if (double.TryParse(who.modData[ModDataKeys.MOVEMENT_DURATION_MILLISECONDS], out var movementDurationMilliseconds))
            {
                return movementDurationMilliseconds;
            }

            return 0;
        }

        internal double GetElapsedMilliseconds(Farmer who)
        {
            if (who.modData.ContainsKey(ModDataKeys.ELAPSED_MILLISECONDS) is false)
            {
                who.modData[ModDataKeys.ELAPSED_MILLISECONDS] = "0";
            }

            if (double.TryParse(who.modData[ModDataKeys.ELAPSED_MILLISECONDS], out var elapsedMilliseconds))
            {
                return elapsedMilliseconds;
            }

            return 0;
        }

        internal void Update(Farmer who, GameTime time)
        {
            var elapsedMilliseconds = GetElapsedMilliseconds(who);
            if (elapsedMilliseconds > FashionSense.MAX_TRACKED_MILLISECONDS)
            {
                who.modData[ModDataKeys.ELAPSED_MILLISECONDS] = "0";
            }
            who.modData[ModDataKeys.ELAPSED_MILLISECONDS] = (elapsedMilliseconds + time.ElapsedGameTime.TotalMilliseconds).ToString();

            who.modData[ModDataKeys.MOVEMENT_DURATION_MILLISECONDS] = (GetMovementDuration(who) + (float)time.ElapsedGameTime.TotalMilliseconds).ToString();
            if (GetMovementSpeed(who) == 0)
            {
                who.modData[ModDataKeys.MOVEMENT_DURATION_MILLISECONDS] = "0";
            }
        }

        internal void OnRendered(object sender, RenderedEventArgs e)
        {
            Utility.drawTextWithColoredShadow(e.SpriteBatch, $"Movement Speed: {GetMovementSpeed(Game1.player)}\nDuration: {GetMovementDuration(Game1.player)}", Game1.smallFont, new Vector2(10, 10), Color.LawnGreen, Color.Black, 1);
        }
    }
}
