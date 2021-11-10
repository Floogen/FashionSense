using FashionSense.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Utilities
{
    class ConditionData
    {
        private float _movementSpeed = 0f;
        private float _movementDurationMilliseconds = 0;
        private float _elapsedMilliseconds = 0;

        internal bool IsMovingFastEnough(float requiredMovementSpeed)
        {
            return _movementSpeed >= requiredMovementSpeed;
        }

        internal bool IsMovingLongEnough(float requiredMovementDuration)
        {
            return _movementDurationMilliseconds >= requiredMovementDuration;
        }

        internal bool IsElapsedTimeMultipleOf(Condition condition, bool probe)
        {
            if (_elapsedMilliseconds > condition.GetCache<float>() + condition.GetParsedValue<long>(!probe) || condition.GetCache<float>() > _elapsedMilliseconds)
            {
                if (!probe)
                {
                    condition.SetCache(_elapsedMilliseconds);
                }

                return true;
            }

            return false;
        }

        internal bool IsPlayerMoving()
        {
            return _movementDurationMilliseconds > 0;
        }

        internal void Update(Farmer who, GameTime time)
        {
            if (_elapsedMilliseconds > FashionSense.MAX_TRACKED_MILLISECONDS)
            {
                _elapsedMilliseconds = 0f;
            }
            _elapsedMilliseconds += (float)time.ElapsedGameTime.TotalMilliseconds;

            _movementSpeed = who.getMovementSpeed();

            _movementDurationMilliseconds += (float)time.ElapsedGameTime.TotalMilliseconds;
            if (!who.isMoving())
            {
                _movementSpeed = 0;
                _movementDurationMilliseconds = 0;
            }
        }

        internal void OnRendered(object sender, RenderedEventArgs e)
        {
            Utility.drawTextWithColoredShadow(e.SpriteBatch, $"Movement Speed: {_movementSpeed}\nDuration: {_movementDurationMilliseconds}", Game1.smallFont, new Vector2(10, 10), Color.LawnGreen, Color.Black, 1);
        }
    }
}
