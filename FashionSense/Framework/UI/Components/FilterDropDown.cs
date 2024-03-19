using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace FashionSense.Framework.UI.Components
{
    internal class FilterDropDown : OptionsDropDown
    {
        public bool IsClicked { get; set; }

        public FilterDropDown(string label, int whichOption, int x = -1, int y = -1) : base(label, whichOption, x, y)
        {

        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);

            IsClicked = true;
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
        }

        public override void leftClickReleased(int x, int y)
        {
            if (!base.greyedOut && base.dropDownOptions.Count > 0)
            {
                base.leftClickReleased(x, y);

                if (base.dropDownBounds.Contains(x, y) || (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse))
                {
                    _ = base.selectedOption;
                }
                else
                {
                    base.selectedOption = base.startingSelected;
                }
                OptionsDropDown.selected = null;
            }

            IsClicked = false;
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            base.draw(b, slotX, slotY, context);
        }
    }
}
