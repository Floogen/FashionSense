using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Models.Hat;
using FashionSense.Framework.Models.Pants;
using FashionSense.Framework.Models.Shirt;
using FashionSense.Framework.Models.Sleeves;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.UI
{
    public class FilterMenu : IClickableMenu
    {
        private int _currentPage;
        private string _hoverText = "";
        private string _appearanceFilter;
        private const int PACK_PER_PAGE = 6;

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        public List<ClickableComponent> packButtons = new List<ClickableComponent>();

        private IClickableMenu _callbackMenu;
        private List<List<AppearanceContentPack>> _pages;

        public FilterMenu(string appearanceFilter, IClickableMenu callbackMenu) : base(0, 0, 700, 550, showUpperRightCloseButton: true)
        {
            _callbackMenu = callbackMenu;
            _appearanceFilter = appearanceFilter;

            // Set up menu structure
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                base.height += 64;
            }

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            Game1.playSound("bigSelect");
            PaginatePacks();

            // Establish the buttons that will be used to select the content pack
            for (int i = 0; i < PACK_PER_PAGE; i++)
            {
                ClickableComponent packButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 32) / 6), base.width - 32, (base.height - 32) / 6 + 4), string.Concat(i))
                {
                    myID = i,
                    downNeighborID = -7777,
                    upNeighborID = ((i > 0) ? (i - 1) : (-1)),
                    rightNeighborID = i + 103,
                    leftNeighborID = -7777,
                    fullyImmutable = true
                };
                packButtons.Add(packButton);
            }

            // Set up the various other buttons
            backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = 102,
                rightNeighborID = -7777
            };
            forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 64 - 48, base.yPositionOnScreen + base.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = 101
            };
        }

        public void PaginatePacks()
        {
            var appearancePacks = FashionSense.textureManager.GetAllAppearanceModels();
            switch (_appearanceFilter)
            {
                case HandMirrorMenu.HAIR_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HairContentPack).ToList();
                    break;
                case HandMirrorMenu.ACCESSORY_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is AccessoryContentPack).ToList();
                    break;
                case HandMirrorMenu.HAT_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HatContentPack).ToList();
                    break;
                case HandMirrorMenu.SHIRT_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShirtContentPack).ToList();
                    break;
                case HandMirrorMenu.PANTS_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is PantsContentPack).ToList();
                    break;
                case HandMirrorMenu.SLEEVES_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is SleevesContentPack).ToList();
                    break;
            }
            _pages = new List<List<AppearanceContentPack>>();

            int count = appearancePacks.Count - 1;
            foreach (var contentPack in appearancePacks.GroupBy(p => p.Owner).Select(g => g.First()).OrderBy(p => p.PackName))
            {
                int which = appearancePacks.Count - 1 - count;
                while (_pages.Count <= which / PACK_PER_PAGE)
                {
                    _pages.Add(new List<AppearanceContentPack>());
                }
                _pages[which / PACK_PER_PAGE].Add(contentPack);

                count--;
            }

            if (_pages.Count == 0)
            {
                _pages.Add(new List<AppearanceContentPack>());
            }
            _currentPage = Math.Min(Math.Max(_currentPage, 0), _pages.Count - 1);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && _currentPage > 0)
            {
                _currentPage--;
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && _currentPage < _pages.Count - 1)
            {
                _currentPage++;
                Game1.playSound("shiny4");
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key != 0)
            {
                if (key == Keys.Escape && base.readyToClose())
                {
                    Game1.activeClickableMenu = _callbackMenu;
                    base.exitThisMenu();
                }
                else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !base.overrideSnappyMenuCursorMovementBan())
                {
                    this.applyMovementKey(key);
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            for (int i = 0; i < packButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                if (packButtons[i].containsPoint(x, y))
                {
                    base.exitThisMenu();
                    (_callbackMenu as HandMirrorMenu).SetFilter(_appearanceFilter, _pages[_currentPage][i]);
                    Game1.activeClickableMenu = _callbackMenu;
                    return;
                }
            }

            if (_currentPage < _pages.Count - 1 && forwardButton.containsPoint(x, y))
            {
                _currentPage++;
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus && _currentPage == _pages.Count - 1)
                {
                    base.currentlySnappedComponent = base.getComponentWithID(0);
                    snapCursorToCurrentSnappedComponent();
                }
                return;
            }
            if (_currentPage > 0 && backButton.containsPoint(x, y))
            {
                _currentPage--;
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus && _currentPage == 0)
                {
                    base.currentlySnappedComponent = base.getComponentWithID(0);
                    snapCursorToCurrentSnappedComponent();
                }
                return;
            }
        }

        public override void performHoverAction(int x, int y)
        {
            _hoverText = String.Empty;
            for (int i = 0; i < packButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                if (packButtons[i].containsPoint(x, y))
                {
                    _hoverText = $"{ _pages[_currentPage][i].PackName} by { _pages[_currentPage][i].Author}";
                    return;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }

            // General UI (title, background)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.search"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Draw the content pack buttons
            for (int j = 0; j < packButtons.Count; j++)
            {
                if (_pages.Count() > 0 && _pages[_currentPage].Count() > j)
                {
                    var packName = _pages[_currentPage][j].PackName;
                    if (packName.Length > 23)
                    {
                        packName = $"{packName.Substring(0, 23).TrimEnd()}...";
                    }

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), packButtons[j].bounds.X, packButtons[j].bounds.Y, packButtons[j].bounds.Width, packButtons[j].bounds.Height, packButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
                    SpriteText.drawStringHorizontallyCenteredAt(b, packName, packButtons[j].bounds.X + packButtons[j].bounds.Width / 2, packButtons[j].bounds.Y + 20);
                }
            }

            if (_currentPage < _pages.Count - 1)
            {
                this.forwardButton.draw(b);
            }
            if (_currentPage > 0)
            {
                this.backButton.draw(b);
            }

            // Draw hover text
            if (!_hoverText.Equals(""))
            {
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                IClickableMenu.drawHoverText(b, _hoverText, Game1.smallFont);
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}
