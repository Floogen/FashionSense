using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Body;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.UI.Components;
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

namespace FashionSense.Framework.UI
{
    internal class SearchMenu : IClickableMenu
    {
        public ClickableTextureComponent forwardButton;
        public ClickableTextureComponent backButton;

        public List<AppearanceContentPack> filteredTextureOptions = new List<AppearanceContentPack>();
        public List<AppearanceContentPack> cachedTextureOptions = new List<AppearanceContentPack>();
        public List<ClickableTextureComponent> availableTextures = new List<ClickableTextureComponent>();
        public List<Farmer> fakeFarmers = new List<Farmer>();

        // Textbox
        private TextBox _searchBox;
        private ClickableComponent _searchBoxCC;

        private string _title = "Search";
        private string _hoverText = "";
        private string _cachedTextBoxValue;

        private int _startingRow = 0;
        private int _texturesPerRow = 4;
        private int _maxRows = 2;
        private int _rowChangePerScroll = 2;

        private string _appearanceFilter;
        private Farmer _displayFarmer;
        private HandMirrorMenu _callbackMenu;

        private FilterDropDown _searchFilterOptions;
        private ClickableComponent _changeDirectionButton;

        // Favorites
        // Reuses the vanilla friendship heart icons (Game1.mouseCursors) as the favorite toggle
        private static readonly Rectangle FilledHeartSourceRect = new Rectangle(211, 428, 7, 6);
        private static readonly Rectangle EmptyHeartSourceRect = new Rectangle(218, 428, 7, 6);
        private const float FavoriteIconScale = 3f;
        private const int FavoritesOnlyFilterOptionIndex = 4;

        private int _cachedFilterOption = -1;
        private List<Rectangle> _favoriteIconBounds = new List<Rectangle>();

