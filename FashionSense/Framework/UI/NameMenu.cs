using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace FashionSense.Framework.UI
{
    public class NameMenu : IClickableMenu
    {
        private ClickableTextureComponent _doneNamingButton;
        private TextBox _textBox;
        private TextBoxEvent e;

        private const int MIN_LENGTH = 1;

        private string _hoverText = "";
        private string _title;
        private string _originalName;
        private bool _isNewNameValid;
        private OutfitsMenu _callbackMenu;

        public NameMenu(string title, OutfitsMenu callbackMenu, string originalName = null) : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, showUpperRightCloseButton: true)
        {
            _callbackMenu = callbackMenu;
            _originalName = originalName;
            _title = title;

            _textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
            _textBox.Width = Game1.uiViewport.Width / 2;
            _textBox.Height = 192;
            _textBox.X = (Game1.uiViewport.Width / 2) - _textBox.Width / 2;
            _textBox.Y = Game1.uiViewport.Height / 2;
            e = TextBoxEnter;
            _textBox.OnEnterPressed += e;
            Game1.keyboardDispatcher.Subscriber = _textBox;
            _textBox.Text = originalName;
            _textBox.Selected = true;

            _doneNamingButton = new ClickableTextureComponent(new Rectangle(_textBox.X + _textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 102,
                rightNeighborID = 103,
                leftNeighborID = 104
            };

            EvaluateName();
        }

        public void EvaluateName()
        {
            if (String.IsNullOrEmpty(_textBox.Text))
            {
                _isNewNameValid = false;
            }
            else if (!String.IsNullOrEmpty(_originalName) && _originalName == _textBox.Text)
            {
                _isNewNameValid = true;
            }
            else
            {
                _isNewNameValid = !FashionSense.outfitManager.DoesOutfitExist(Game1.player, _textBox.Text);
            }
        }

        public void TextBoxEnter(TextBox sender)
        {
            if (!_isNewNameValid)
            {
                return;
            }

            if (sender.Text.Length >= MIN_LENGTH)
            {
                if (_textBox.Selected == true)
                {
                    _textBox.Selected = false;
                }
                else
                {
                    if (String.IsNullOrEmpty(_originalName))
                    {
                        // Create a new outfit
                        FashionSense.outfitManager.CreateOutfit(Game1.player, _textBox.Text);
                    }
                    else
                    {
                        // Change the outfit name
                        FashionSense.outfitManager.RenameOutfit(Game1.player, _originalName, _textBox.Text);
                    }
                    exitThisMenu();

                    // Paginate the obelisks to order them with the new name
                    _callbackMenu.PaginatePacks();
                    Game1.activeClickableMenu = _callbackMenu;
                }
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Enter)
            {
                TextBoxEnter(_textBox);
                Game1.playSound("smallSelect");
            }
            else if (key == Keys.Escape && base.readyToClose())
            {
                Game1.activeClickableMenu = _callbackMenu;
                base.exitThisMenu();
            }
            else if (!_textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                base.receiveKeyPress(key);
            }

            EvaluateName();
        }

        public override void performHoverAction(int x, int y)
        {
            _hoverText = String.Empty;
            if (_isNewNameValid && _doneNamingButton.containsPoint(x, y))
            {
                _doneNamingButton.scale = Math.Min(1.1f, _doneNamingButton.scale + 0.05f);
            }
            else
            {
                if (!_isNewNameValid && _doneNamingButton.containsPoint(x, y))
                {
                    _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.warning.name");
                }

                _doneNamingButton.scale = Math.Max(1f, _doneNamingButton.scale - 0.05f);
            }

            base.performHoverAction(x, y);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            _textBox.Update();

            if (_isNewNameValid && _doneNamingButton.containsPoint(x, y))
            {
                TextBoxEnter(_textBox);
                Game1.playSound("smallSelect");
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, _title, Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 - 128, _title);

            _textBox.Draw(b);
            _doneNamingButton.draw(b, _isNewNameValid ? Color.White : Color.Gray, 0f);

            // Draw hover text
            if (!_hoverText.Equals(""))
            {
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                IClickableMenu.drawHoverText(b, _hoverText, Game1.smallFont);
            }

            base.drawMouse(b);
        }
    }
}