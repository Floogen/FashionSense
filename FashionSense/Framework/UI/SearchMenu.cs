using FashionSense.Framework.Managers;
using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Utilities;
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

namespace FashionSense.Framework.UI
{
    internal class SearchMenu : IClickableMenu
    {
        public ClickableTextureComponent forwardButton;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent queryButton;

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

        private string _appearanceFilter;
        private Farmer _displayFarmer;
        private HandMirrorMenu _callbackMenu;

        public SearchMenu(Farmer who, string appearanceFilter, HandMirrorMenu callbackMenu) : base(0, 0, 832, 576, showUpperRightCloseButton: true)
        {
            // Set up menu structure
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                base.height += 64;
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

                    if (callbackMenu.GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                    {
                        appearancePacks = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShoesContentPack).ToList();
                    }
                    break;
            }

            for (int m = 0; m < appearancePacks.Count; m++)
            {
                filteredTextureOptions.Add(appearancePacks[m]);
                cachedTextureOptions.Add(appearancePacks[m]);
            }

            _displayFarmer = who;
            _appearanceFilter = appearanceFilter;
            _callbackMenu = callbackMenu;

            var drawingScale = 4f;
            var widthOffsetScale = 3;
            var sourceRect = new Rectangle(0, 0, Game1.daybg.Width, Game1.daybg.Height);

            for (int r = 0; r < _maxRows; r++)
            {
                for (int c = 0; c < _texturesPerRow; c++)
                {
                    var componentId = c + r * _texturesPerRow;
                    availableTextures.Add(new ClickableTextureComponent(new Rectangle(32 + base.xPositionOnScreen + IClickableMenu.borderWidth + componentId % _texturesPerRow * 64 * widthOffsetScale, base.yPositionOnScreen + sourceRect.Height / 2 + componentId + (r * sourceRect.Height) + (r > 0 ? 64 : 0) - 32, sourceRect.Width, sourceRect.Height), null, new Rectangle(), drawingScale, false)
                    {
                        myID = componentId,
                        downNeighborID = componentId + _texturesPerRow,
                        upNeighborID = r >= _texturesPerRow ? componentId - _texturesPerRow : -1,
                        rightNeighborID = c == 5 ? 9997 : componentId + 1,
                        leftNeighborID = c > 0 ? componentId - 1 : 9998
                    });

                    var fakeFarmer = _displayFarmer.CreateFakeEventFarmer();
                    fakeFarmer.faceDirection(who.facingDirection);
                    foreach (var key in _displayFarmer.modData.Keys)
                    {
                        fakeFarmer.modData[key] = _displayFarmer.modData[key];
                    }

                    List<string> accessoryIds = JsonConvert.DeserializeObject<List<string>>(fakeFarmer.modData[ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID]);
                    List<string> accessoryColors = JsonConvert.DeserializeObject<List<string>>(fakeFarmer.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLLECTIVE_COLOR]);
                    if (accessoryIds is not null && accessoryColors is not null)
                    {
                        FashionSense.accessoryManager.SetAccessories(fakeFarmer, accessoryIds, accessoryColors);
                    }

                    fakeFarmers.Add(fakeFarmer);
                }
            }
            UpdateDisplayFarmers();

            backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = 9998,
                rightNeighborID = 0
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
                X = xTextbox,
                Y = yTextbox,
                Width = 384,
                limitWidth = false,
                Text = String.Empty
            };

            _searchBoxCC = new ClickableComponent(new Rectangle(xTextbox, yTextbox, 192, 48), "")
            {
                myID = 9999,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            Game1.keyboardDispatcher.Subscriber = _searchBox;
            _searchBox.Selected = true;

            queryButton = new ClickableTextureComponent(new Rectangle(xTextbox - 32, base.yPositionOnScreen - 48, 48, 44), Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 2f)
            {
                myID = -1
            };

            // Call snap functions
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                snapToDefaultClickableComponent();
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
                            FashionSense.accessoryManager.AddAccessory(fakeFarmers[i], targetPack.Id, _callbackMenu.GetAccessoryIndex());
                            FashionSense.ResetAnimationModDataFields(fakeFarmers[i], 0, AnimationModel.Type.Idle, fakeFarmers[i].facingDirection);
                            FashionSense.SetSpriteDirty();
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
                            if (_callbackMenu.GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                            {
                                modDataKey = ModDataKeys.CUSTOM_SHOES_ID;
                            }
                            break;
                    }

                    fakeFarmers[i].modData[modDataKey] = targetPack.Id;
                    FashionSense.ResetAnimationModDataFields(fakeFarmers[i], 0, AnimationModel.Type.Idle, fakeFarmers[i].facingDirection);
                    FashionSense.SetSpriteDirty();
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            _hoverText = String.Empty;
            if (Game1.IsFading())
            {
                return;
            }

            for (int i = 0; i < availableTextures.Count; i++)
            {
                var textureIndex = i + _startingRow * _texturesPerRow;
                if (textureIndex < filteredTextureOptions.Count && availableTextures[i].containsPoint(x, y))
                {
                    var targetPack = filteredTextureOptions[textureIndex];

                    _hoverText = $"{targetPack.Name}\nBy: {targetPack.Author}";
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
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (_searchBox.Text != _cachedTextBoxValue)
            {
                _startingRow = 0;
                _cachedTextBoxValue = _searchBox.Text;

                if (String.IsNullOrEmpty(_searchBox.Text))
                {
                    filteredTextureOptions = cachedTextureOptions;
                }
                else
                {
                    filteredTextureOptions = cachedTextureOptions.Where(p => p.Name.Contains(_searchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                UpdateDisplayFarmers();
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
                ClickableTextureComponent c = availableTextures[i];
                var textureIndex = i + _startingRow * _texturesPerRow;
                if (textureIndex < filteredTextureOptions.Count && c.containsPoint(x, y))
                {
                    foreach (var key in fakeFarmers[i].modData.Keys)
                    {
                        _displayFarmer.modData[key] = fakeFarmers[i].modData[key];
                    }
                    FashionSense.ResetAnimationModDataFields(_displayFarmer, 0, AnimationModel.Type.Idle, _displayFarmer.facingDirection);
                    FashionSense.SetSpriteDirty();
                    base.exitThisMenu();
                    Game1.activeClickableMenu = _callbackMenu;
                    return;
                }
            }

            if (_startingRow > 0 && backButton.containsPoint(x, y))
            {
                _startingRow--;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
                return;
            }
            if ((_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count && forwardButton.containsPoint(x, y))
            {
                _startingRow++;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
                return;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && _startingRow > 0)
            {
                _startingRow--;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && (_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count)
            {
                _startingRow++;
                UpdateDisplayFarmers();
                Game1.playSound("shiny4");
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.dialogueUp && !Game1.IsFading())
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                SpriteText.drawStringWithScrollCenteredAt(b, _title, base.xPositionOnScreen + base.width / 4, base.yPositionOnScreen - 64);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

                for (int i = 0; i < availableTextures.Count; i++)
                {
                    availableTextures[i].item = null;
                    availableTextures[i].texture = null;

                    var textureIndex = i + _startingRow * _texturesPerRow;
                    if (textureIndex < filteredTextureOptions.Count)
                    {
                        var targetPack = filteredTextureOptions[textureIndex];

                        // Farmer portrait
                        b.Draw(Game1.daybg, new Vector2(availableTextures[i].bounds.X, availableTextures[i].bounds.Y), Color.White);
                        FarmerRenderer.isDrawingForUI = true;
                        var fakeFarmer = fakeFarmers[i];
                        fakeFarmer.FarmerRenderer.draw(b, fakeFarmer.FarmerSprite.CurrentAnimationFrame, fakeFarmer.FarmerSprite.CurrentFrame, fakeFarmer.FarmerSprite.SourceRect, new Vector2(availableTextures[i].bounds.Center.X - 32, availableTextures[i].bounds.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, fakeFarmer);

                        //fakeFarmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Rectangle(0, 0, 16, 32), new Vector2(availableTextures[i].bounds.Center.X - 32, availableTextures[i].bounds.Bottom - 160), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, fakeFarmer);
                        FarmerRenderer.isDrawingForUI = false;
                    }
                }

                _searchBox.Draw(b);
                queryButton.draw(b);
            }

            if (_startingRow > 0)
            {
                backButton.draw(b);
            }
            if ((_maxRows + _startingRow) * _texturesPerRow < filteredTextureOptions.Count)
            {
                forwardButton.draw(b);
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