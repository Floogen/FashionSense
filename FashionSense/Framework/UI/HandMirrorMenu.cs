using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
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
    public class HandMirrorMenu : IClickableMenu
    {
        private Rectangle _portraitBox;
        private Color? _cachedColor;
        private Farmer _displayFarmer;
        private string hoverText = "";
        private int colorPickerTimer;
        private int currentAccessorySlot;
        private int currentColorMaskLayerIndex;

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
        internal const string MASK_LAYERS = "MaskLayers";

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
        public List<ClickableComponent> sidePanelButtons = new List<ClickableComponent>();
        public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

        public SimpleColorPicker colorPicker;
        private ClickableTextureComponent exitButton;
        private ClickableTextureComponent randomButton;
        private ClickableTextureComponent clearButton;
        private ClickableTextureComponent searchButton;
        private ClickableTextureComponent outfitButton;
        private ClickableTextureComponent colorCopyButton;
        private ClickableTextureComponent colorPasteButton;

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
                myID = 604,
                upNeighborID = 603,
                leftNeighborImmutable = true,
                rightNeighborID = 605,
                downNeighborID = -99998
            });
            rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(_portraitBox.Right - 32, _portraitBox.Y + yOffset, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 605,
                upNeighborID = 608,
                leftNeighborID = 604,
                rightNeighborID = 624,
                downNeighborID = -99998
            });

            leftSelectionButtons.Add(new ClickableTextureComponent(LIMIT_TO_ACCCESSORIES, new Rectangle(_portraitBox.X + 8, _portraitBox.Y + yOffset + 60, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 0.5f));
            rightSelectionButtons.Add(new ClickableTextureComponent(LIMIT_TO_ACCCESSORIES, new Rectangle(_portraitBox.Right - 40, _portraitBox.Y + yOffset + 60, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 0.5f));

            yOffset += 64;
            leftSelectionButtons.Add(new ClickableTextureComponent("Appearance", new Rectangle(_portraitBox.X - 64, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
            {
                myID = 611,
                upNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborID = 612,
                downNeighborID = -99998
            });
            labels.Add(descriptionLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 164, _portraitBox.Y + yOffset + 32, 164, 32), FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.hair")));
            rightSelectionButtons.Add(new ClickableTextureComponent("Appearance", new Rectangle(_portraitBox.Right, _portraitBox.Y + yOffset + 16, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
            {
                myID = 612,
                upNeighborID = -99998,
                leftNeighborID = 611,
                rightNeighborID = 624,
                downNeighborID = -99998
            });
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
                downNeighborID = 604
            });

            filterButtons.Add(new ClickableTextureComponent(SHIRT_FILTER_BUTTON, new Rectangle(_portraitBox.Right + 25, base.yPositionOnScreen + 70, 64, 64), null, lastSelectedFilter == SHIRT_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.shirtButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 606,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = 619,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(PANTS_FILTER_BUTTON, new Rectangle(_portraitBox.Right + 25, base.yPositionOnScreen + 125, 64, 64), null, lastSelectedFilter == PANTS_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.pantsButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 607,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            });

            filterButtons.Add(new ClickableTextureComponent(SLEEVES_FILTER_BUTTON, new Rectangle(_portraitBox.Right + 25, base.yPositionOnScreen + 180, 64, 64), null, lastSelectedFilter == SLEEVES_FILTER_BUTTON ? "enabled" : "disabled", FashionSense.assetManager.sleevesAndShoesButtonTexture, new Rectangle(0, 0, 15, 15), 3f)
            {
                myID = 608,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = 624,
                downNeighborID = 605
            });
            labels.Add(new ClickableComponent(new Rectangle(_portraitBox.Right - 100, _portraitBox.Y + yOffset - 32, 1, 1), "Acc. #", LIMIT_TO_ACCCESSORIES));
            labels.Add(accessorySlotLabel = new ClickableComponent(new Rectangle(_portraitBox.Right - 72, _portraitBox.Y + yOffset - 2, 1, 1), "1", LIMIT_TO_ACCCESSORIES));

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
                rightNeighborID = 614,
                downNeighborID = 611
            });
            featureButtons.Add(new ClickableTextureComponent(SHOES_OPTION_BUTTON, new Rectangle(_portraitBox.Right - 34, _portraitBox.Y + yOffset - 10, 32, 32), null, "disabled", FashionSense.assetManager.shoesButtonTexture, new Rectangle(0, 0, 15, 15), 2f)
            {
                myID = 614,
                upNeighborID = -99998,
                leftNeighborID = 613,
                rightNeighborID = 624,
                downNeighborID = 612
            });

            // Add the leftover buttons
            searchButton = new ClickableTextureComponent("Search", new Rectangle(base.xPositionOnScreen + base.width + IClickableMenu.spaceToClearSideBorder - 16, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder - 48, 32, 32), null, null, Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 2f)
            {
                myID = 619,
                upNeighborID = -99998,
                leftNeighborID = 606,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            outfitButton = new ClickableTextureComponent("Outfits", new Rectangle(searchButton.bounds.X, searchButton.bounds.Y + 48, 32, 32), null, null, Game1.mouseCursors2, new Rectangle(6, 52, 7, 8), 4f)
            {
                myID = 620,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            colorCopyButton = new ClickableTextureComponent("Copy", new Rectangle(searchButton.bounds.X + 3, searchButton.bounds.Y + 96, 32, 32), null, null, Game1.mouseCursors, new Rectangle(278, 288, 5, 6), 4f)
            {
                myID = 621,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            colorPasteButton = new ClickableTextureComponent("Paste", new Rectangle(searchButton.bounds.X + 3, searchButton.bounds.Y + 136, 32, 32), null, null, Game1.mouseCursors, new Rectangle(296, 504, 5, 5), 4f)
            {
                myID = 622,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            randomButton = new ClickableTextureComponent("Randomize", new Rectangle(searchButton.bounds.X, searchButton.bounds.Y + 170, 32, 32), null, null, Game1.mouseCursors, new Rectangle(50, 428, 10, 10), 3f)
            {
                myID = 623,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            clearButton = new ClickableTextureComponent("Clear", new Rectangle(searchButton.bounds.X + 1, searchButton.bounds.Y + 208, 32, 32), null, null, Game1.mouseCursors, new Rectangle(323, 433, 9, 10), 3f)
            {
                myID = 624,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            exitButton = new ClickableTextureComponent("Exit", new Rectangle(searchButton.bounds.X + 1, searchButton.bounds.Y + 248, 32, 32), null, null, Game1.mouseCursors2, new Rectangle(67, 243, 9, 10), 3f)
            {
                myID = 625,
                upNeighborID = -99998,
                leftNeighborID = 608,
                rightNeighborImmutable = true,
                downNeighborID = -99998
            };
            sidePanelButtons = new List<ClickableComponent>()
            {
                searchButton,
                outfitButton,
                colorCopyButton,
                colorPasteButton,
                randomButton,
                clearButton,
                exitButton
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
                    colorPicker.SetColor(Game1.player.hairstyleColor.Value);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, GetAccessoryIndex()));
                    break;
                default:
                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));
                    break;
            }
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
            {
                myID = 615,
                downNeighborID = -99998,
                upNeighborID = -99998,
                leftNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
            {
                myID = 616,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
            {
                myID = 617,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true
            });

            labels.Add(new ClickableComponent(new Rectangle(colorPickerCCs[0].bounds.Right + 80, colorPickerCCs[0].bounds.Y - 15, 1, 1), FashionSense.modHelper.Translation.Get("ui.fashion_sense.title.mask_layer")));

            leftSelectionButtons.Add(new ClickableTextureComponent(MASK_LAYERS, new Rectangle(colorPickerCCs[0].bounds.Right + 60, colorPickerCCs[0].bounds.Y + 10, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
            rightSelectionButtons.Add(new ClickableTextureComponent(MASK_LAYERS, new Rectangle(colorPickerCCs[0].bounds.Right + 105, colorPickerCCs[0].bounds.Y + 10, 48, 48), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));

            // Handle GamePad integration
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        internal void Reset()
        {
            currentColorMaskLayerIndex = GetNextValidColorMaskLayer(GetActiveModel(), -1, 1);
            if (currentColorMaskLayerIndex < 0)
            {
                currentColorMaskLayerIndex = 0;
            }
            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    colorPicker.SetColor(Game1.player.hairstyleColor.Value);
                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player, maskLayerIndex: currentColorMaskLayerIndex));
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, GetAccessoryIndex(), maskLayerIndex: currentColorMaskLayerIndex));
                    break;
                default:
                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player, maskLayerIndex: currentColorMaskLayerIndex));
                    break;
            }
        }

        internal string GetColorPickerLabel(bool isDisabled = false, bool isCompact = false, string enabledFilterName = null)
        {
            var separator = isCompact ? "\n" : " ";
            if (isDisabled)
            {
                return $"{FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.generic")}{separator}{FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_disabled.generic")}:";
            }

            if (GetActiveModel() is not null && GetActiveModel().ColorMaskLayers.Count > currentColorMaskLayerIndex && String.IsNullOrEmpty(GetActiveModel().ColorMaskLayers[currentColorMaskLayerIndex].Name) is false)
            {
                return $"{GetActiveModel().ColorMaskLayers[currentColorMaskLayerIndex].Name}:";
            }

            return $"{FashionSense.modHelper.Translation.Get("ui.fashion_sense.mask_layer.base")} {FashionSense.modHelper.Translation.Get("ui.fashion_sense.color_active.generic")}:";
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

                    colorPicker.SetColor(Game1.player.hairstyleColor.Value);

                    filterButton = filterButtons.First(b => b.name == HAIR_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;

                    colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, GetAccessoryIndex()));

                    filterButton = filterButtons.First(b => b.name == ACCESSORY_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case HAT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAT_FILTER_BUTTON;

                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));

                    filterButton = filterButtons.First(b => b.name == HAT_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case SHIRT_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SHIRT_FILTER_BUTTON;

                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));

                    filterButton = filterButtons.First(b => b.name == SHIRT_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case PANTS_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = PANTS_FILTER_BUTTON;

                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));

                    filterButton = filterButtons.First(b => b.name == PANTS_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
                case SLEEVES_FILTER_BUTTON:
                    Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SLEEVES_FILTER_BUTTON;

                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));

                    filterButton = filterButtons.First(b => b.name == SLEEVES_FILTER_BUTTON) as ClickableTextureComponent;
                    break;
            }
            filterButton.hoverText = "enabled";

            // Get the first AppearanceContentPack that matches the packName
            List<AppearanceContentPack> appearanceModels = new List<AppearanceContentPack>();
            switch (filterName)
            {
                case HAIR_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HairContentPack).OrderBy(m => m.Name).ToList();
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is AccessoryContentPack).OrderBy(m => m.Name).ToList();
                    break;
                case HAT_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is HatContentPack).OrderBy(m => m.Name).ToList();
                    break;
                case SHIRT_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is ShirtContentPack).OrderBy(m => m.Name).ToList();
                    break;
                case PANTS_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is PantsContentPack).OrderBy(m => m.Name).ToList();
                    break;
                case SLEEVES_FILTER_BUTTON:
                    appearanceModels = FashionSense.textureManager.GetAllAppearanceModels().Where(m => m is SleevesContentPack).OrderBy(m => m.Name).ToList();
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

        private AppearanceContentPack GetActiveContentPack()
        {
            AppearanceContentPack contentPack = null;
            switch (GetNameOfEnabledFilter())
            {
                case HAIR_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<HairContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_HAIR_ID]);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(Game1.player, GetAccessoryIndex()));
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
                    if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                    {
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHOES_ID]);
                    }
                    else
                    {
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
                    }
                    break;
            }

            return contentPack;
        }

        private AppearanceModel GetActiveModel()
        {
            AppearanceContentPack contentPack = GetActiveContentPack();
            if (contentPack is null)
            {
                return null;
            }

            if (contentPack is SleevesContentPack sleevesPack)
            {
                return sleevesPack.GetSleevesFromFacingDirection(Game1.player.FacingDirection);
            }
            else if (contentPack is HairContentPack hairPack)
            {
                return hairPack.GetHairFromFacingDirection(Game1.player.FacingDirection);
            }
            else if (contentPack is AccessoryContentPack accessoryPack)
            {
                return accessoryPack.GetAccessoryFromFacingDirection(Game1.player.FacingDirection);
            }
            else if (contentPack is HatContentPack hatPack)
            {
                return hatPack.GetHatFromFacingDirection(Game1.player.FacingDirection);
            }
            else if (contentPack is ShirtContentPack shirtPack)
            {
                return shirtPack.GetShirtFromFacingDirection(Game1.player.FacingDirection);
            }
            else if (contentPack is PantsContentPack pantsPack)
            {
                return pantsPack.GetPantsFromFacingDirection(Game1.player.FacingDirection);
            }
            else if (contentPack is ShoesContentPack shoePack)
            {
                return shoePack.GetShoesFromFacingDirection(Game1.player.FacingDirection);
            }

            return null;
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
                    AppearanceHelpers.SetAppearanceColorForLayer(GetActiveModel(), Game1.player, color, maskLayerIndex: currentColorMaskLayerIndex);
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    FashionSense.accessoryManager.SetColorForIndex(Game1.player, GetAccessoryIndex(), color, maskLayerIndex: currentColorMaskLayerIndex);
                    break;
                case HAT_FILTER_BUTTON:
                    AppearanceHelpers.SetAppearanceColorForLayer(GetActiveModel(), Game1.player, color, maskLayerIndex: currentColorMaskLayerIndex);
                    break;
                case SHIRT_FILTER_BUTTON:
                    AppearanceHelpers.SetAppearanceColorForLayer(GetActiveModel(), Game1.player, color, maskLayerIndex: currentColorMaskLayerIndex);
                    FashionSense.SetSpriteDirty(skipColorMaskRefresh: true);
                    break;
                case PANTS_FILTER_BUTTON:
                    AppearanceHelpers.SetAppearanceColorForLayer(GetActiveModel(), Game1.player, color, maskLayerIndex: currentColorMaskLayerIndex);
                    break;
                case SLEEVES_FILTER_BUTTON:
                    AppearanceHelpers.SetAppearanceColorForLayer(GetActiveModel(), Game1.player, color, maskLayerIndex: currentColorMaskLayerIndex);
                    if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID && Game1.player.modData.ContainsKey(ModDataKeys.CUSTOM_SHOES_ID) && Game1.player.modData[ModDataKeys.CUSTOM_SHOES_ID] == ModDataKeys.INTERNAL_COLOR_OVERRIDE_SHOE_ID)
                    {
                        FashionSense.SetSpriteDirty(skipColorMaskRefresh: true);
                        FashionSense.messageManager.SendVanillaBootColorChangeMessage(_displayFarmer);
                    }
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

                    currentAppearance = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(Game1.player, accessoryIndex));
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
                FashionSense.accessoryManager.AddAccessory(Game1.player, appearanceId, accessoryIndex, preserveColor: true);
            }
            else
            {
                Game1.player.modData[modDataKey] = appearanceId;
            }

            FashionSense.ResetAnimationModDataFields(Game1.player, 0, AnimationModel.Type.Idle, Game1.player.FacingDirection);
            Game1.playSound("grassyStep");

            FashionSense.SetSpriteDirty();
            FashionSense.ResetTextureIfNecessary(currentAppearance);
        }

        private void selectionClick(string name, int change)
        {
            AppearanceModel appearanceModel = GetActiveModel();
            switch (name)
            {
                case "Appearance":
                    {
                        UpdateAppearance(change);

                        Reset();
                        break;
                    }
                case "Direction":
                    _displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
                    _displayFarmer.FarmerSprite.StopAnimation();
                    _displayFarmer.completelyStopAnimatingOrDoingAction();
                    Game1.playSound("pickUpItem");

                    Reset();
                    FashionSense.SetSpriteDirty();
                    break;
                case LIMIT_TO_ACCCESSORIES:
                    if (Game1.player.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON) && Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] == ACCESSORY_FILTER_BUTTON)
                    {
                        currentAccessorySlot = currentAccessorySlot + change < 0 ? 0 : currentAccessorySlot + change;
                        accessorySlotLabel.name = (currentAccessorySlot + 1).ToString();

                        colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, GetAccessoryIndex(), maskLayerIndex: currentColorMaskLayerIndex));

                        Reset();
                    }

                    break;
                case MASK_LAYERS:
                    if (appearanceModel is null)
                    {
                        break;
                    }

                    int updatedColorMaskLayerIndex = GetNextValidColorMaskLayer(appearanceModel, currentColorMaskLayerIndex, change);
                    if (updatedColorMaskLayerIndex >= 0 && appearanceModel.ColorMaskLayers.Count > updatedColorMaskLayerIndex)
                    {
                        currentColorMaskLayerIndex = updatedColorMaskLayerIndex;
                    }

                    if (Game1.player.modData.ContainsKey(ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON) && Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] == ACCESSORY_FILTER_BUTTON)
                    {
                        colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, GetAccessoryIndex(), maskLayerIndex: currentColorMaskLayerIndex));
                    }
                    else
                    {
                        colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player, maskLayerIndex: currentColorMaskLayerIndex));
                    }

                    break;
            }
        }

        private int GetNextValidColorMaskLayer(AppearanceModel appearanceModel, int maskLayerIndex, int change)
        {
            bool hasFoundNextLayer = false;

            if (appearanceModel is not null && appearanceModel.ColorMaskLayers.ElementAtOrDefault(maskLayerIndex + change) is not null)
            {
                maskLayerIndex += change;
            }

            while (hasFoundNextLayer is false)
            {
                if (appearanceModel is not null)
                {
                    var colorMaskLayer = appearanceModel.ColorMaskLayers.ElementAtOrDefault(maskLayerIndex);
                    if (colorMaskLayer is not null && colorMaskLayer.IgnoreUserColorChoice is true)
                    {
                        maskLayerIndex += change;
                        continue;
                    }
                }

                hasFoundNextLayer = true;
            }

            return maskLayerIndex;
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
                    currentColorMaskLayerIndex = 0;

                    c.hoverText = "enabled";
                    switch (c.name)
                    {
                        case HAIR_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAIR_FILTER_BUTTON;

                            colorPicker.SetColor(Game1.player.hairstyleColor.Value);
                            break;
                        case ACCESSORY_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = ACCESSORY_FILTER_BUTTON;
                            break;
                        case HAT_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = HAT_FILTER_BUTTON;
                            break;
                        case SHIRT_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SHIRT_FILTER_BUTTON;
                            break;
                        case PANTS_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = PANTS_FILTER_BUTTON;
                            break;
                        case SLEEVES_FILTER_BUTTON:
                            Game1.player.modData[ModDataKeys.UI_HAND_MIRROR_FILTER_BUTTON] = SLEEVES_FILTER_BUTTON;
                            break;
                    }
                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));

                    if (c.scale != 0f)
                    {
                        c.scale -= 0.25f;
                        c.scale = Math.Max(2.75f, c.scale);
                    }

                    Reset();
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

                    colorPicker.SetColor(FashionSense.accessoryManager.GetColorFromIndex(Game1.player, GetAccessoryIndex()));
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

                    colorPicker.SetColor(AppearanceHelpers.GetAppearanceColorByLayer(GetActiveModel(), Game1.player));
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
                if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.RightShift))
                {
                    FashionSense.outfitManager.ClearOutfit(Game1.player);
                }
                else
                {
                    string modDataKey = String.Empty;
                    switch (GetNameOfEnabledFilter())
                    {
                        case HAIR_FILTER_BUTTON:
                            modDataKey = ModDataKeys.CUSTOM_HAIR_ID;
                            break;
                        case ACCESSORY_FILTER_BUTTON:
                            FashionSense.accessoryManager.RemoveAccessory(Game1.player, currentAccessorySlot);
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
                }

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
                        modDataKey = FashionSense.accessoryManager.GetKeyForAccessoryId(GetAccessoryIndex());
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

            if (exitButton.containsPoint(x, y))
            {
                exitButton.scale -= 0.25f;
                exitButton.scale = Math.Max(0.75f, exitButton.scale);
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
                else if (label == descriptionLabel && descriptionLabel.containsPoint(x, y) && GetActiveModel() is AppearanceModel model && model is not null && model.HidePlayerBase is true)
                {
                    hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.warning.hiding_player_sprite");
                    break;
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
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(Game1.player, GetAccessoryIndex()));
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
                        if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                        {
                            contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHOES_ID]);
                        }
                        else
                        {
                            contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
                        }
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
                        contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<AccessoryContentPack>(FashionSense.accessoryManager.GetAccessoryIdByIndex(Game1.player, GetAccessoryIndex()));
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
                        if (GetCurrentFeatureSlotKey() == ModDataKeys.CUSTOM_SHOES_ID)
                        {
                            contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<ShoesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SHOES_ID]);
                        }
                        else
                        {
                            contentPack = FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(Game1.player.modData[ModDataKeys.CUSTOM_SLEEVES_ID]);
                        }
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

            if (exitButton.containsPoint(x, y))
            {
                hoverText = FashionSense.modHelper.Translation.Get("ui.fashion_sense.exit_button");
                exitButton.scale = Math.Min(exitButton.scale + 0.02f, 3.2f);
            }
            else
            {
                exitButton.scale = Math.Max(exitButton.scale - 0.02f, exitButton.baseScale);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            colorPicker.Scroll(direction);
            HandleColorPicker();
        }

        public override void gamePadButtonHeld(Buttons b)
        {
            base.gamePadButtonHeld(b);
            if (base.currentlySnappedComponent == null)
            {
                return;
            }
            switch (b)
            {
                case Buttons.DPadRight:
                case Buttons.LeftThumbstickRight:
                    switch (base.currentlySnappedComponent.myID)
                    {
                        case 615:
                            colorPicker.recentSliderBar = colorPicker.hueSlider;
                            colorPicker.Scroll(1);
                            HandleColorPicker();
                            break;
                        case 616:
                            colorPicker.recentSliderBar = colorPicker.saturationSlider;
                            colorPicker.Scroll(1);
                            HandleColorPicker();
                            break;
                        case 617:
                            colorPicker.recentSliderBar = colorPicker.lightnessSlider;
                            colorPicker.Scroll(1);
                            HandleColorPicker();
                            break;
                    }
                    break;
                case Buttons.DPadLeft:
                case Buttons.LeftThumbstickLeft:
                    switch (base.currentlySnappedComponent.myID)
                    {
                        case 615:
                            colorPicker.recentSliderBar = colorPicker.hueSlider;
                            colorPicker.Scroll(-1);
                            HandleColorPicker();
                            break;
                        case 616:
                            colorPicker.recentSliderBar = colorPicker.saturationSlider;
                            colorPicker.Scroll(-1);
                            HandleColorPicker();
                            break;
                        case 617:
                            colorPicker.recentSliderBar = colorPicker.lightnessSlider;
                            colorPicker.Scroll(-1);
                            HandleColorPicker();
                            break;
                    }
                    break;
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            if (b == Buttons.RightTrigger)
            {
                selectionClick("Appearance", 1);
            }
            else if (b == Buttons.LeftTrigger)
            {
                selectionClick("Appearance", -1);
            }
            else if (b == Buttons.RightShoulder)
            {
                selectionClick("Direction", 1);
            }
            else if (b == Buttons.LeftShoulder)
            {
                selectionClick("Direction", -1);
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(601);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void draw(SpriteBatch b)
        {
            if (this.currentlySnappedComponent is not null)
            {
                //FashionSense.monitor.Log(this.currentlySnappedComponent.myID.ToString(), StardewModdingAPI.LogLevel.Debug);
            }

            if (Game1.dialogueUp || Game1.IsFading())
            {
                return;
            }

            // Get the custom hair object, if it exists
            AppearanceContentPack contentPack = GetActiveContentPack();
            AppearanceModel appearanceModel = GetActiveModel();

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
                else if (leftSelectionButton.name == MASK_LAYERS && (GetNextValidColorMaskLayer(appearanceModel, currentColorMaskLayerIndex, -1) < 0 || currentColorMaskLayerIndex == GetNextValidColorMaskLayer(appearanceModel, currentColorMaskLayerIndex, -1)))
                {
                    leftSelectionButton.draw(b, Color.Gray, 1f);
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
                else if (rightSelectionButton.name == MASK_LAYERS && currentColorMaskLayerIndex == GetNextValidColorMaskLayer(appearanceModel, currentColorMaskLayerIndex, 1))
                {
                    rightSelectionButton.draw(b, Color.Gray, 1f);
                    continue;
                }

                rightSelectionButton.draw(b);
            }
            foreach (ClickableTextureComponent filterButton in filterButtons)
            {
                filterButton.draw(b, filterButton.hoverText == "enabled" ? Color.White : Color.Gray, 1f);
            }

            // Draw option button for supported appearance types (currently only accessories)
            if (GetNameOfEnabledFilter() == SLEEVES_FILTER_BUTTON)
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

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            sideBarPosition.Y += 8 * 4;
            b.Draw(Game1.mouseCursors, sideBarPosition, new Rectangle(316, 369, 13, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.75f);

            // Draw the bottom side bar
            sideBarPosition.Y += 8 * 2;
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
            exitButton.draw(b);

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

                    var activeModel = GetActiveModel();
                    if (activeModel is not null && activeModel.HidePlayerBase is true)
                    {
                        color = Game1.eveningColor;
                    }

                    offset = descriptionLabel.bounds.Width - 64f - Game1.smallFont.MeasureString(c.name).X / 2f;
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
                        if (contentPack is SleevesContentPack sleevesPack && sleevesPack.GetSleevesFromFacingDirection(Game1.player.FacingDirection) is SleevesModel slModel && slModel != null)
                        {
                            if (slModel.IsPlayerColorChoiceIgnored() || (sPack is not null && sPack.GetShirtFromFacingDirection(Game1.player.FacingDirection) is ShirtModel shModel && shModel is not null && shModel.SleeveColors is not null && slModel.UseShirtColors && slModel.SkinToneMasks is null))
                            {
                                name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                            }
                        }
                        else if (contentPack is HairContentPack hairPack && hairPack.GetHairFromFacingDirection(Game1.player.FacingDirection) is HairModel hModel && hModel != null && hModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is AccessoryContentPack accessoryPack && accessoryPack.GetAccessoryFromFacingDirection(Game1.player.FacingDirection) is AccessoryModel aModel && aModel != null && aModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is HatContentPack hatPack && hatPack.GetHatFromFacingDirection(Game1.player.FacingDirection) is HatModel htModel && htModel != null && htModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is ShirtContentPack shirtPack && shirtPack.GetShirtFromFacingDirection(Game1.player.FacingDirection) is ShirtModel shModel && shModel != null && shModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        else if (contentPack is PantsContentPack pantsPack && pantsPack.GetPantsFromFacingDirection(Game1.player.FacingDirection) is PantsModel pModel && pModel != null && pModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
                        }
                        if (contentPack is ShoesContentPack shoesPack && shoesPack.GetShoesFromFacingDirection(Game1.player.FacingDirection) is ShoesModel sModel && sModel != null && sModel.IsPlayerColorChoiceIgnored())
                        {
                            name = GetColorPickerLabel(true, enabledFilterName: GetNameOfEnabledFilter());
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
