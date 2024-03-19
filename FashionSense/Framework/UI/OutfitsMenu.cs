using FashionSense.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using TextCopy;

namespace FashionSense.Framework.UI
{
    public class OutfitsMenu : IClickableMenu
    {
        private int _currentPage;
        private string _hoverText = "";
        private string _bottomBannerMessage = "";
        private double _bottomBannerTimeRemainingInMilliseconds = 0;
        private bool _isDisplayingPresets;
        private const int OUTFITS_PER_PAGE = 6;
        private const string CREATE_OUTFIT_NAME = "PeacefulEnd.Create.Outfit.Button";

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        public ClickableComponent importButton;
        public ClickableComponent presetsButton;
        public List<ClickableComponent> outfitButtons = new List<ClickableComponent>();
        public List<ClickableTextureComponent> shareButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> exportButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> saveButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> renameButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> deleteButtons = new List<ClickableTextureComponent>();

        private HandMirrorMenu _callbackMenu;
        private List<List<Outfit>> _pages;

        public OutfitsMenu(HandMirrorMenu callbackMenu) : base(0, 0, 700, 550, showUpperRightCloseButton: false)
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
            PaginatePacks(FashionSense.outfitManager.GetOutfits(Game1.player));

            // Establish the buttons that will be used to select the outfits
            for (int i = 0; i <= OUTFITS_PER_PAGE; i++)
            {
                ClickableComponent packButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 32) / 6), base.width - 32, (base.height - 32) / 6 + 4), string.Concat(i))
                {
                    myID = i,
                    downNeighborID = i < OUTFITS_PER_PAGE - 1 ? i + 1 : -1,
                    upNeighborID = i > 0 ? i - 1 : -1,
                    rightNeighborID = i + 200,
                    leftNeighborID = 102
                };
                outfitButtons.Add(packButton);

                ClickableTextureComponent shareButton = new ClickableTextureComponent(new Rectangle(packButton.bounds.Right - 320, packButton.bounds.Y + packButton.bounds.Height / 4 + 2, 56, 48), Game1.mouseCursors, new Rectangle(0, 592, 16, 16), 3f)
                {
                    myID = i + 200,
                    downNeighborID = i < OUTFITS_PER_PAGE - 1 ? i + 200 + 1 : -1,
                    upNeighborID = i > 0 ? i + 200 - 1 : -1,
                    rightNeighborID = i + 300,
                    leftNeighborID = i,
                    fullyImmutable = true,
                    name = "inactive"
                };
                shareButtons.Add(shareButton);

                ClickableTextureComponent exportButton = new ClickableTextureComponent(new Rectangle(packButton.bounds.Right - 246, packButton.bounds.Y + packButton.bounds.Height / 4 - 4, 56, 48), FashionSense.assetManager.exportButton, new Rectangle(0, 0, 9, 16), 3f)
                {
                    myID = i + 300,
                    downNeighborID = i < OUTFITS_PER_PAGE - 1 ? i + 200 + 1 : -1,
                    upNeighborID = i > 0 ? i + 200 - 1 : -1,
                    rightNeighborID = i + 300,
                    leftNeighborID = i,
                    fullyImmutable = true,
                    name = "inactive"
                };
                exportButtons.Add(exportButton);

                ClickableTextureComponent renameButton = new ClickableTextureComponent(new Rectangle(packButton.bounds.Right - 192, packButton.bounds.Y + packButton.bounds.Height / 4 + 8, 56, 48), Game1.mouseCursors, new Rectangle(66, 4, 14, 12), 3f)
                {
                    myID = i + 400,
                    downNeighborID = i < OUTFITS_PER_PAGE - 1 ? i + 300 + 1 : -1,
                    upNeighborID = i > 0 ? i + 300 - 1 : -1,
                    rightNeighborID = i + 400,
                    leftNeighborID = i + 200,
                    fullyImmutable = true
                };
                renameButtons.Add(renameButton);

                ClickableTextureComponent saveButton = new ClickableTextureComponent(new Rectangle(renameButton.bounds.X + 64, packButton.bounds.Y + packButton.bounds.Height / 4 - 2, 56, 48), Game1.mouseCursors, new Rectangle(240, 320, 16, 16), 3f)
                {
                    myID = i + 500,
                    downNeighborID = i < OUTFITS_PER_PAGE - 1 ? i + 400 + 1 : -1,
                    upNeighborID = i > 0 ? i + 400 - 1 : -1,
                    rightNeighborID = i + 500,
                    leftNeighborID = i + 300,
                    fullyImmutable = true
                };
                saveButtons.Add(saveButton);

                ClickableTextureComponent deleteButton = new ClickableTextureComponent(new Rectangle(renameButton.bounds.X + 128, packButton.bounds.Y + packButton.bounds.Height / 4 + 4, 56, 48), Game1.mouseCursors, new Rectangle(323, 433, 9, 10), 4f)
                {
                    myID = i + 600,
                    downNeighborID = i < OUTFITS_PER_PAGE - 1 ? i + 500 + 1 : -1,
                    upNeighborID = i > 0 ? i + 500 - 1 : -1,
                    rightNeighborID = 101,
                    leftNeighborID = i + 400,
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
            importButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 4, base.yPositionOnScreen - 48, (int)Game1.dialogueFont.MeasureString(FashionSense.modHelper.Translation.Get("ui.fashion_sense.buttons.import")).X + 64, 52), FashionSense.modHelper.Translation.Get("ui.fashion_sense.buttons.import"))
            {
                myID = 100,
                rightNeighborID = -7777
            };
            presetsButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width - 202, base.yPositionOnScreen - 48, (int)Game1.dialogueFont.MeasureString(FashionSense.modHelper.Translation.Get("ui.fashion_sense.buttons.presets")).X + 64, 52), FashionSense.modHelper.Translation.Get("ui.fashion_sense.buttons.presets"))
            {
                myID = 99
            };

            // Handle GamePad integration
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        public void PaginatePacks(List<Outfit> outfits, bool isPreset = false)
        {
            _isDisplayingPresets = isPreset;
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

                if (page == 0 && which == 0 && _isDisplayingPresets is false)
                {
                    _pages[page].Add(new Outfit() { Name = CREATE_OUTFIT_NAME });
                    count--;
                }

                _pages[page].Add(contentPack);

                count--;
            }

            if (_pages.Count == 0 && _isDisplayingPresets is false)
            {
                _pages.Add(new List<Outfit>());
                _pages[0].Add(new Outfit() { Name = CREATE_OUTFIT_NAME });
            }
            _currentPage = Math.Min(Math.Max(_currentPage, 0), _pages.Count - 1);
        }

        private void CreateBottomBannerMesage(string message)
        {
            _bottomBannerMessage = message;
            _bottomBannerTimeRemainingInMilliseconds = 3500;
        }

        private void DrawButton(SpriteBatch b, ClickableComponent button)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), button.bounds.X, button.bounds.Y, button.bounds.Width, button.bounds.Height, Color.White, 4f, drawShadow: false, 1f);
            Vector2 string_center = Game1.dialogueFont.MeasureString(button.name) / 2f;
            string_center.X = (int)(string_center.X / 4f) * 4;
            string_center.Y = (int)(string_center.Y / 4f) * 4;
            Utility.drawTextWithShadow(b, button.name, Game1.dialogueFont, new Vector2(button.bounds.Center.X, button.bounds.Center.Y) - string_center, Game1.textColor, 1f, 1f + 1E-06f, -1, -1, 0);
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
                    if (_isDisplayingPresets)
                    {
                        PaginatePacks(FashionSense.outfitManager.GetOutfits(Game1.player), isPreset: false);
                    }
                    else
                    {
                        Game1.activeClickableMenu = _callbackMenu;
                        base.exitThisMenu();
                    }
                    return;
                }
                else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !base.overrideSnappyMenuCursorMovementBan())
                {
                    this.applyMovementKey(key);
                    this.currentlySnappedComponent.snapMouseCursorToCenter();
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            if (importButton.containsPoint(x, y))
            {
                string clipboardText = ClipboardService.GetText();
                if (string.IsNullOrEmpty(clipboardText))
                {
                    CreateBottomBannerMesage("No text found in clipboard!");
                    return;
                }

                try
                {
                    var outfit = JsonConvert.DeserializeObject<Outfit>(clipboardText);

                    FashionSense.outfitManager.AddOutfit(Game1.player, outfit);
                    PaginatePacks(FashionSense.outfitManager.GetOutfits(Game1.player));

                    CreateBottomBannerMesage($"Imported the outfit \"{outfit.Name}\" by {outfit.Author}");
                }
                catch (Exception ex)
                {
                    CreateBottomBannerMesage("Failed to parse clipboard text!");
                    FashionSense.monitor.Log($"Failed to parse clipboard text into a Fashion Sense outfit: {ex}", StardewModdingAPI.LogLevel.Trace);
                }

                return;
            }
            if (presetsButton.containsPoint(x, y))
            {
                PaginatePacks(FashionSense.outfitManager.GetPresetOutfits(), isPreset: true);
                return;
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

                        var outfit = FashionSense.outfitManager.GetOutfit(Game1.player, _pages[_currentPage][i].Name, _isDisplayingPresets);
                        if (outfit.IsGlobal is false && outfit.IsPreset is false)
                        {
                            if (outfit.IsBeingShared is false)
                            {
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
                                    PaginatePacks(FashionSense.outfitManager.GetOutfits(Game1.player));
                                    return;
                                }
                            }
                            if (exportButtons[i].containsPoint(x, y))
                            {
                                // Set the author name to the player's name, if it is null / empty
                                if (string.IsNullOrEmpty(outfit.Author))
                                {
                                    outfit.Author = Game1.player.Name;
                                }

                                ClipboardService.SetText(outfit.Export());
                                CreateBottomBannerMesage(FashionSense.modHelper.Translation.Get("ui.fashion_sense.exported_outfit"));

                                return;
                            }
                            if (shareButtons[i].containsPoint(x, y))
                            {
                                FashionSense.outfitManager.SetOutfitShareState(Game1.player, _pages[_currentPage][i].Name, !outfit.IsBeingShared);

                                return;
                            }
                        }

                        FashionSense.outfitManager.SetOutfit(Game1.player, _pages[_currentPage][i]);
                        _callbackMenu.Reset();

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

            if (importButton.containsPoint(x, y))
            {
                _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.buttons.import.description");
                return;
            }
            else if (presetsButton.containsPoint(x, y))
            {
                _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.buttons.presets.description");
                return;
            }

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
                    var outfit = FashionSense.outfitManager.GetOutfit(Game1.player, _pages[_currentPage][i].Name, _isDisplayingPresets);
                    if (outfit.IsGlobal is false && outfit.IsPreset is false)
                    {
                        if (outfit.IsBeingShared is false)
                        {
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
                            if (exportButtons[i].containsPoint(x, y))
                            {
                                _hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.export");
                                return;
                            }
                        }
                        if (shareButtons[i].containsPoint(x, y))
                        {
                            _hoverText = outfit.IsBeingShared ? FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.share.active") : FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.share.inactive");
                            return;
                        }
                    }
                    else if (outfit.IsPreset)
                    {
                        _hoverText = string.Format(FashionSense.modHelper.Translation.Get("ui.fashion_sense.preset_info"), outfit.Name, outfit.Author, outfit.Source);
                    }
                    else if (_pages[_currentPage][i].Name.Length > 12)
                    {
                        _hoverText = $"{_pages[_currentPage][i].Name}";
                    }

                    var missingAppearanceIds = outfit.GetMissingAppearanceIds();
                    if (missingAppearanceIds.Count > 0)
                    {
                        _hoverText += string.IsNullOrEmpty(_hoverText) ? "" : "\n\n";
                        _hoverText += string.Format(FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.missing_appearances"), string.Join("\n", missingAppearanceIds));
                    }
                }
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            if (b == Buttons.B && base.readyToClose())
            {
                Game1.activeClickableMenu = _callbackMenu;
                base.exitThisMenu();
                return;
            }

            if ((b == Buttons.RightTrigger || b == Buttons.RightShoulder) && _currentPage < _pages.Count - 1)
            {
                _currentPage++;
                Game1.playSound("shiny4");
            }
            else if ((b == Buttons.LeftTrigger || b == Buttons.LeftShoulder) && _currentPage > 0)
            {
                _currentPage--;
                Game1.playSound("shiny4");
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.currentlySnappedComponent.snapMouseCursorToCenter();
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (_bottomBannerTimeRemainingInMilliseconds > 0)
            {
                _bottomBannerTimeRemainingInMilliseconds -= time.ElapsedGameTime.TotalMilliseconds;
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

                    var outfit = FashionSense.outfitManager.GetOutfit(Game1.player, packName, _isDisplayingPresets);
                    if (packName.Length > 12)
                    {
                        packName = $"{packName.Substring(0, 12).TrimEnd()}...";
                    }

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), outfitButtons[j].bounds.X, outfitButtons[j].bounds.Y, outfitButtons[j].bounds.Width, outfitButtons[j].bounds.Height, outfitButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
                    SpriteText.drawString(b, packName, outfitButtons[j].bounds.X + 32, outfitButtons[j].bounds.Y + 20, color: outfit.HasAllRequiredAppearances() ? new Color(86, 22, 12) : new Color(86, 22, 12, 150));

                    // Draw the functional buttons
                    if (outfit.IsPreset is true)
                    {
                        SpriteText.drawString(b, FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.preset"), outfitButtons[j].bounds.Width + 135, outfitButtons[j].bounds.Y + 20);
                    }
                    else if (outfit.IsGlobal is false)
                    {
                        if (outfit.IsBeingShared is false)
                        {
                            saveButtons[j].draw(b);
                            renameButtons[j].draw(b);
                            deleteButtons[j].draw(b);
                        }

                        exportButtons[j].draw(b);
                        shareButtons[j].draw(b, outfit.IsBeingShared ? Color.White : new Color(55, 55, 55, 55), 1f);
                    }
                    else
                    {
                        SpriteText.drawString(b, FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_info.shared"), outfitButtons[j].bounds.Width + 135, outfitButtons[j].bounds.Y + 20);
                    }
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

            // Draw import / presets buttons
            DrawButton(b, this.importButton);
            DrawButton(b, this.presetsButton);

            // Draw hover text
            if (!_hoverText.Equals(""))
            {
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                IClickableMenu.drawHoverText(b, _hoverText, Game1.smallFont);
            }

            if (string.IsNullOrEmpty(_bottomBannerMessage) is false)
            {
                if (_bottomBannerTimeRemainingInMilliseconds > 0)
                {
                    SpriteText.drawStringWithScrollCenteredAt(b, _bottomBannerMessage, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + base.height + 8, alpha: (float)(_bottomBannerTimeRemainingInMilliseconds > 1000 ? 1f : _bottomBannerTimeRemainingInMilliseconds / 1000));
                }
                else
                {
                    _bottomBannerMessage = null;
                }
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}
