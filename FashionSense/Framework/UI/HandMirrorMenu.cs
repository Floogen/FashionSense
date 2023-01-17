using FashionSense.Framework.Models;
using FashionSense.Framework.Models.Accessory;
using FashionSense.Framework.Models.Hair;
using FashionSense.Framework.Models.Hat;
using FashionSense.Framework.Models.Pants;
using FashionSense.Framework.Models.Shirt;
using FashionSense.Framework.Models.Shoes;
using FashionSense.Framework.Models.Sleeves;
using FashionSense.Framework.UI.Components;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
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
        private Color? _cachedColor;
        private Farmer _displayFarmer;
        private string hoverText = "";
        private int colorPickerTimer;
        private int currentAccessorySlot;

        internal const string ACCESSORY_FILTER_BUTTON = "AccessoryFilter";
        internal const string HAIR_FILTER_BUTTON = "HairFilter";
        internal const string HAT_FILTER_BUTTON = "HatFilter";
        internal const string SHIRT_FILTER_BUTTON = "ShirtFilter";
        internal const string PANTS_FILTER_BUTTON = "PantsFilter";
        internal const string SLEEVES_FILTER_BUTTON = "SleevesFilter";

        internal const string FIRST_OPTION_BUTTON = "FirstOption";
        internal const string SECOND_OPTION_BUTTON = "SecondOption";
        internal const string THIRD_OPTION_BUTTON = "ThirdOption";
        internal const string SLEEVES_OPTION_BUTTON = "SleevesOption";
        internal const string SHOES_OPTION_BUTTON = "ShoesOption";

        internal const string LIMIT_TO_ACCCESSORIES = "LimitedToAccessories";

        private ClickableComponent descriptionLabel;
        private ClickableComponent appearanceLabel;
        private ClickableComponent colorLabel;
        private ClickableComponent contentPackLabel;
        private ClickableComponent accessorySlotLabel;

        public List<ClickableComponent> labels = new List<ClickableComponent>();
        public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> filterButtons = new List<ClickableComponent>();
        public List<ClickableComponent> optionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> featureButtons = new List<ClickableComponent>();
        public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

        public SimpleColorPicker colorPicker;
        private ClickableTextureComponent randomButton;
        private ClickableTextureComponent clearButton;
        private ClickableTextureComponent searchButton;
        private ClickableTextureComponent outfitButton;
        private ClickableTextureComponent colorCopyButton;
        private ClickableTextureComponent colorPasteButton;
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
            leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.X - 32, _portraitBox.Y + yOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 520,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.Right - 32, _portraitBox.Y + yOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 521,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            leftSelectionButtons.Add(new ClickableTextureComponent(LIMIT_TO_ACCCESSORIES, new Rectangle(_portraitBox.X + 8, _portraitBox.Y + yOffset + 60, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 0.5f)
            {
                myID = 520,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            rightSelectionButtons.Add(new ClickableTextureComponent(LIMIT_TO_ACCCESSORIES, new Rectangle(_portraitBox.Right - 40, _portraitBox.Y + yOffset + 60, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 0.5f)
            {
                myID = 521,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            yOffset += 64;
            leftSelectionButtons.Add(new ClickableTextureComponent("Appearance", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 514,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 84, _portraitBox.Y + yOffset + 32, 1, 1), FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hair")));
            rightSelectionButtons.Add(new ClickableTextureComponent("Appearance", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
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
                myID = 602,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(HAT_FILTER_BUTTON, new Rectangle(base.xPositionOnScreen + 50, base.yPositionOnScreen + 180, 64, 64), null, lastSelectedFilter == HAT_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.hatButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 603,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(SHIRT_FILTER_BUTTON, new Rectangle(_portraitBox.Right + 25, base.yPositionOnScreen + 70, 64, 64), null, lastSelectedFilter == SHIRT_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.shirtButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 604,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(PANTS_FILTER_BUTTON, new Rectangle(_portraitBox.Right + 25, base.yPositionOnScreen + 125, 64, 64), null, lastSelectedFilter == PANTS_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.pantsButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 605,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(SLEEVES_FILTER_BUTTON, new Rectangle(_portraitBox.Right + 25, base.yPositionOnScreen + 180, 64, 64), null, lastSelectedFilter == SLEEVES_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.sleevesAndShoesButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 605,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            labels.Add(new ClickableComponent(new Rectangle(_portraitBox.Right - 100, _portraitBox.Y + yOffset - 32, 1, 1), "Acc. #", LIMIT_TO_ACCCESSORIES));
            labels.Add(accessorySlotLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 72, _portraitBox.Y + yOffset - 2, 1, 1), "0", LIMIT_TO_ACCCESSORIES));

            #region Start of obsolete option buttons
            /*
            // Add the option buttons
            optionButtons.Add(new ClickableTextureComponent(FIRST_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 130, _portraitBox.Y + yOffset, 32, 32), null, "enabled", FashionSense.assetManager.optionOneButton, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 611,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            optionButtons.Add(new ClickableTextureComponent(SECOND_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 82, _portraitBox.Y + yOffset, 32, 32), null, "disabled", FashionSense.assetManager.optionTwoButton, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 612,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            optionButtons.Add(new ClickableTextureComponent(THIRD_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 34, _portraitBox.Y + yOffset, 32, 32), null, "disabled", FashionSense.assetManager.optionThreeButton, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 613,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            */
            #endregion

            // Add the feature buttons
            featureButtons.Add(new ClickableTextureComponent(SLEEVES_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 130, _portraitBox.Y + yOffset - 10, 32, 32), null, "enabled", FashionSense.assetManager.sleevesButtonTexture, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 613,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });
            featureButtons.Add(new ClickableTextureComponent(SHOES_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 34, _portraitBox.Y + yOffset - 10, 32, 32), null, "disabled", FashionSense.assetManager.shoesButtonTexture, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 613,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            // Add the leftover buttons
            okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 56, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 20, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 505,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };

            searchButton = new ClickableTextureComponent("Search", new Rectangle(base.xPositionOnScreen + base.width + IClickableMenu.spaceToClearSideBorder - 16, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder - 48, 32, 32), null, null, Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 2f)
            {
                myID = 701,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            outfitButton = new ClickableTextureComponent("Outfits", new Rectangle(searchButton.bounds.X, searchButton.bounds.Y + 48, 32, 32), null, null, Game1.mouseCursors2, new Rectangle(6, 52, 7, 8), 4f)
            {
                myID = 701,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            colorCopyButton = new ClickableTextureComponent("Copy", new Rectangle(searchButton.bounds.X + 3, searchButton.bounds.Y + 96, 32, 32), null, null, Game1.mouseCursors, new Rectangle(278, 288, 5, 6), 4f)
            {
                myID = 701,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            colorPasteButton = new ClickableTextureComponent("Paste", new Rectangle(searchButton.bounds.X + 3, searchButton.bounds.Y + 136, 32, 32), null, null, Game1.mouseCursors, new Rectangle(296, 504, 5, 5), 4f)
            {
                myID = 701,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            randomButton = new ClickableTextureComponent("Randomize", new Rectangle(searchButton.bounds.X, searchButton.bounds.Y + 170, 32, 32), null, null, Game1.mouseCursors, new Rectangle(50, 428, 10, 10), 3f)
            {
                myID = 703,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            clearButton = new ClickableTextureComponent("Clear", new Rectangle(searchButton.bounds.X + 1, searchButton.bounds.Y + 208, 32, 32), null, null, Game1.mouseCursors, new Rectangle(323, 433, 9, 10), 3f)
            {
                myID = 702,
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
            colorPicker = new SimpleColorPicker(top.X, top.Y);

            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    colorPicker.SetColor(Game1.player.hairstyleColor);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    var accessoryData = FashionSense.accessoryManager.GetAccessoryDataByIndex(GetAccessoryIndex());
                    if (accessoryData is not null)
                    {
                        colorPicker.SetColor(accessoryData.Color);
                    }
                    break;
                case HAT_FILTER_BUTTON:
                    var hatColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) };
                    colorPicker.SetColor(hatColor);
                    break;
                case SHIRT_FILTER_BUTTON:
                    var shirtColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR]) };
                    colorPicker.SetColor(shirtColor);
                    break;
                case PANTS_FILTER_BUTTON:
                    var pantsColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR]) };
                    colorPicker.SetColor(pantsColor);
                    break;
                case SLEEVES_FILTER_BUTTON:
                    var appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR]) };
                    switch (GetCurrentFeatureSlotKey())
                    {
                        case ModDataKeys.CUSTOM_SHOES_ID:
                            appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR]) };
                            break;
                    }
                    colorPicker.SetColor(appearanceColor);
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
                    case SHIRT_FILTER_BUTTON:
                        labelName = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.shirt");
                        break;
                    case PANTS_FILTER_BUTTON:
                        labelName = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.pants");
                        break;
                    case SLEEVES_FILTER_BUTTON:
                        labelName = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.sleeves");
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

                    colorPicker.SetColor(Game1.player.hairstyleColor);

                    filterButton = filterButtons.First(b => b.name == HAIR_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;

                    var accessoryData = FashionSense.accessoryManager.GetAccessoryDataByIndex(GetAccessoryIndex());
                    if (accessoryData is not null)
                    {
                        colorPicker.SetColor(accessoryData.Color);
                    }

                    filterButton = filterButtons.First(b => b.name == ACCESSORY_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case HAT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAT_FILTER_BUTTON;

                    var hatColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) };
                    colorPicker.SetColor(hatColor);

                    filterButton = filterButtons.First(b => b.name == HAT_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case SHIRT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SHIRT_FILTER_BUTTON;

                    var shirtColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR]) };
                    colorPicker.SetColor(shirtColor);

                    filterButton = filterButtons.First(b => b.name == SHIRT_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case PANTS_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = PANTS_FILTER_BUTTON;

                    var pantsColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR]) };
                    colorPicker.SetColor(pantsColor);

                    filterButton = filterButtons.First(b => b.name == PANTS_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case SLEEVES_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SLEEVES_FILTER_BUTTON;

                    var appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR]) };
                    switch (GetCurrentFeatureSlotKey())
                    {
                        case ModDataKeys.CUSTOM_SHOES_ID:
                            appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR]) };
                            break;
                    }
                    colorPicker.SetColor(appearanceColor);

                    filterButton = filterButtons.First(b => b.name == SLEEVES_FILTER_BUTTON) as ClickableTextureComponent;
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
                case SHIRT_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShirtContentPack).ToList();
                    break;
                case PANTS_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is PantsContentPack).ToList();
                    break;
                case SLEEVES_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is SleevesContentPack).ToList();
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

        internal string GetCurrentAccessorySlotKey()
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

        internal int GetAccessoryIndex()
        {
            return currentAccessorySlot;
        }

        internal string GetCurrentFeatureSlotKey()
        {
            var enabledButton = featureButtons.FirstOrDefault(f => (f as ClickableTextureComponent).hoverText == "enabled");
            if (enabledButton is null)
            {
                return null;
            }

            switch (enabledButton.name)
            {
                case SHOES_OPTION_BUTTON:
                    return ModDataKeys.CUSTOM_SHOES_ID;
                default:
                    return ModDataKeys.CUSTOM_SLEEVES_ID;
            }
        }

        private void HandleColorPicker(int x, int y, bool held)
        {
            if (!(held ? colorPicker.ClickHeld(x, y) : colorPicker.Click(x, y)))
            {
                return;
            }

            HandleColorPicker();
        }

        private void HandleColorPicker()
        {
            Color color = colorPicker.GetCurrentColor();
            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    Game1.player.changeHairColor(color);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    var accessoryData = FashionSense.accessoryManager.GetAccessoryDataByIndex(GetAccessoryIndex());
                    if (accessoryData is not null)
                    {
                        accessoryData.Color = color;
                    }
                    break;
                case HAT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR] = color.PackedValue.ToString();
                    break;
                case SHIRT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR] = color.PackedValue.ToString();
                    FashionSense.SetSpriteDirty();
                    break;
                case PANTS_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR] = color.PackedValue.ToString();
                    break;
                case SLEEVES_FILTER_BUTTON:
                    var appearanceColorKey = ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR;
                    switch (GetCurrentFeatureSlotKey())
                    {
                        case ModDataKeys.CUSTOM_SHOES_ID:
                            appearanceColorKey = ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR;
                            FashionSense.SetSpriteDirty();
                            break;
                    }
                    Game1.player.modData[appearanceColorKey] = color.PackedValue.ToString();
                    break;
            }
        }

        private void UpdateAppearance(int change, bool overrideIndex = false)
        {
            int accessoryIndex = -1;
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
                    modDataKey = ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID;
                    accessoryIndex = GetAccessoryIndex();

                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(accessoryIndex));
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is AccessoryContentPack).ToList();
                    break;
                case HAT_FILTER_BUTTON:
                    modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HatContentPack).ToList();
                    break;
                case SHIRT_FILTER_BUTTON:
                    modDataKey = ModDataKeys.CUSTOM_SHIRT_ID;
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShirtContentPack).ToList();
                    break;
                case PANTS_FILTER_BUTTON:
                    modDataKey = ModDataKeys.CUSTOM_PANTS_ID;
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is PantsContentPack).ToList();
                    break;
                case SLEEVES_FILTER_BUTTON:
                    if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                    {
                        modDataKey = ModDataKeys.CUSTOM_SHOES_ID;
                        currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(Game1.player.modData[modDataKey]);
                        appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShoesContentPack).ToList();
                        break;
                    }
                    modDataKey = ModDataKeys.CUSTOM_SLEEVES_ID;
                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[modDataKey]);
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is SleevesContentPack).ToList();
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

            string appearanceId = current_index == -1 ? "None" : appearanceModels[current_index].Id;
            if (modDataKey == ModDataKeys.CUSTOM_ACCESSORY_COLLECTIVE_ID)
            {
                FashionSense.accessoryManager.AddAccessory(appearanceId, accessoryIndex);
            }
            else
            {
                Game1.player.modData[modDataKey] = appearanceId;
            }

            FashionSense.ResetAnimationModDataFields(Game1.player, 0, AnimationModel.Type.Idle, Game1.player.facingDirection);
            Game1.playSound("grassyStep");

            FashionSense.SetSpriteDirty();
            FashionSense.ResetTextureIfNecessary(currentAppearance);
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
                case LIMIT_TO_ACCCESSORIES:
                    currentAccessorySlot = currentAccessorySlot + change < 0 ? 0 : currentAccessorySlot + change;
                    accessorySlotLabel.name = currentAccessorySlot.ToString();

                    var accessoryData = FashionSense.accessoryManager.GetAccessoryDataByIndex(currentAccessorySlot);
                    if (accessoryData is not null)
                    {
                        colorPicker.SetColor(accessoryData.Color);
                    }

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

                            colorPicker.SetColor(Game1.player.hairstyleColor);
                            break;
                        case ACCESSORY_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;

                            var accessoryData = FashionSense.accessoryManager.GetAccessoryDataByIndex(GetAccessoryIndex());
                            if (accessoryData is not null)
                            {
                                colorPicker.SetColor(accessoryData.Color);
                            }
                            break;
                        case HAT_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAT_FILTER_BUTTON;

                            var hatColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_HAT_COLOR]) };
                            colorPicker.SetColor(hatColor);
                            break;
                        case SHIRT_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SHIRT_FILTER_BUTTON;

                            var shirtColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHIRT_COLOR]) };
                            colorPicker.SetColor(shirtColor);
                            break;
                        case PANTS_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = PANTS_FILTER_BUTTON;

                            var pantsColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_PANTS_COLOR]) };
                            colorPicker.SetColor(pantsColor);
                            break;
                        case SLEEVES_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SLEEVES_FILTER_BUTTON;

                            var appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR]) };
                            switch (GetCurrentFeatureSlotKey())
                            {
                                case ModDataKeys.CUSTOM_SHOES_ID:
                                    appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR]) };
                                    break;
                            }
                            colorPicker.SetColor(appearanceColor);
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
                if (c.containsPoint(x, y) && c.hoverText != "enabled" && GetNameOfEnabledFilter() == ACCESSORY_FILTER_BUTTON)
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

                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;

                    var accessoryData = FashionSense.accessoryManager.GetAccessoryDataByIndex(GetAccessoryIndex());
                    if (accessoryData is not null)
                    {
                        colorPicker.SetColor(accessoryData.Color);
                    }
                }
            }

            foreach (ClickableTextureComponent c in featureButtons)
            {
                if (c.containsPoint(x, y) && c.hoverText != "enabled" && GetNameOfEnabledFilter() == SLEEVES_FILTER_BUTTON)
                {
                    var enabledButton = featureButtons.FirstOrDefault(b => (b as ClickableTextureComponent).hoverText == "enabled");
                    if (enabledButton != null)
                    {
                        (enabledButton as ClickableTextureComponent).hoverText = "disabled";
                    }

                    c.hoverText = "enabled";
                    if (c.scale != 0f)
                    {
                        c.scale -= 0.25f;
                        c.scale = Math.Max(2f, c.scale);
                    }

                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SLEEVES_FILTER_BUTTON;

                    var appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SLEEVES_COLOR]) };
                    switch (GetCurrentFeatureSlotKey())
                    {
                        case ModDataKeys.CUSTOM_SHOES_ID:
                            appearanceColor = new Color() { PackedValue = uint.Parse(Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_SHOES_COLOR]) };
                            break;
                    }
                    colorPicker.SetColor(appearanceColor);
                }
            }

            if (colorPicker != null)
            {
                HandleColorPicker(x, y, held: false);
            }

            if (searchButton.containsPoint(x, y))
            {
                Game1.activeClickableMenu = new SearchMenu(Game1.player, GetNameOfEnabledFilter(), this);
            }

            if (outfitButton.containsPoint(x, y))
            {
                Game1.activeClickableMenu = new OutfitsMenu(this);
            }

            if (colorCopyButton.containsPoint(x, y))
            {
                _cachedColor = colorPicker.GetCurrentColor();
            }

            if (colorPasteButton.containsPoint(x, y) && _cachedColor is not null)
            {
                colorPicker.SetColor(_cachedColor.Value);
                HandleColorPicker();
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
                        FashionSense.accessoryManager.RemoveAccessory(currentAccessorySlot);
                        FashionSense.SetSpriteDirty();
                        return;
                    case HAT_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                        break;
                    case SHIRT_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_SHIRT_ID;
                        break;
                    case PANTS_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_PANTS_ID;
                        break;
                    case SLEEVES_FILTER_BUTTON:
                        modDataKey = GetCurrentFeatureSlotKey();
                        break;
                }

                Game1.player.modData[modDataKey] = "None";
                FashionSense.SetSpriteDirty();
            }

            if (randomButton.containsPoint(x, y))
            {
                string modDataKey = String.Empty;
                AppearanceContentPack randomContentPack = null;
                switch (GetNameOfEnabledFilter())
                {
                    case HAIR_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                        randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<HairContentPack>();
                        break;
                    case ACCESSORY_FILTER_BUTTON:
                        modDataKey = GetCurrentAccessorySlotKey();
                        randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<AccessoryContentPack>();
                        break;
                    case HAT_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_HAT_ID;
                        randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<HatContentPack>();
                        break;
                    case SHIRT_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_SHIRT_ID;
                        randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<ShirtContentPack>();
                        break;
                    case PANTS_FILTER_BUTTON:
                        modDataKey = ModDataKeys.CUSTOM_PANTS_ID;
                        randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<PantsContentPack>();
                        break;
                    case SLEEVES_FILTER_BUTTON:
                        if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SLEEVES_ID)
                        {
                            modDataKey = ModDataKeys.CUSTOM_SLEEVES_ID;
                            randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<SleevesContentPack>();
                        }
                        else
                        {
                            modDataKey = ModDataKeys.CUSTOM_SHOES_ID;
                            randomContentPack = FashionSense.textureManager.GetRandomAppearanceModel<ShoesContentPack>();
                        }
                        break;
                }

                if (randomContentPack is not null)
                {
                    Game1.player.modData[modDataKey] = randomContentPack.Id;
                    FashionSense.SetSpriteDirty();
                    FashionSense.ResetTextureIfNecessary(randomContentPack.Id);
                }
            }

            if (okButton.containsPoint(x, y))
            {
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
                exitThisMenu();
                Game1.playSound("coin");
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            if (colorPickerTimer > 0)
            {
                return;
            }
            if (colorPicker != null && !Game1.options.SnappyMenus)
            {
                HandleColorPicker(x, y, held: true);
            }

            colorPickerTimer = 100;
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
                        case SHIRT_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.shirt");
                            break;
                        case PANTS_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.pants");
                            break;
                        case SLEEVES_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.sleevesAndShoes");
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
            foreach (ClickableTextureComponent c5 in featureButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    if (c5.hoverText == "disabled")
                    {
                        c5.scale = Math.Min(c5.scale + 0.02f, 2.2f);
                    }

                    if (GetNameOfEnabledFilter() == SLEEVES_FILTER_BUTTON)
                    {
                        switch (c5.name)
                        {
                            case SLEEVES_OPTION_BUTTON:
                                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.sleeves");
                                break;
                            case SHOES_OPTION_BUTTON:
                                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.shoes");
                                break;
                            default:
                                continue;
                        }
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
                        case SHIRT_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_info.shirt");
                            break;
                        case PANTS_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_info.pants");
                            break;
                        case SLEEVES_FILTER_BUTTON:
                            hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_info.sleeves");
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
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(GetAccessoryIndex()));
                        break;
                    case HAT_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAT_ID]);
                        break;
                    case SHIRT_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHIRT_ID]);
                        break;
                    case PANTS_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_PANTS_ID]);
                        break;
                    case SLEEVES_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
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
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(GetAccessoryIndex()));
                        break;
                    case HAT_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAT_ID]);
                        break;
                    case SHIRT_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHIRT_ID]);
                        break;
                    case PANTS_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_PANTS_ID]);
                        break;
                    case SLEEVES_FILTER_BUTTON:
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
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

            if (outfitButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.outfit_button");
                outfitButton.scale = Math.Min(outfitButton.scale + 0.02f, outfitButton.baseScale + 0.3f);
            }
            else
            {
                outfitButton.scale = Math.Max(outfitButton.scale - 0.02f, outfitButton.baseScale);
            }

            if (colorCopyButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_copy_button");
                colorCopyButton.scale = Math.Min(colorCopyButton.scale + 0.02f, colorCopyButton.baseScale + 0.3f);
            }
            else
            {
                colorCopyButton.scale = Math.Max(colorCopyButton.scale - 0.02f, colorCopyButton.baseScale);
            }

            if (colorPasteButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_paste_button");
                if (_cachedColor is null)
                {
                    hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_paste_button_empty");
                }
                colorPasteButton.scale = Math.Min(colorPasteButton.scale + 0.05f, colorPasteButton.baseScale + 0.3f);
            }
            else
            {
                colorPasteButton.scale = Math.Max(colorPasteButton.scale - 0.05f, colorPasteButton.baseScale);
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

            if (randomButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.random_button");
                randomButton.scale = Math.Min(randomButton.scale + 0.02f, 3.2f);
            }
            else
            {
                randomButton.scale = Math.Max(randomButton.scale - 0.02f, randomButton.baseScale);
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

        public override void receiveScrollWheelAction(int direction)
        {
            colorPicker.Scroll(direction);
            HandleColorPicker();
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
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(GetAccessoryIndex()));
                    break;
                case HAT_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HatContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAT_ID]);
                    break;
                case SHIRT_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHIRT_ID]);
                    break;
                case PANTS_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<PantsContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_PANTS_ID]);
                    break;
                case SLEEVES_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
                    if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                    {
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHOES_ID]);
                        break;
                    }
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
                if (leftSelectionButton.name == LIMIT_TO_ACCCESSORIES && GetNameOfEnabledFilter() != ACCESSORY_FILTER_BUTTON)
                {
                    continue;
                }
                leftSelectionButton.draw(b);
            }
            foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
            {
                if (rightSelectionButton.name == LIMIT_TO_ACCCESSORIES && GetNameOfEnabledFilter() != ACCESSORY_FILTER_BUTTON)
                {
                    continue;
                }
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
                    //optionButton.draw(b, optionButton.hoverText == "enabled" ? Color.White : Color.Gray, 1f);
                }
            }
            else if (GetNameOfEnabledFilter() == SLEEVES_FILTER_BUTTON)
            {
                foreach (ClickableTextureComponent featureButton in featureButtons)
                {
                    featureButton.draw(b, featureButton.hoverText == "enabled" ? Color.White : Color.Gray, 1f);
                }
            }

            // Draw the top side bar
            var sideBarPosition = new Vector2(base.xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder + 12, base.yPositionOnScreen + 24);
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 361, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            // Draw the side bar background for each button
            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            // Draw the bottom side bar
            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 377, 13, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            // Draw the buttons
            searchButton.draw(b);
            outfitButton.draw(b);
            colorCopyButton.draw(b);
            colorPasteButton.draw(b);
            if (_cachedColor is not null)
            {
                b.Draw(Game1.mouseCursors2, new Vector2(colorPasteButton.bounds.X, colorPasteButton.bounds.Y), new Rectangle(99, 146, 5, 5), _cachedColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            }

            randomButton.draw(b);
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
                        case SHIRT_FILTER_BUTTON:
                            descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.shirt");
                            break;
                        case PANTS_FILTER_BUTTON:
                            descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.pants");
                            break;
                        case SLEEVES_FILTER_BUTTON:
                            if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SLEEVES_ID)
                            {
                                descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.sleeves");
                            }
                            else
                            {
                                descriptionLabel.name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.shoes");
                            }
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
                        ShirtContentPack sPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHIRT_ID]);
                        if (contentPack is SleevesContentPack sleevesPack && sleevesPack.GetSleevesFromFacingDirection(Game1.player.facingDirection) is SleevesModel slModel && slModel != null)
                        {
                            if (slModel.IsPlayerColorChoiceIgnored() || (sPack is not null && sPack.GetShirtFromFacingDirection(Game1.player.facingDirection) is ShirtModel shModel && shModel is not null && shModel.SleeveColors is not null && slModel.UseShirtColors && slModel.SkinToneMasks is null))
                            {
                                name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                            }
                        }
                        else if (contentPack is HairContentPack hairPack && hairPack.GetHairFromFacingDirection(Game1.player.facingDirection) is HairModel hModel && hModel != null && hModel.IsPlayerColorChoiceIgnored())
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
                        else if (contentPack is ShirtContentPack shirtPack && shirtPack.GetShirtFromFacingDirection(Game1.player.facingDirection) is ShirtModel shModel && shModel != null && shModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is PantsContentPack pantsPack && pantsPack.GetPantsFromFacingDirection(Game1.player.facingDirection) is PantsModel pModel && pModel != null && pModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is not null && contentPack.PackType == AppearanceContentPack.Type.Shoes)
                        {
                            name = FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.shoes");
                        }
                    }

                    colorLabel.name = name;
                }
                else if (c.label == LIMIT_TO_ACCCESSORIES)
                {
                    if (GetNameOfEnabledFilter() != ACCESSORY_FILTER_BUTTON)
                    {
                        continue;
                    }
                }
                else if (c == accessorySlotLabel)
                {
                    offset = currentAccessorySlot > 9 ? -6 : 0;
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
                colorPicker.Draw(b);
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