        public SearchMenu(Farmer who, string appearanceFilter, HandMirrorMenu callbackMenu) : base(0, 0, 832, 576, showUpperRightCloseButton: true)
        {
            _displayFarmer = who;
            _appearanceFilter = appearanceFilter;
            _callbackMenu = callbackMenu;

            if (Game1.uiViewport.Height >= 720)
            {
                int adjustedHeight = Game1.uiViewport.Height - 150;

                _maxRows = adjustedHeight / 240;
                _rowChangePerScroll = _maxRows;
                base.height = adjustedHeight;
            }

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            // Populate the texture selection components
            var appearancePacks = FashionSense.textureManager.GetAllAppearanceModels();
            switch (appearanceFilter)
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
                case HandMirrorMenu.SHOES_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShoesContentPack).ToList();
                    break;
                case HandMirrorMenu.BODY_FILTER_BUTTON:
                    appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is BodyContentPack).ToList();
                    break;
            }

            for (int m = 0; m < appearancePacks.Count; m++)
            {
                filteredTextureOptions.Add(appearancePacks[m]);
                cachedTextureOptions.Add(appearancePacks[m]);
            }

            var drawingScale = 4f;
            var widthOffsetScale = 3;
            var sourceRect = new Rectangle(0, 0, Game1.daybg.Width, Game1.daybg.Height);

            for (int r = 0; r < _maxRows; r++)
            {
                for (int c = 0; c < _texturesPerRow; c++)
                {
                    var componentId = c + r * _texturesPerRow;
                    availableTextures.Add(new ClickableTextureComponent(new Rectangle(32 + base.xPositionOnScreen + IClickableMenu.borderWidth + componentId % _texturesPerRow * 64 * widthOffsetScale, base.yPositionOnScreen + sourceRect.Height / 2 + componentId + (r * sourceRect.Height) + (r > 0 ? (r + 1) * 32 : 32) - 32, sourceRect.Width, sourceRect.Height), null, new Rectangle(), drawingScale, false)
                    {
                        myID = componentId,
                        downNeighborID = componentId + _texturesPerRow,
                        upNeighborID = componentId >= _texturesPerRow ? componentId - _texturesPerRow : -1,
                        rightNeighborID = c == _texturesPerRow - 1 ? 9997 : componentId + 1,
                        leftNeighborID = c > 0 ? componentId - 1 : 9998
                    });

                    var tileBounds = availableTextures[availableTextures.Count - 1].bounds;
                    _favoriteIconBounds.Add(new Rectangle(tileBounds.X + 8, tileBounds.Y + 8, (int)(FilledHeartSourceRect.Width * FavoriteIconScale), (int)(FilledHeartSourceRect.Height * FavoriteIconScale)));

                    try
                    {
                        var fakeFarmer = _displayFarmer.CreateFakeEventFarmer();
                        fakeFarmer.faceDirection(who.FacingDirection);
                        foreach (var key in _displayFarmer.modData.Keys)
                        {
                            fakeFarmer.modData[key] = _displayFarmer.modData[key];
                        }
                        FashionSense.accessoryManager.CopyAccessories(_displayFarmer, fakeFarmer);
                        FashionSense.colorManager.CopyColors(_displayFarmer, fakeFarmer);

                        fakeFarmers.Add(fakeFarmer);
                    }
                    catch (Exception ex)
                    {
                        FashionSense.monitor.Log($"Failed to create display farmers for search menu:\n{ex}", StardewModdingAPI.LogLevel.Trace);
                        base.exitThisMenu();
                    }
                }
            }
            UpdateDisplayFarmers();

            backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + base.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = 9998,
                rightNeighborID = 0,
                leftNeighborImmutable = true,
                upNeighborImmutable = true,
                downNeighborImmutable = true
            };
            forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 64 - 48, base.yPositionOnScreen + base.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = 9997
            };

            // Textbox related
            var xTextbox = base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 320;
            var yTextbox = base.yPositionOnScreen - 58;
            _searchBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = base.xPositionOnScreen + 32,
                Y = base.yPositionOnScreen + 32,
                Width = 384,
                limitWidth = false,
                Text = String.Empty
            };

            // Establish the search options
            List<string> options = new List<string>() { FashionSense.modHelper.Translation.Get("ui.fashion_sense.search.filter.none"), FashionSense.modHelper.Translation.Get("ui.fashion_sense.search.filter.author"), FashionSense.modHelper.Translation.Get("ui.fashion_sense.search.filter.pack_name"), FashionSense.modHelper.Translation.Get("ui.fashion_sense.search.filter.tags"), FashionSense.modHelper.Translation.Get("ui.fashion_sense.search.filter.favorites") };
            _searchFilterOptions = new FilterDropDown("Filter", 0) { dropDownDisplayOptions = options, dropDownOptions = options };
            _searchFilterOptions.bounds = new Rectangle(_searchBox.X + _searchBox.Width + 16, _searchBox.Y - 1, 256, 48);
            _searchFilterOptions.RecalculateBounds();

            _searchBoxCC = new ClickableComponent(new Rectangle(xTextbox, yTextbox, 192, 48), "")
            {
                myID = 9999,
                downNeighborID = 0,
                upNeighborImmutable = true,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            };
            Game1.keyboardDispatcher.Subscriber = _searchBox;
            _searchBox.Selected = true;

            _changeDirectionButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width - 256, base.yPositionOnScreen - 42, 256, 45), "changeDirectionButton");

            // Handle GamePad integration
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        public void UpdateDisplayFarmers()
        {
            for (int i = 0; i < availableTextures.Count; i++)
            {
                var textureIndex = i + _startingRow * _texturesPerRow;
                if (textureIndex < filteredTextureOptions.Count)
                {
                    var targetPack = filteredTextureOptions[textureIndex];

                    string modDataKey = null;
                    switch (_appearanceFilter)
                    {
                        case HandMirrorMenu.HAIR_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                            break;
                        case HandMirrorMenu.ACCESSORY_FILTER_BUTTON:
                            FashionSense.accessoryManager.AddAccessory(fakeFarmers[i], targetPack.Id, _callbackMenu.GetAccessoryIndex(), preserveColor: true);
                            FashionSense.ResetAnimationModDataFields(fakeFarmers[i], 0, AnimationModel.Type.Idle, fakeFarmers[i].FacingDirection);
                            FashionSense.SetSpriteDirty(fakeFarmers[i]);
                            continue;
                        case HandMirrorMenu.HAT_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                            break;
                        case HandMirrorMenu.SHIRT_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_SHIRT_ID;
                            break;
                        case HandMirrorMenu.PANTS_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_PANTS_ID;
                            break;
                        case HandMirrorMenu.SLEEVES_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_SLEEVES_ID;
                            break;
                        case HandMirrorMenu.SHOES_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_SHOES_ID;
                            break;
                        case HandMirrorMenu.BODY_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_BODY_ID;
                            break;
                    }

                    if (fakeFarmers.ElementAtOrDefault(i) is null)
                    {
                        continue;
                    }

                    fakeFarmers[i].modData[modDataKey] = targetPack.Id;
                    FashionSense.ResetAnimationModDataFields(fakeFarmers[i], 0, AnimationModel.Type.Idle, fakeFarmers[i].FacingDirection);
                    FashionSense.SetSpriteDirty(fakeFarmers[i]);
                }
            }
        }

        private void ChangeDisplayFarmersDirection()
        {
            for (int i = 0; i < availableTextures.Count; i++)
            {
                fakeFarmers[i].faceDirection(fakeFarmers[i].FacingDirection == 3 ? 0 : fakeFarmers[i].FacingDirection + 1);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            _hoverText = String.Empty;
            if (Game1.IsFading())
            {
                return;
            }

            if (_searchFilterOptions.IsClicked && _searchFilterOptions.dropDownBounds.Contains(x, y))
            {
                return;
            }

            for (int i = 0; i < availableTextures.Count; i++)
            {
                var textureIndex = i + _startingRow * _texturesPerRow;
                if (textureIndex >= filteredTextureOptions.Count)
                {
                    continue;
                }

                if (_favoriteIconBounds[i].Contains(x, y))
                {
                    var targetPack = filteredTextureOptions[textureIndex];
                    _hoverText = FashionSense.favoritesManager.IsFavorite(_displayFarmer, targetPack.PackType, targetPack.Id)
                        ? FashionSense.modHelper.Translation.Get("ui.fashion_sense.favorite.remove")
                        : FashionSense.modHelper.Translation.Get("ui.fashion_sense.favorite.add");
                }
                else if (availableTextures[i].containsPoint(x, y))
                {
                    var targetPack = filteredTextureOptions[textureIndex];

                    _hoverText = string.Format(FashionSense.modHelper.Translation.Get("ui.fashion_sense.search.about"), targetPack.Name, targetPack.Author, targetPack.PackName, string.Join(", ", targetPack.Tags));
                }
            }

            forwardButton.tryHover(x, y, 0.2f);
            backButton.tryHover(x, y, 0.2f);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && base.readyToClose())
            {
                Game1.activeClickableMenu = _callbackMenu;
                base.exitThisMenu();
                return;
            }
            else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !base.overrideSnappyMenuCursorMovementBan())
            {
                this.applyMovementKey(key);
                this.currentlySnappedComponent.snapMouseCursorToCenter();
            }
        }

        private void ApplyFilters()
        {
            IEnumerable<AppearanceContentPack> results = cachedTextureOptions;

            if (_searchFilterOptions.selectedOption == FavoritesOnlyFilterOptionIndex)
            {
                results = results.Where(i => FashionSense.favoritesManager.IsFavorite(_displayFarmer, i.PackType, i.Id));
            }
            else if (String.IsNullOrEmpty(_searchBox.Text) is false)
            {
                switch (_searchFilterOptions.selectedOption)
                {
                    case 0:
                        results = results.Where(i => i.Name.Contains(_searchBox.Text, StringComparison.OrdinalIgnoreCase));
                        break;
                    case 1:
                        results = results.Where(i => i.Author.Contains(_searchBox.Text, StringComparison.OrdinalIgnoreCase));
                        break;
                    case 2:
                        results = results.Where(i => i.PackName.Contains(_searchBox.Text, StringComparison.OrdinalIgnoreCase));
                        break;
                    case 3:
                        results = results.Where(i => i.HasTag(_searchBox.Text));
                        break;
                }
            }

            _startingRow = 0;
            filteredTextureOptions = results.ToList();
            UpdateDisplayFarmers();
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (_searchBox.Text != _cachedTextBoxValue || _searchFilterOptions.selectedOption != _cachedFilterOption)
            {
                _cachedTextBoxValue = _searchBox.Text;
                _cachedFilterOption = _searchFilterOptions.selectedOption;
                ApplyFilters();
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = false)
        {
            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            for (int i = 0; i < availableTextures.Count; i++)
            {
                var textureIndex = i + _startingRow * _texturesPerRow;
                if (textureIndex < filteredTextureOptions.Count && _favoriteIconBounds[i].Contains(x, y))
                {
                    var targetPack = filteredTextureOptions[textureIndex];
                    FashionSense.favoritesManager.ToggleFavorite(_displayFarmer, targetPack.PackType, targetPack.Id);
                    Game1.playSound("coin");

                    if (_searchFilterOptions.selectedOption == FavoritesOnlyFilterOptionIndex)
                    {
                        ApplyFilters();
                    }
                    return;
                }
            }

            for (int i = 0; i < availableTextures.Count; i++)
            {
                ClickableTextureComponent c = availableTextures[i];
                var textureIndex = i + _startingRow * _texturesPerRow;
                if (textureIndex < filteredTextureOptions.Count && c.containsPoint(x, y))
                {
                    foreach (var key in fakeFarmers[i].modData.Keys)
                    {
                        _displayFarmer.modData[key] = fakeFarmers[i].modData[key];
                    }
                    FashionSense.accessoryManager.CopyAccessories(fakeFarmers[i], _displayFarmer);

                    FashionSense.ResetAnimationModDataFields(_displayFarmer, 0, AnimationModel.Type.Idle, _displayFarmer.FacingDirection);
                    FashionSense.SetSpriteDirty(_displayFarmer);
                    _callbackMenu.Reset();

                    base.exitThisMenu();
                    Game1.activeClickableMenu = _callbackMenu;
                    return;
                }
            }

            if (_startingRow > 0 && backButton.containsPoint(x, y))
            {
                _startingRow = Math.Max(_startingRow - _rowChangePerScroll, 0);
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
                return;
            }
            if ((_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count && forwardButton.containsPoint(x, y))
            {
                _startingRow += _rowChangePerScroll;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
                return;
            }

            // Handle filter
            if (_searchFilterOptions.bounds.Contains(x, y) || (_searchFilterOptions.IsClicked && _searchFilterOptions.dropDownBounds.Contains(x, y)))
            {
                _searchFilterOptions.receiveLeftClick(x, y);
            }

            // Handle change direction button
            if (_changeDirectionButton.containsPoint(x, y))
            {
                ChangeDisplayFarmersDirection();
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);

            // Handle filter
            if (_searchFilterOptions.bounds.Contains(x, y) || (_searchFilterOptions.IsClicked && _searchFilterOptions.dropDownBounds.Contains(x, y)))
            {
                _searchFilterOptions.leftClickHeld(x, y);
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);

            // Handle filter
            if (_searchFilterOptions.bounds.Contains(x, y) || _searchFilterOptions.dropDownBounds.Contains(x, y))
            {
                _searchFilterOptions.leftClickReleased(x, y);
                _cachedTextBoxValue = null;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && _startingRow > 0)
            {
                _startingRow = Math.Max(_startingRow - _rowChangePerScroll, 0);
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && (_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count)
            {
                _startingRow += _rowChangePerScroll;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
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

            if ((b == Buttons.RightTrigger || b == Buttons.RightShoulder) && (_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count)
            {
                _startingRow += _rowChangePerScroll;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
            }
            else if ((b == Buttons.LeftTrigger || b == Buttons.LeftShoulder) && _startingRow > 0)
            {
                _startingRow = Math.Max(_startingRow - _rowChangePerScroll, 0);
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.dialogueUp && !Game1.IsFading())
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                SpriteText.drawStringWithScrollCenteredAt(b, _title, base.xPositionOnScreen + 128, base.yPositionOnScreen - 64);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

                for (int i = 0; i < availableTextures.Count; i++)
                {
                    availableTextures[i].item = null;
                    availableTextures[i].texture = null;

                    var textureIndex = i + _startingRow * _texturesPerRow;
                    if (textureIndex < filteredTextureOptions.Count && fakeFarmers.Count > i)
                    {
                        var targetPack = filteredTextureOptions[textureIndex];

                        // Farmer portrait
                        b.Draw(Game1.daybg, new Vector2(availableTextures[i].bounds.X, availableTextures[i].bounds.Y), Color.White);
                        FarmerRenderer.isDrawingForUI = true;
                        var fakeFarmer = fakeFarmers[i];
                        fakeFarmer.FarmerRenderer.draw(b, fakeFarmer.FarmerSprite.CurrentAnimationFrame, fakeFarmer.FarmerSprite.CurrentFrame, fakeFarmer.FarmerSprite.SourceRect, new Vector2(availableTextures[i].bounds.Center.X - 32, availableTextures[i].bounds.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, fakeFarmer);

                        //fakeFarmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Rectangle(0, 0, 16, 32), new Vector2(availableTextures[i].bounds.Center.X - 32, availableTextures[i].bounds.Bottom - 160), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, fakeFarmer);
                        FarmerRenderer.isDrawingForUI = false;

                        // Favorite toggle icon
                        var isFavorite = FashionSense.favoritesManager.IsFavorite(_displayFarmer, targetPack.PackType, targetPack.Id);
                        b.Draw(Game1.mouseCursors, new Vector2(_favoriteIconBounds[i].X, _favoriteIconBounds[i].Y), isFavorite ? FilledHeartSourceRect : EmptyHeartSourceRect, Color.White, 0f, Vector2.Zero, FavoriteIconScale, SpriteEffects.None, 1f);
                    }
                }

                _searchBox.Draw(b);
            }

            // Draw scroll buttons
            backButton.draw(b, _startingRow > 0 ? Color.White : Color.Gray, 1f, 0, 0);
            forwardButton.draw(b, (_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count ? Color.White : Color.Gray, 1f, 0, 0);

            // Draw the filter options box
            _searchFilterOptions.draw(b, 0, 0);

            // Draw change direction button
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), _changeDirectionButton.bounds.X, _changeDirectionButton.bounds.Y, _changeDirectionButton.bounds.Width, _changeDirectionButton.bounds.Height, Color.White, 4f, drawShadow: false);
            Utility.drawTextWithShadow(b, "Change Direction", Game1.smallFont, new Vector2(_changeDirectionButton.bounds.X + 25, _changeDirectionButton.bounds.Y + 5), Game1.textColor);

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