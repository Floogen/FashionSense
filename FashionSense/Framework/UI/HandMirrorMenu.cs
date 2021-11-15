using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Models.Hat;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class HandMirrorMenu : IClickableMenu
    {
        private Rectangle _portraitBox;
        private Farmer _displayFarmer;
        private string hoverText = "";

        internal const string ACCESSORY_FILTER_BUTTON = "AccessoryFilter";
        internal const string HAIR_FILTER_BUTTON = "HairFilter";
        internal const string HAT_FILTER_BUTTON = "HatFilter";

        internal const string FIRST_OPTION_BUTTON = "FirstOption";
        internal const string SECOND_OPTION_BUTTON = "SecondOption";
        internal const string THIRD_OPTION_BUTTON = "ThirdOption";

        private ClickableComponent descriptionLabel;
        private ClickableComponent appearanceLabel;
        private ClickableComponent colorLabel;
        private ClickableComponent contentPackLabel;

        public List<ClickableComponent> labels = new List<ClickableComponent>();
        public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> filterButtons = new List<ClickableComponent>();
        public List<ClickableComponent> optionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

        public ColorPicker colorPicker;
        private ClickableTextureComponent clearButton;
        private ClickableTextureComponent searchButton;
        public ClickableTextureComponent okButton;

        public HandMirrorMenu() : base(0, 0, 375, 550, showUpperRightCloseButton: true)
        {
            // Set up menu structure
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                base.height += 64;
            }

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            // Set up portrait and farmer
            topLeft = Utility.getTopLeftPositionForCenteringOnScreen(128, 192);
            _portraitBox = new Rectangle((int)topLeft.X, base.yPositionOnScreen + 64, 128, 192);
            _displayFarmer = Game1.player;

            // Add author label
            var contentPackName = "Content Pack's Name";
            labels.Add(contentPackLabel = new ClickableComponent(new Rectangle((int)(_portraitBox.X - Game1.smallFont.MeasureString(contentPackName).X / 2) + 64, base.yPositionOnScreen + 32, 1, 1), contentPackName));

            // Add appearance-related buttons
            int yOffset = 160;
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 32, _portraitBox.Y + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 520,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.Right - 32, _portraitBox.Y + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 521,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            yOffset += 64;
            leftSelectionButtons.Add(new ClickableTextureComponent("Appearance", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 514,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 32, 1, 1), FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hair")));
            rightSelectionButtons.Add(new ClickableTextureComponent("Appearance", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 515,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            }); ;
            appearanceLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset + 64, 1, 1), String.Empty);

            // Get the last selected filter button
            var lastSelectedFilter = HAIR_FILTER_BUTTON;
            if (Game1.player.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON) && !String.IsNullOrEmpty(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON]))
            {
                lastSelectedFilter = Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON];
            }

            // Add all the relevant filter buttons
            filterButtons.Add(new ClickableTextureComponent(HAIR_FILTER_BUTTON, new Rectangle(base.xPositionOnScreen + 50, base.yPositionOnScreen + 70, 64, 64), null, lastSelectedFilter == HAIR_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.scissorsButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 601,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(ACCESSORY_FILTER_BUTTON, new Rectangle(base.xPositionOnScreen + 50, base.yPositionOnScreen + 125, 64, 64), null, lastSelectedFilter == ACCESSORY_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.accessoryButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 601,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(HAT_FILTER_BUTTON, new Rectangle(base.xPositionOnScreen + 50, base.yPositionOnScreen + 180, 64, 64), null, lastSelectedFilter == HAT_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.hatButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 601,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            // Add the option buttons (currently only for accessories)
            optionButtons.Add(new ClickableTextureComponent(FIRST_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 134, _portraitBox.Y + yOffset, 32, 32), null, "enabled", FashionSense.assetManager.optionOneButton, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 601,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            optionButtons.Add(new ClickableTextureComponent(SECOND_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 86, _portraitBox.Y + yOffset, 32, 32), null, "disabled", FashionSense.assetManager.optionTwoButton, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 601,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            optionButtons.Add(new ClickableTextureComponent(THIRD_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 38, _portraitBox.Y + yOffset, 32, 32), null, "disabled", FashionSense.assetManager.optionThreeButton, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 601,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            // Add the left over buttons
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 32, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 32, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 505,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };

            searchButton = new ClickableTextureComponent("Search", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder + 8, base.yPositionOnScreen + 26, 32, 32), null, null, Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 2f)
            {
                myID = 701,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            clearButton = new ClickableTextureComponent("Clear", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder + 8, base.yPositionOnScreen + 70, 32, 32), null, null, Game1.mouseCursors, new Rectangle(323, 433, 9, 10), 3f)
            {
                myID = 701,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };

            // Add color picker
            yOffset += 48;
            var measuredStringSize = Game1.smallFont.MeasureString(GetColorPickerLabel(true));
            labels.Add(colorLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, (int)measuredStringSize.X, (int)measuredStringSize.Y), GetColorPickerLabel(false)));
            yOffset += 32;
            var top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
            colorPicker = new ColorPicker("Hair", top.X, top.Y);

            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    colorPicker.setColor(Game1.player.hairstyleColor);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    var accessoryColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR]) };
                    colorPicker.setColor(accessoryColor);
                    break;
                case HAT_FILTER_BUTTON:
                    var hatColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) };
                    colorPicker.setColor(hatColor);
                    break;
            }
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
            {
                myID = 525,
                downNeighborID = -99998,
                upNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
            {
                myID = 526,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
            {
                myID = 527,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
        }

        internal static string GetColorPickerLabel(bool isDisabled = false, bool isCompact = false, string enabledFilterName = null)
        {
            string labelName = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.hair");

            if (!String.IsNullOrEmpty(enabledFilterName))
            {
                switch (enabledFilterName)
                {
                    case ACCESSORY_FILTER_BUTTON:
                        labelName = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.accessory");
                        break;
                    case HAT_FILTER_BUTTON:
                        labelName = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.hat");
                        break;
                }
            }


            if (isDisabled)
            {
                var separator = isCompact ? "\n" : " ";
                labelName += $"{separator}{FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_disabled.generic")}";
            }

            return $"{labelName}:";
        }

        internal void SetFilter(string filterName, AppearanceContentPack appearancePack)
        {
            var enabledButton = filterButtons.FirstOrDefault(b => (b as ClickableTextureComponent).hoverText == "enabled");
            if (enabledButton != null)
            {
                (enabledButton as ClickableTextureComponent).hoverText = "disabled";
            }

            ClickableTextureComponent filterButton = null;
            switch (filterName)
            {
                case HAIR_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAIR_FILTER_BUTTON;

                    colorPicker.setColor(Game1.player.hairstyleColor);

                    filterButton = filterButtons.First(b => b.name == HAIR_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;

                    var accessoryColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR]) };
                    colorPicker.setColor(accessoryColor);

                    filterButton = filterButtons.First(b => b.name == ACCESSORY_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case HAT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAT_FILTER_BUTTON;

                    var hatColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) };
                    colorPicker.setColor(hatColor);

                    filterButton = filterButtons.First(b => b.name == HAT_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
            }
            filterButton.hoverText = "enabled";

            // Get the first AppearanceContentPack that matches the packName
            List<AppearanceContentPack> appearanceModels = new List<AppearanceContentPack>();
            switch (filterName)
            {
                case HAIR_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HairContentPack).ToList();
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is AccessoryContentPack).ToList();
                    break;
                case HAT_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HatContentPack).ToList();
                    break;
            }

            UpdateAppearance(appearanceModels.IndexOf(appearancePack), true);
        }

        private string GetNameOfEnabledFilter()
        {
            var enabledButton = filterButtons.FirstOrDefault(f => (f as ClickableTextureComponent).hoverText == "enabled");
            if (enabledButton is null)
            {
                return null;
            }

            return enabledButton.name;
        }

        private string GetCurrentAccessorySlotKey()
        {
            var enabledButton = optionButtons.FirstOrDefault(f => (f as ClickableTextureComponent).hoverText == "enabled");
            if (enabledButton is null)
            {
                return null;
            }

            switch (enabledButton.name)
            {
                case SECOND_OPTION_BUTTON:
                    return ModDataKeys.CUSTOM_ACCESSORY_SECONDARY_ID;
                case THIRD_OPTION_BUTTON:
                    return ModDataKeys.CUSTOM_ACCESSORY_TERTIARY_ID;
                default:
                    return ModDataKeys.CUSTOM_ACCESSORY_ID;
            }
        }

        private void UpdateAppearance(int change, bool overrideIndex = false)
        {
            string modDataKey = null;
            AppearanceContentPack currentAppearance = null;
            List<AppearanceContentPack> appearanceModels = new List<AppearanceContentPack>();
            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HairContentPack).ToList();
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    modDataKey = GetCurrentAccessorySlotKey();
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is AccessoryContentPack).ToList();
                    break;
                case HAT_FILTER_BUTTON:
                    modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HatContentPack).ToList();
                    break;
            }

            int current_index = -1;
            if (currentAppearance != null)
            {
                current_index = appearanceModels.IndexOf(currentAppearance);
            }

            current_index = overrideIndex ? change : current_index + change;
            if (current_index >= appearanceModels.Count)
            {
                current_index = -1;
            }
            else if (current_index < -1)
            {
                current_index = appearanceModels.Count() - 1;
            }

            Game1.player.modData[modDataKey] = current_index == -1 ? "None" : appearanceModels[current_index].Id;
            FashionSense.ResetAnimationModDataFields(Game1.player, 0, AnimationModel.Type.Idle, Game1.player.facingDirection);
            Game1.playSound("grassyStep");
        }

        private void selectionClick(string name, int change)
        {
            switch (name)
            {
                case "Appearance":
                    {
                        UpdateAppearance(change);
                        break;
                    }
                case "Direction":
                    _displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
                    _displayFarmer.FarmerSprite.StopAnimation();
                    _displayFarmer.completelyStopAnimatingOrDoingAction();
                    Game1.playSound("pickUpItem");
                    break;
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (leftSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent c2 in leftSelectionButtons)
                {
                    if (c2.containsPoint(x, y))
                    {
                        selectionClick(c2.name, -1);
                        if (c2.scale != 0f)
                        {
                            c2.scale -= 0.25f;
                            c2.scale = Math.Max(0.75f, c2.scale);
                        }
                    }
                }
            }

            if (rightSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent c in rightSelectionButtons)
                {
                    if (c.containsPoint(x, y))
                    {
                        selectionClick(c.name, 1);
                        if (c.scale != 0f)
                        {
                            c.scale -= 0.25f;
                            c.scale = Math.Max(0.75f, c.scale);
                        }
                    }
                }
            }

            foreach (ClickableTextureComponent c in filterButtons)
            {
                if (c.containsPoint(x, y) && c.hoverText != "enabled")
                {
                    var enabledButton = filterButtons.FirstOrDefault(b => (b as ClickableTextureComponent).hoverText == "enabled");
                    if (enabledButton != null)
                    {
                        (enabledButton as ClickableTextureComponent).hoverText = "disabled";
                    }

                    c.hoverText = "enabled";
                    switch (c.name)
                    {
                        case HAIR_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAIR_FILTER_BUTTON;

                            colorPicker.setColor(Game1.player.hairstyleColor);
                            break;
                        case ACCESSORY_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;

                            var accessoryColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR]) };
                            colorPicker.setColor(accessoryColor);
                            break;
                        case HAT_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAT_FILTER_BUTTON;

                            var hatColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) };
                            colorPicker.setColor(hatColor);
                            break;
                    }

                    if (c.scale != 0f)
                    {
                        c.scale -= 0.25f;
                        c.scale = Math.Max(2.75f, c.scale);
                    }
                }
            }

            foreach (ClickableTextureComponent c in optionButtons)
            {
                if (c.containsPoint(x, y) && c.hoverText != "enabled")
                {
                    var enabledButton = optionButtons.FirstOrDefault(b => (b as ClickableTextureComponent).hoverText == "enabled");
                    if (enabledButton != null)
                    {
                        (enabledButton as ClickableTextureComponent).hoverText = "disabled";
                    }

                    c.hoverText = "enabled";
                    if (c.scale != 0f)
                    {
                        c.scale -= 0.25f;
                        c.scale = Math.Max(1.75f, c.scale);
                    }
                }
            }

            if (colorPicker != null && colorPicker.containsPoint(x, y))
            {
                Color color2 = colorPicker.click(x, y);
                switch (GetNameOfEnabledFilter())
                {
                    case HAIR_FILTER_BUTTON:
                        Game1.player.changeHairColor(color2);
                        break;
                    case ACCESSORY_FILTER_BUTTON:
                        Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_ACCESSORY_COLOR] = color2.PackedValue.ToString();
                        break;
                    case HAT_FILTER_BUTTON:
                        Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR] = color2.PackedValue.ToString();
                        break;
                }
            }

            if (searchButton.containsPoint(x, y))
            {
                Game1.activeClickableMenu = new FilterMenu(GetNameOfEnabledFilter(), this);
            }

            if (clearButton.containsPoint(x, y))
            {
                string modDataKey = String.Empty;
                switch (GetNameOfEnabledFilter())
                {
                    case HAIR_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                        break;
                    case ACCESSORY_FILTER_BUTTON:
                        modDataKey = GetCurrentAccessorySlotKey();
                        break;
                    case HAT_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                        break;
                }

                Game1.player.modData[modDataKey] = "None";
            }

            if (okButton.containsPoint(x, y))
            {
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
                exitThisMenu();
                Game1.playSound("coin");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";
            foreach (ClickableTextureComponent c6 in leftSelectionButtons)
            {
                if (c6.containsPoint(x, y))
                {
                    c6.scale = Math.Min(c6.scale + 0.02f, c6.baseScale + 0.1f);
                }
                else
                {
                    c6.scale = Math.Max(c6.scale - 0.02f, c6.baseScale);
                }
            }
            foreach (ClickableTextureComponent c5 in rightSelectionButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    c5.scale = Math.Min(c5.scale + 0.02f, c5.baseScale + 0.1f);
                }
                else
                {
                    c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
                }
            }
            foreach (ClickableTextureComponent c5 in filterButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    if (c5.hoverText == "disabled")
                    {
                        c5.scale = Math.Min(c5.scale + 0.02f, c5.baseScale + 0.1f);
                    }

                    switch (c5.name)
                    {
                        case HAIR_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hair");
                            break;
                        case ACCESSORY_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.accessory");
                            break;
                        case HAT_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hat");
                            break;
                        default:
                            continue;
                    }
                }
                else
                {
                    c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
                }
            }
            foreach (ClickableTextureComponent c5 in optionButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    if (c5.hoverText == "disabled")
                    {
                        c5.scale = Math.Min(c5.scale + 0.02f, 2.2f);
                    }
                }
                else
                {
                    c5.scale = Math.Max(c5.scale - 0.02f, 2f);
                }
            }
            foreach (ClickableComponent label in labels)
            {
                if (label == colorLabel && label.name == GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter()) && colorLabel.containsPoint(x, y))
                {
                    switch (GetNameOfEnabledFilter())
                    {
                        case HAIR_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_info.hair");
                            break;
                        case ACCESSORY_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_info.accessory");
                            break;
                        case HAT_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_info.hat");
                            break;
                    }
                }
            }

            if (contentPackLabel.containsPoint(x, y))
            {
                AppearanceContentPack contentPack = null;
                switch (GetNameOfEnabledFilter())
                {
                    case HAIR_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID]);
                        break;
                    case ACCESSORY_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]);
                        break;
                    case HAT_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAT_ID]);
                        break;
                }

                if (contentPack != null)
                {
                    hoverText = $"{contentPack.PackName} by {contentPack.Author}";
                }
            }

            if (appearanceLabel.containsPoint(x, y))
            {
                AppearanceContentPack contentPack = null;
                switch (GetNameOfEnabledFilter())
                {
                    case HAIR_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID]);
                        break;
                    case ACCESSORY_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_ACCESSORY_ID]);
                        break;
                    case HAT_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAT_ID]);
                        break;
                }

                if (contentPack != null)
                {
                    hoverText = contentPack.Name;
                }
            }

            if (searchButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.search_button");
                searchButton.scale = Math.Min(searchButton.scale + 0.02f, searchButton.baseScale + 0.1f);
            }
            else
            {
                searchButton.scale = Math.Max(searchButton.scale - 0.02f, searchButton.baseScale);
            }

            if (clearButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.clear_button");
                clearButton.scale = Math.Min(clearButton.scale + 0.02f, 3.2f);
            }
            else
            {
                clearButton.scale = Math.Max(clearButton.scale - 0.02f, clearButton.baseScale);
            }

            if (okButton.containsPoint(x, y))
            {
                okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
            }
            else
            {
                okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }

            // Get the custom hair object, if it exists
            AppearanceContentPack contentPack = null;
            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID]);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(Game1.player.modData[GetCurrentAccessorySlotKey()]);
                    break;
                case HAT_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAT_ID]);
                    break;
            }

            // General UI (title, background)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, FashionSense.modHelper.Translation.Get("tools.name.hand_mirror"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Farmer portrait
            b.Draw(Game1.daybg, new Vector2(_portraitBox.X, _portraitBox.Y), Color.White);
            FarmerRenderer.isDrawingForUI = true;
            _displayFarmer.FarmerRenderer.draw(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(_portraitBox.Center.X - 32, _portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, _displayFarmer);
            FarmerRenderer.isDrawingForUI = false;

            // Draw buttons
            foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
            {
                leftSelectionButton.draw(b);
            }
            foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
            {
                rightSelectionButton.draw(b);
            }
            foreach (ClickableTextureComponent filterButton in filterButtons)
            {
                filterButton.draw(b, filterButton.hoverText == "enabled" ? Color.White : Color.Gray, 1f);
            }

            // Draw option button for supported appearance types (currently only accessories)
            if (GetNameOfEnabledFilter() == ACCESSORY_FILTER_BUTTON)
            {
                foreach (ClickableTextureComponent optionButton in optionButtons)
                {
                    optionButton.draw(b, optionButton.hoverText == "enabled" ? Color.White : Color.Gray, 1f);
                }
            }
            searchButton.draw(b);
            clearButton.draw(b);
            okButton.draw(b);

            // Draw labels
            foreach (ClickableComponent c in labels)
            {
                if (!c.visible)
                {
                    continue;
                }

                float offset = 0f;
                Color color = Game1.textColor;
                if (c == descriptionLabel)
                {
                    // Update display name, if needed
                    var descriptionName = Game1.player.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON) ? Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] : String.Empty;
                    switch (descriptionName)
                    {
                        case ACCESSORY_FILTER_BUTTON:
                            descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.accessory");
                            break;
                        case HAT_FILTER_BUTTON:
                            descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hat");
                            break;
                        default:
                            descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hair");
                            break;
                    }

                    offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
                    if (!c.name.Contains("Color"))
                    {
                        appearanceLabel.name = "None";
                        if (contentPack != null)
                        {
                            appearanceLabel.name = contentPack.Name;
                            if (appearanceLabel.name.Length > 21)
                            {
                                appearanceLabel.name = $"{appearanceLabel.name.Substring(0, 21).TrimEnd()}...";
                            }
                        }

                        var labelMeasured = Game1.smallFont.MeasureString(appearanceLabel.name);
                        appearanceLabel.bounds.Width = (int)labelMeasured.X;
                        appearanceLabel.bounds.Height = (int)labelMeasured.Y;
                    }
                }
                else if (c == contentPackLabel)
                {
                    contentPackLabel.name = "";
                    if (contentPack != null)
                    {
                        contentPackLabel.name = contentPack.PackName;
                    }

                    if (contentPackLabel.name.Length > 21)
                    {
                        contentPackLabel.name = $"{contentPackLabel.name.Substring(0, 21).TrimEnd()}...";
                    }

                    var labelMeasured = Game1.smallFont.MeasureString(contentPackLabel.name);
                    contentPackLabel.bounds.X = (int)(_portraitBox.X - labelMeasured.X / 2) + 64;
                    contentPackLabel.bounds.Width = (int)labelMeasured.X;
                    contentPackLabel.bounds.Height = (int)labelMeasured.Y;
                }
                else if (c == colorLabel)
                {
                    var name = GetColorPickerLabel(false, enabledFilterName: GetNameOfEnabledFilter());
                    if (contentPack != null)
                    {
                        if (contentPack is HairContentPack hairPack && hairPack.GetHairFromFacingDirection(Game1.player.facingDirection) is HairModel hModel && hModel != null && hModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is AccessoryContentPack accessoryPack && accessoryPack.GetAccessoryFromFacingDirection(Game1.player.facingDirection) is AccessoryModel aModel && aModel != null && aModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is HatContentPack hatPack && hatPack.GetHatFromFacingDirection(Game1.player.facingDirection) is HatModel htModel && htModel != null && htModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                    }

                    colorLabel.name = name;
                }
                else
                {
                    color = Game1.textColor;
                }
                Utility.drawTextWithShadow(b, c.name, Game1.smallFont, new Vector2((float)c.bounds.X + offset, c.bounds.Y), color);
            }

            if (appearanceLabel.name.Length > 0)
            {
                Utility.drawTextWithShadow(b, appearanceLabel.name, Game1.smallFont, new Vector2((appearanceLabel.bounds.X + 21) - Game1.smallFont.MeasureString(appearanceLabel.name).X / 2f, appearanceLabel.bounds.Y + 5f), Game1.textColor);
            }

            // Draw color selector
            if (colorPicker != null)
            {
                colorPicker.draw(b);
            }

            // Draw hover text
            if (!hoverText.Equals(""))
            {
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}
