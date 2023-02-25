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
    public class OutfitsMenu : IClickableMenu
    {
        private int _currentPage;
        private string _hoverText = "";
        private const int OUTFITS_PER_PAGE = 6;
        private const string CREATE_OUTFIT_NAME = "PeacefulEnd.Create.Outfit.Button";

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        public List<ClickableComponent> outfitButtons = new List<ClickableComponent>();
        public List<ClickableTextureComponent> saveButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> renameButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> deleteButtons = new List<ClickableTextureComponent>();

        private HandMirrorMenu _callbackMenu;
        private List<List<Outfit>> _pages;

        public OutfitsMenu(HandMirrorMenu callbackMenu) : base(0, 0, 700, 550, showUpperRightCloseButton: true)
        {
            _callbackMenu = callbackMenu;

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

            // Establish the buttons that will be used to select the outfits
            for (int i = 0; i <= OUTFITS_PER_PAGE; i++)
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
                outfitButtons.Add(packButton);

                ClickableTextureComponent renameButton = new ClickableTextureComponent(new Rectangle(packButton.bounds.Right - 192, packButton.bounds.Y + packButton.bounds.Height / 4 + 8, 56, 48), Game1.mouseCursors, new Rectangle(66, 4, 14, 12), 3f)
                {
                    myID = i + 200,
                    downNeighborID = -7777,
                    upNeighborID = ((i > 0) ? (i + 100 - 1) : (-1)),
                    rightNeighborID = -7777,
                    leftNeighborID = -7777,
                    fullyImmutable = true
                };
                renameButtons.Add(renameButton);

                ClickableTextureComponent saveButton = new ClickableTextureComponent(new Rectangle(renameButton.bounds.X + 64, packButton.bounds.Y + packButton.bounds.Height / 4 - 2, 56, 48), Game1.mouseCursors, new Rectangle(240, 320, 16, 16), 3f)
                {
                    myID = i + 100,
                    downNeighborID = -7777,
                    upNeighborID = ((i > 0) ? (i + 100 - 1) : (-1)),
                    rightNeighborID = -7777,
                    leftNeighborID = -7777,
                    fullyImmutable = true
                };
                saveButtons.Add(saveButton);

                ClickableTextureComponent deleteButton = new ClickableTextureComponent(new Rectangle(renameButton.bounds.X + 128, packButton.bounds.Y + packButton.bounds.Height / 4 + 4, 56, 48), Game1.mouseCursors, new Rectangle(323, 433, 9, 10), 4f)
                {
                    myID = i + 300,
                    downNeighborID = -7777,
                    upNeighborID = ((i > 0) ? (i + 100 - 1) : (-1)),
                    rightNeighborID = -7777,
                    leftNeighborID = -7777,
                    fullyImmutable = true
                };
                deleteButtons.Add(deleteButton);
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
            base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 20, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        }

        public void PaginatePacks()
        {
            var outfits = FashionSense.outfitManager.GetOutfits(Game1.player);
            _pages = new List<List<Outfit>>();

            int count = outfits.Count - 1;
            foreach (var contentPack in outfits.OrderBy(p => p.Name))
            {
                int which = outfits.Count - 1 - count;
                int page = which / OUTFITS_PER_PAGE;

                while (_pages.Count <= page)
                {
                    _pages.Add(new List<Outfit>());
                }

                if (page == 0 && which == 0)
                {
                    _pages[page].Add(new Outfit() { Name = CREATE_OUTFIT_NAME });
                    count--;
                }

                _pages[page].Add(contentPack);

                count--;
            }

            if (_pages.Count == 0)
            {
                _pages.Add(new List<Outfit>());
                _pages[0].Add(new Outfit() { Name = CREATE_OUTFIT_NAME });
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
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            if (base.upperRightCloseButton != null && base.readyToClose() && base.upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                {
                    Game1.playSound("bigDeSelect");
                }

                Game1.activeClickableMenu = _callbackMenu;
                base.exitThisMenu();
            }

            for (int i = 0; i < outfitButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                // Check if the outfits are being clicked
                if (outfitButtons[i].containsPoint(x, y))
                {
                    if (_pages[_currentPage][i].Name == CREATE_OUTFIT_NAME)
                    {
                        Game1.activeClickableMenu = new NameMenu(FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.outfit_naming"), this);
                    }
                    else
                    {
                        // Check if the functional buttons are being clicked
                        if (saveButtons[i].containsPoint(x, y))
                        {
                            FashionSense.outfitManager.OverrideOutfit(Game1.player, _pages[_currentPage][i].Name);
                            Game1.activeClickableMenu = _callbackMenu;
                            base.exitThisMenu();

                            return;
                        }
                        if (renameButtons[i].containsPoint(x, y))
                        {
                            Game1.activeClickableMenu = new NameMenu(FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.outfit_naming"), this, _pages[_currentPage][i].Name);
                            return;
                        }
                        if (deleteButtons[i].containsPoint(x, y))
                        {
                            FashionSense.outfitManager.DeleteOutfit(Game1.player, _pages[_currentPage][i].Name);
                            PaginatePacks();
                            return;
                        }

                        FashionSense.outfitManager.SetOutfit(Game1.player, _pages[_currentPage][i]);
                        _callbackMenu.colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, _callbackMenu.GetAccessoryIndex()));

                        Game1.activeClickableMenu = _callbackMenu;
                        base.exitThisMenu();
                    }

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

            for (int i = 0; i < outfitButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                // Check if the outfits are being hovered
                if (outfitButtons[i].containsPoint(x, y))
                {
                    if (_pages[_currentPage][i].Name == CREATE_OUTFIT_NAME)
                    {
                        _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.create_outfit_hover");
                        return;
                    }

                    // Check if the functional buttons are being hovered
                    if (saveButtons[i].containsPoint(x, y))
                    {
                        _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.save");
                        return;
                    }
                    if (renameButtons[i].containsPoint(x, y))
                    {
                        _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.rename");
                        return;
                    }
                    if (deleteButtons[i].containsPoint(x, y))
                    {
                        _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.delete");
                        return;
                    }

                    if (_pages[_currentPage][i].Name.Length > 18)
                    {
                        _hoverText = $"{ _pages[_currentPage][i].Name}";
                        return;
                    }
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
            SpriteText.drawStringWithScrollCenteredAt(b, FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.outfits"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Draw the content pack buttons
            for (int j = 0; j < outfitButtons.Count; j++)
            {
                if (_pages.Count() > 0 && _pages[_currentPage].Count() > j)
                {
                    var packName = _pages[_currentPage][j].Name;
                    if (packName == CREATE_OUTFIT_NAME)
                    {
                        // Draw the create outfit button
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), outfitButtons[j].bounds.X, outfitButtons[j].bounds.Y, outfitButtons[j].bounds.Width, outfitButtons[j].bounds.Height, outfitButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
                        SpriteText.drawStringHorizontallyCenteredAt(b, "= " + FashionSense.modHelper.Translation.Get("ui.fashion_sense.create_outfit_button") + " =", outfitButtons[j].bounds.X + outfitButtons[j].bounds.Width / 2, outfitButtons[j].bounds.Y + 20);
                        continue;
                    }

                    if (packName.Length > 18)
                    {
                        packName = $"{packName.Substring(0, 18).TrimEnd()}...";
                    }

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), outfitButtons[j].bounds.X, outfitButtons[j].bounds.Y, outfitButtons[j].bounds.Width, outfitButtons[j].bounds.Height, outfitButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
                    SpriteText.drawString(b, packName, outfitButtons[j].bounds.X + 32, outfitButtons[j].bounds.Y + 20);

                    // Draw the functional buttons
                    saveButtons[j].draw(b);
                    renameButtons[j].draw(b);
                    deleteButtons[j].draw(b);
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
            base.upperRightCloseButton.draw(b);

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}
