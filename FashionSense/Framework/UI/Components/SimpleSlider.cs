using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.UI.Components
{
    public class SimpleSlider
    {
        public ClickableTextureComponent handle;
        public Rectangle bounds;
        protected float _sliderPosition;
        public int min;
        public int max;
        public Func<float, Color> getDrawColor;
        protected int _displayedValue;

        public SimpleSlider(Rectangle bounds, int min, int max)
        {
            handle = new ClickableTextureComponent(new Rectangle(0, 0, 4, 5), Game1.mouseCursors, new Rectangle(72, 256, 16, 20), 1f)
            {
                upNeighborID = -99998,
                upNeighborImmutable = true,
                downNeighborID = -99998,
                downNeighborImmutable = true,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            };

            this.bounds = bounds;
            this.min = min;
            this.max = max;
        }

        public virtual void ApplyMovementKey(int direction)
        {
            SetValue(_displayedValue + direction);
        }

        public virtual bool ReceiveLeftClick(int x, int y, bool force = false)
        {
            if (bounds.Contains(x, y) || force)
            {
                SetValueFromPosition(x, y);

                return true;
            }

            return false;
        }

        public virtual void SetValueFromPosition(int x, int y)
        {
            if (bounds.Width != 0 && min != max)
            {
                float new_value = x - bounds.Left;
                new_value /= (float)bounds.Width;
                if (new_value < 0f)
                {
                    new_value = 0f;
                }
                if (new_value > 1f)
                {
                    new_value = 1f;
                }
                int steps = max - min;
                new_value /= (float)steps;
                new_value *= (float)steps;

                if (_sliderPosition != new_value)
                {
                    _sliderPosition = new_value;
                    SetValue(min + (int)(_sliderPosition * (float)steps));
                }
            }
        }

        public void SetValue(int value)
        {
            if (value > max)
            {
                value = max;
            }
            if (value < min)
            {
                value = min;
            }
            _sliderPosition = (float)(value - min) / (float)(max - min);
            handle.bounds.X = (int)Utility.Lerp(bounds.Left, bounds.Right, _sliderPosition) - handle.bounds.Width / 2 * 4;
            handle.bounds.Y = bounds.Top - 6;

            if (_displayedValue != value)
            {
                _displayedValue = value;
            }
        }

        public int GetValue()
        {
            return _displayedValue;
        }

        public virtual void Draw(SpriteBatch b)
        {
            int divisions = 20;
            for (int i = 0; i < divisions; i++)
            {
                Rectangle section_bounds = new Rectangle((int)((float)bounds.X + (float)bounds.Width / (float)divisions * (float)i), bounds.Y, (int)Math.Ceiling((float)bounds.Width / (float)divisions), 5);
                Color drawn_color = Color.Black;
                if (getDrawColor != null)
                {
                    drawn_color = getDrawColor(Utility.Lerp(min, max, (float)i / (float)divisions));
                }
                b.Draw(Game1.staminaRect, section_bounds, drawn_color);
            }
            handle.draw(b);

            Utility.drawTextWithShadow(b, GetValue().ToString() ?? "", Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y - 12), Game1.textColor);
        }

        public virtual void Update(int x, int y)
        {
            SetValueFromPosition(x, y);
        }
    }
}
