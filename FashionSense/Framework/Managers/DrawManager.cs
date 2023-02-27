using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Generic;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.Models.General;
using FashionSense.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using static StardewValley.FarmerSprite;

namespace FashionSense.Framework.Managers
{
    internal class DrawManager
    {
        private SpriteBatch _spriteBatch { get; }
        private FarmerRenderer _farmerRenderer { get; }
        private SkinToneModel _skinToneModel { get; }
        private Texture2D _baseTexture { get; }
        private Rectangle _farmerSourceRectangle { get; }
        private Rectangle _shirtSourceRectangle { get; }
        private Rectangle _dyedShirtSourceRectangle { get; }
        private Rectangle _accessorySourceRectangle { get; }
        private Rectangle _hatSourceRectangle { get; }
        private Dictionary<AppearanceModel, Rectangle> _appearanceTypeToSourceRectangles { get; }
        private AnimationFrame _animationFrame { get; }
        private bool _areColorMasksPendingRefresh { get; }
        private bool _isDrawingForUI { get; }
        private bool _hideSleeves { get; }
        private Color _overrideColor { get; }
        private Vector2 _position { get; }
        private Vector2 _origin { get; }
        private Vector2 _positionOffset { get; }
        private Vector2 _rotationAdjustment; // Purposely omitting { get; set; } as certain vanilla draw methods modify this value
        private int _facingDirection { get; }
        private int _currentFrame { get; }
        private float _scale { get; }
        private float _rotation { get; }

        internal float LayerDepth { get; set; }

        public DrawManager(SpriteBatch spriteBatch, FarmerRenderer farmerRenderer, SkinToneModel skinToneModel, Texture2D baseTexture, Rectangle farmerSourceRectangle, Rectangle shirtSourceRectangle, Rectangle dyedShirtSourceRectangle, Rectangle accessorySourceRectangle, Rectangle hatSourceRectangle, Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles, AnimationFrame animationFrame, Color overrideColor, Vector2 position, Vector2 origin, Vector2 positionOffset, Vector2 rotationAdjustment, int facingDirection, int currentFrame, float scale, float rotation, bool areColorMasksPendingRefresh, bool isDrawingForUI, bool hideSleeves)
        {
            _spriteBatch = spriteBatch;
            _farmerRenderer = farmerRenderer;
            _skinToneModel = skinToneModel;
            _baseTexture = baseTexture;
            _farmerSourceRectangle = farmerSourceRectangle;
            _shirtSourceRectangle = shirtSourceRectangle;
            _dyedShirtSourceRectangle = dyedShirtSourceRectangle;
            _accessorySourceRectangle = accessorySourceRectangle;
            _hatSourceRectangle = hatSourceRectangle;
            _appearanceTypeToSourceRectangles = appearanceTypeToSourceRectangles;
            _animationFrame = animationFrame;
            _overrideColor = overrideColor;
            _position = position;
            _origin = origin;
            _positionOffset = positionOffset;
            _rotationAdjustment = rotationAdjustment;
            _facingDirection = facingDirection;
            _currentFrame = currentFrame;
            _scale = scale;
            _rotation = rotation;
            _areColorMasksPendingRefresh = areColorMasksPendingRefresh;
            _isDrawingForUI = isDrawingForUI;
            _hideSleeves = hideSleeves;
        }

        public void DrawLayers(Farmer who, List<LayerData> layers)
        {
            foreach (var layer in layers)
            {
                if (layer.IsVanilla)
                {
                    DrawVanillaLayer(who, layer);
                }
                else
                {
                    DrawCustomLayer(who, layer);
                }
            }
        }

        private void DrawVanillaLayer(Farmer who, LayerData layer)
        {
            switch (layer.AppearanceType)
            {
                case AppearanceContentPack.Type.Pants:
                    DrawPantsVanilla(who);
                    break;
                case AppearanceContentPack.Type.Sleeves:
                    DrawSleevesVanilla(who);
                    break;
                case AppearanceContentPack.Type.Shirt:
                    DrawShirtVanilla(who);
                    break;
                case AppearanceContentPack.Type.Accessory:
                    DrawAccessoryVanilla(who);
                    break;
                case AppearanceContentPack.Type.Hair:
                    DrawHairVanilla(who);
                    break;
                case AppearanceContentPack.Type.Hat:
                    DrawHatVanilla(who);
                    break;
            }
        }

        private void DrawCustomLayer(Farmer who, LayerData layer)
        {
            switch (layer.AppearanceType)
            {
                case AppearanceContentPack.Type.Pants:
                case AppearanceContentPack.Type.Shirt:
                case AppearanceContentPack.Type.Accessory:
                case AppearanceContentPack.Type.Hair:
                case AppearanceContentPack.Type.Hat:
                    DrawAppearance(who, layer);
                    break;
                case AppearanceContentPack.Type.Sleeves:
                    DrawSleevesCustom(who, layer);
                    break;
            }
        }

        #region Vanilla draw methods
        private void DrawPantsVanilla(Farmer who)
        {
            Rectangle pants_rect = new Rectangle(_farmerSourceRectangle.X, _farmerSourceRectangle.Y, _farmerSourceRectangle.Width, _farmerSourceRectangle.Height);
            pants_rect.X += _farmerRenderer.ClampPants(who.GetPantsIndex()) % 10 * 192;
            pants_rect.Y += _farmerRenderer.ClampPants(who.GetPantsIndex()) / 10 * 688;

            if (!who.IsMale)
            {
                pants_rect.X += 96;
            }

            _spriteBatch.Draw(FarmerRenderer.pantsTexture, _position + _origin + _positionOffset, pants_rect, _overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : _overrideColor, _rotation, _origin, 4f * _scale, _animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, IncrementAndGetLayerDepth());
        }

        private void DrawSleevesVanilla(Farmer who)
        {
            // Get the sleeves model, if applicable
            SleevesModel sleevesModel = null;
            if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SLEEVES_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<SleevesContentPack>(who.modData[ModDataKeys.CUSTOM_SLEEVES_ID]) is SleevesContentPack sleevesPack && sleevesPack != null)
            {
                sleevesModel = sleevesPack.GetSleevesFromFacingDirection(_facingDirection);
            }

            // Handle the vanilla sleeve / arm drawing, if a custom sleeve model isn't given
            if (sleevesModel is null && _hideSleeves is false)
            {
                _farmerSourceRectangle.Offset((_animationFrame.secondaryArm ? 192 : 96), 0);

                _spriteBatch.Draw(_baseTexture, _position + _origin + _positionOffset + who.armOffset, _farmerSourceRectangle, _overrideColor, _rotation, _origin, 4f * _scale, _animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, IncrementAndGetLayerDepth());
            }
        }

        private void DrawShirtVanilla(Farmer who)
        {
            switch (_facingDirection)
            {
                case 0:
                    if (!who.bathingClothes)
                    {
                        _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + new Vector2(16f * _scale + (float)(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4) + (float)(int)_farmerRenderer.heightOffset * _scale), _shirtSourceRectangle, _overrideColor.Equals(Color.White) ? Color.White : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                        _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + new Vector2(16f * _scale + (float)(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4) + (float)(int)_farmerRenderer.heightOffset * _scale), _dyedShirtSourceRectangle, _overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                    }
                    break;
                case 1:
                    if (_rotation == -(float)Math.PI / 32f)
                    {
                        _rotationAdjustment.X = 6f;
                        _rotationAdjustment.Y = -2f;
                    }
                    else if (_rotation == (float)Math.PI / 32f)
                    {
                        _rotationAdjustment.X = -6f;
                        _rotationAdjustment.Y = 1f;
                    }
                    if (!who.bathingClothes)
                    {
                        _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(16f * _scale + (float)(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4), 56f * _scale + (float)(FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4) + (float)(int)_farmerRenderer.heightOffset * _scale), _shirtSourceRectangle, _overrideColor.Equals(Color.White) ? Color.White : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.None, IncrementAndGetLayerDepth());
                        _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(16f * _scale + (float)(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4), 56f * _scale + (float)(FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4) + (float)(int)_farmerRenderer.heightOffset * _scale), _dyedShirtSourceRectangle, _overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.None, IncrementAndGetLayerDepth());
                    }
                    break;
                case 2:
                    if (!who.bathingClothes)
                    {
                        _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4) + (float)(int)_farmerRenderer.heightOffset * _scale - (float)(who.IsMale ? 0 : 0)), _shirtSourceRectangle, _overrideColor.Equals(Color.White) ? Color.White : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth() + 1.5E-07f);
                        _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4) + (float)(int)_farmerRenderer.heightOffset * _scale - (float)(who.IsMale ? 0 : 0)), _dyedShirtSourceRectangle, _overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth() + 1.5E-07f);
                    }
                    break;
                case 3:
                    {
                        if (_rotation == -(float)Math.PI / 32f)
                        {
                            _rotationAdjustment.X = 6f;
                            _rotationAdjustment.Y = -2f;
                        }
                        else if (_rotation == (float)Math.PI / 32f)
                        {
                            _rotationAdjustment.X = -5f;
                            _rotationAdjustment.Y = 1f;
                        }
                        if (!who.bathingClothes)
                        {
                            _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + (int)_farmerRenderer.heightOffset), _shirtSourceRectangle, _overrideColor.Equals(Color.White) ? Color.White : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.None, IncrementAndGetLayerDepth() + 1.5E-07f);
                            _spriteBatch.Draw(FarmerRenderer.shirtsTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + (int)_farmerRenderer.heightOffset), _dyedShirtSourceRectangle, _overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.None, IncrementAndGetLayerDepth() + 1.5E-07f);
                        }
                        break;
                    }
            }
        }

        private void DrawAccessoryVanilla(Farmer who)
        {
            if ((int)who.accessory >= 0)
            {
                switch (who.facingDirection.Value)
                {
                    case 0:
                        return;
                    case 1:
                        _spriteBatch.Draw(FarmerRenderer.accessoriesTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + (int)_farmerRenderer.heightOffset - 4), _accessorySourceRectangle, (_overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.None, IncrementAndGetLayerDepth());
                        break;
                    case 2:
                        _spriteBatch.Draw(FarmerRenderer.accessoriesTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + (int)_farmerRenderer.heightOffset - 4), _accessorySourceRectangle, (_overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.None, IncrementAndGetLayerDepth());
                        break;
                    case 3:
                        _spriteBatch.Draw(FarmerRenderer.accessoriesTexture, _position + _origin + _positionOffset + _rotationAdjustment + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + (int)_farmerRenderer.heightOffset), _accessorySourceRectangle, (_overrideColor.Equals(Color.White) && (int)who.accessory < 6) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale + ((_rotation != 0f) ? 0f : 0f), SpriteEffects.FlipHorizontally, IncrementAndGetLayerDepth());
                        break;
                }
            }
        }

        private void DrawHairVanilla(Farmer who)
        {
            int hair_style = who.getHair();
            HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
            if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
            {
                hair_style = hair_metadata.coveredIndex;
                hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
            }

            Texture2D hairTexture = FarmerRenderer.hairStylesTexture;
            Rectangle hairstyleSourceRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
            if (hair_metadata != null)
            {
                hairTexture = hair_metadata.texture;
                hairstyleSourceRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
            }

            switch (_facingDirection)
            {
                case 0:
                    hairstyleSourceRect.Offset(0, 64);
                    _spriteBatch.Draw(hairTexture, _position + _origin + _positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + 4 + ((who.IsMale && hair_style >= 16) ? (-4) : ((!who.IsMale && hair_style < 16) ? 4 : 0))), hairstyleSourceRect, _overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                    break;
                case 1:
                    hairstyleSourceRect.Offset(0, 32);
                    _spriteBatch.Draw(hairTexture, _position + _origin + _positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, _overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                    break;
                case 2:
                    _spriteBatch.Draw(hairTexture, _position + _origin + _positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, _overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                    break;
                case 3:
                    bool flip2 = true;
                    if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                    {
                        flip2 = false;
                        hairstyleSourceRect.Offset(0, 96);
                    }
                    else
                    {
                        hairstyleSourceRect.Offset(0, 32);
                    }
                    _spriteBatch.Draw(hairTexture, _position + _origin + _positionOffset + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))), hairstyleSourceRect, _overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor) : _overrideColor, _rotation, _origin, 4f * _scale, flip2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, IncrementAndGetLayerDepth());
                    break;
            }
        }

        private void DrawHatVanilla(Farmer who)
        {
            if (who.hat.Value != null && !who.bathingClothes)
            {
                bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
                if (who.hat.Value.isMask && _facingDirection == 0)
                {
                    Rectangle mask_draw_rect = _hatSourceRectangle;
                    mask_draw_rect.Height -= 11;
                    mask_draw_rect.Y += 11;
                    _spriteBatch.Draw(FarmerRenderer.hatsTexture, _position + _origin + _positionOffset + new Vector2(0f, 44f) + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)_farmerRenderer.heightOffset), mask_draw_rect, Color.White, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                    mask_draw_rect = _hatSourceRectangle;
                    mask_draw_rect.Height = 11;
                    _spriteBatch.Draw(FarmerRenderer.hatsTexture, _position + _origin + _positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)_farmerRenderer.heightOffset), mask_draw_rect, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : Color.White, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                }
                else
                {
                    _spriteBatch.Draw(FarmerRenderer.hatsTexture, _position + _origin + _positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[_currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[_currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)_farmerRenderer.heightOffset), _hatSourceRectangle, who.hat.Value.isPrismatic ? Utility.GetPrismaticColor() : Color.White, _rotation, _origin, 4f * _scale, SpriteEffects.None, IncrementAndGetLayerDepth());
                }
            }
        }
        #endregion

        #region Custom draw methods
        private void DrawAppearance(Farmer who, LayerData layer)
        {
            var model = layer.AppearanceModel;
            var modelPack = model.Pack;

            // Adjust color if needed
            var modelColor = layer.Color;
            if (model.DisableGrayscale)
            {
                modelColor = Color.White;
            }
            else if (model.IsPrismatic)
            {
                modelColor = Utility.GetPrismaticColor(speedMultiplier: model.PrismaticAnimationSpeedMultiplier);
            }

            // Get any positional offset
            Position positionOffset = GetPositionOffset(model);

            var featureOffset = GetFeatureOffset(_facingDirection, _currentFrame, _scale, _farmerRenderer, model, false);
            featureOffset.Y -= who.IsMale ? 4 : 0;
            _spriteBatch.Draw(modelPack.Texture, GetScaledPosition(_position, model, _isDrawingForUI) + _origin + _positionOffset + featureOffset, GetSourceRectangle(model, _appearanceTypeToSourceRectangles), model.HasColorMask() ? Color.White : modelColor, _rotation, _origin + new Vector2(positionOffset.X, positionOffset.Y), model.Scale * _scale, model.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, IncrementAndGetLayerDepth());

            if (model.HasColorMask())
            {
                DrawColorMask(_spriteBatch, modelPack, model, _areColorMasksPendingRefresh, GetScaledPosition(_position, model, _isDrawingForUI) + _origin + _positionOffset + featureOffset, GetSourceRectangle(model, _appearanceTypeToSourceRectangles), modelColor, _rotation, _origin + new Vector2(positionOffset.X, positionOffset.Y), model.Scale * _scale, IncrementAndGetLayerDepth());
            }
            if (model.HasSkinToneMask())
            {
                DrawSkinToneMask(_spriteBatch, modelPack, model, _skinToneModel, _areColorMasksPendingRefresh, GetScaledPosition(_position, model, _isDrawingForUI) + _origin + _positionOffset + featureOffset, GetSourceRectangle(model, _appearanceTypeToSourceRectangles), modelColor, _rotation, _origin + new Vector2(positionOffset.X, positionOffset.Y), model.Scale * _scale, IncrementAndGetLayerDepth());
            }
        }

        private void DrawSleevesCustom(Farmer who, LayerData layer)
        {
            var sleevesModel = layer.AppearanceModel as SleevesModel;
            var sleevesModelPack = sleevesModel.Pack as SleevesContentPack;

            // Adjust color if needed
            var modelColor = layer.Color;
            if (sleevesModel.DisableGrayscale)
            {
                modelColor = Color.White;
            }
            else if (sleevesModel.IsPrismatic)
            {
                modelColor = Utility.GetPrismaticColor(speedMultiplier: sleevesModel.PrismaticAnimationSpeedMultiplier);
            }

            // Get any positional offset
            Position positionOffset = GetPositionOffset(sleevesModel);

            var featureOffset = GetFeatureOffset(_facingDirection, _currentFrame, _scale, _farmerRenderer, sleevesModel, false);
            featureOffset.Y -= who.IsMale ? 4 : 0;
            _spriteBatch.Draw(sleevesModelPack.Texture, GetScaledPosition(_position, sleevesModel, _isDrawingForUI) + _origin + _positionOffset + featureOffset, GetSourceRectangle(sleevesModel, _appearanceTypeToSourceRectangles), sleevesModel.HasColorMask() ? Color.White : modelColor, _rotation, _origin + new Vector2(positionOffset.X, positionOffset.Y), sleevesModel.Scale * _scale, sleevesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, IncrementAndGetLayerDepth());

            Rectangle customSleevesSourceRect = _appearanceTypeToSourceRectangles[sleevesModel];
            if ((sleevesModel.HasColorMask() || sleevesModel.HasShirtToneMask()) && sleevesModel.UseShirtColors)
            {
                // Get the shirt model, if applicable
                ShirtModel shirtModel = null;
                if (who.modData.ContainsKey(ModDataKeys.CUSTOM_SHIRT_ID) && FashionSense.textureManager.GetSpecificAppearanceModel<ShirtContentPack>(who.modData[ModDataKeys.CUSTOM_SHIRT_ID]) is ShirtContentPack sPack && sPack != null)
                {
                    shirtModel = sPack.GetShirtFromFacingDirection(_facingDirection);
                }

                if (shirtModel is not null && shirtModel.SleeveColors is not null)
                {
                    DrawSleeveColorMask(_spriteBatch, sleevesModelPack, sleevesModel, shirtModel, _areColorMasksPendingRefresh, GetScaledPosition(_position, sleevesModel, _isDrawingForUI) + _origin + _positionOffset + featureOffset, customSleevesSourceRect, modelColor, _rotation, _origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * _scale, IncrementAndGetLayerDepth());
                }
                else
                {
                    DrawSleeveColorMaskVanilla(_spriteBatch, sleevesModelPack, sleevesModel, _areColorMasksPendingRefresh, who, _farmerRenderer, GetScaledPosition(_position, sleevesModel, _isDrawingForUI) + _origin + _positionOffset + featureOffset, customSleevesSourceRect, modelColor, _rotation, _origin + new Vector2(sleevesModel.BodyPosition.X, sleevesModel.BodyPosition.Y), sleevesModel.Scale * _scale, IncrementAndGetLayerDepth());
                }
            }
            else
            {
                DrawColorMask(_spriteBatch, sleevesModelPack, sleevesModel, _areColorMasksPendingRefresh, GetScaledPosition(_position, sleevesModel, _isDrawingForUI) + _origin + _positionOffset + featureOffset, customSleevesSourceRect, modelColor, _rotation, _origin + new Vector2(positionOffset.X, positionOffset.Y), sleevesModel.Scale * _scale, IncrementAndGetLayerDepth());
            }

            if (sleevesModel.HasSkinToneMask())
            {
                DrawSkinToneMask(_spriteBatch, sleevesModelPack, sleevesModel, _skinToneModel, _areColorMasksPendingRefresh, GetScaledPosition(_position, sleevesModel, _isDrawingForUI) + _origin + _positionOffset + featureOffset, customSleevesSourceRect, modelColor, _rotation, _origin + new Vector2(positionOffset.X, positionOffset.Y), sleevesModel.Scale * _scale, IncrementAndGetLayerDepth());
            }
        }

        internal static void DrawColorMask(SpriteBatch b, AppearanceContentPack appearancePack, AppearanceModel appearanceModel, bool areColorMasksPendingRefresh, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (appearancePack.ColorMaskTexture is null || areColorMasksPendingRefresh)
            {
                Color[] data = new Color[appearancePack.Texture.Width * appearancePack.Texture.Height];
                appearancePack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, appearancePack.Texture.Width, appearancePack.Texture.Height);

                for (int i = 0; i < data.Length; i++)
                {
                    if (!appearanceModel.IsMaskedColor(data[i]))
                    {
                        data[i] = Color.Transparent;
                    }
                }

                maskedTexture.SetData(data);
                appearancePack.ColorMaskTexture = maskedTexture;
            }

            b.Draw(appearancePack.ColorMaskTexture, position, sourceRect, color, rotation, origin, scale, appearanceModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private void DrawSleeveColorMask(SpriteBatch b, SleevesContentPack sleevesPack, SleevesModel sleevesModel, ShirtModel shirtModel, bool areColorMasksPendingRefresh, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (sleevesPack.ShirtToneTexture is null || areColorMasksPendingRefresh)
            {
                Color[] data = new Color[sleevesPack.Texture.Width * sleevesPack.Texture.Height];
                sleevesPack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, sleevesPack.Texture.Width, sleevesPack.Texture.Height);

                var firstSleeveColor = shirtModel.GetSleeveColor(0);
                var secondSleeveColor = shirtModel.GetSleeveColor(1);
                var thirdSleeveColor = shirtModel.GetSleeveColor(2);

                for (int i = 0; i < data.Length; i++)
                {
                    if (sleevesModel.ShirtToneMask is not null)
                    {
                        if (sleevesModel.IsShirtToneMaskColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ShirtToneMask.DarkTone is not null && data[i] == sleevesModel.ShirtToneMask.Darkest && shirtModel.HasSleeveColorAtLayer(0))
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.MediumTone is not null && data[i] == sleevesModel.ShirtToneMask.Medium && shirtModel.HasSleeveColorAtLayer(1))
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.LightTone is not null && data[i] == sleevesModel.ShirtToneMask.Lightest && shirtModel.HasSleeveColorAtLayer(2))
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                    else if (sleevesModel.ColorMasks is not null)
                    {
                        if (sleevesModel.IsMaskedColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ColorMasks.Count > 0 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[0]) && shirtModel.HasSleeveColorAtLayer(0))
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 1 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[1]) && shirtModel.HasSleeveColorAtLayer(1))
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 2 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[2]) && shirtModel.HasSleeveColorAtLayer(2))
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                }

                maskedTexture.SetData(data);
                sleevesPack.ShirtToneTexture = maskedTexture;
            }

            b.Draw(sleevesPack.ShirtToneTexture, position, sourceRect, Color.White, rotation, origin, scale, sleevesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        private void DrawSleeveColorMaskVanilla(SpriteBatch b, SleevesContentPack sleevesPack, SleevesModel sleevesModel, bool areColorMasksPendingRefresh, Farmer who, FarmerRenderer renderer, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (sleevesPack.ShirtToneTexture is null || areColorMasksPendingRefresh)
            {
                Color[] data = new Color[sleevesPack.Texture.Width * sleevesPack.Texture.Height];
                sleevesPack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, sleevesPack.Texture.Width, sleevesPack.Texture.Height);

                var shirtSleeveColors = AppearanceHelpers.GetVanillaShirtSleeveColors(who, renderer);
                var firstSleeveColor = shirtSleeveColors[0];
                var secondSleeveColor = shirtSleeveColors[1];
                var thirdSleeveColor = shirtSleeveColors[2];

                for (int i = 0; i < data.Length; i++)
                {
                    if (sleevesModel.ShirtToneMask is not null)
                    {
                        if (sleevesModel.IsShirtToneMaskColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ShirtToneMask.DarkTone is not null && data[i] == sleevesModel.ShirtToneMask.Darkest)
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.MediumTone is not null && data[i] == sleevesModel.ShirtToneMask.Medium)
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ShirtToneMask.LightTone is not null && data[i] == sleevesModel.ShirtToneMask.Lightest)
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                    else if (sleevesModel.ColorMasks is not null)
                    {
                        if (sleevesModel.IsMaskedColor(data[i]) is false)
                        {
                            data[i] = Color.Transparent;
                            continue;
                        }

                        if (sleevesModel.ColorMasks.Count > 0 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[0]))
                        {
                            data[i] = firstSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 1 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[1]))
                        {
                            data[i] = secondSleeveColor;
                        }
                        else if (sleevesModel.ColorMasks.Count > 2 && data[i] == AppearanceModel.GetColor(sleevesModel.ColorMasks[2]))
                        {
                            data[i] = thirdSleeveColor;
                        }
                    }
                }

                maskedTexture.SetData(data);
                sleevesPack.ShirtToneTexture = maskedTexture;
            }

            b.Draw(sleevesPack.ShirtToneTexture, position, sourceRect, Color.White, rotation, origin, scale, sleevesModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        internal static void DrawSkinToneMask(SpriteBatch b, AppearanceContentPack appearancePack, AppearanceModel appearanceModel, SkinToneModel skinTone, bool areColorMasksPendingRefresh, Vector2 position, Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth)
        {
            if (appearancePack.SkinMaskTexture is null || areColorMasksPendingRefresh)
            {
                Color[] data = new Color[appearancePack.Texture.Width * appearancePack.Texture.Height];
                appearancePack.Texture.GetData(data);
                Texture2D maskedTexture = new Texture2D(Game1.graphics.GraphicsDevice, appearancePack.Texture.Width, appearancePack.Texture.Height);

                for (int i = 0; i < data.Length; i++)
                {
                    if (!appearanceModel.IsSkinToneMaskColor(data[i]))
                    {
                        data[i] = Color.Transparent;
                    }
                    else if (appearanceModel.SkinToneMasks is not null)
                    {
                        if (appearanceModel.SkinToneMasks.LightTone is not null && data[i] == appearanceModel.SkinToneMasks.Lightest)
                        {
                            data[i] = skinTone.Lightest;
                        }
                        else if (appearanceModel.SkinToneMasks.MediumTone is not null && data[i] == appearanceModel.SkinToneMasks.Medium)
                        {
                            data[i] = skinTone.Medium;
                        }
                        else if (appearanceModel.SkinToneMasks.DarkTone is not null && data[i] == appearanceModel.SkinToneMasks.Darkest)
                        {
                            data[i] = skinTone.Darkest;
                        }
                    }
                }

                maskedTexture.SetData(data);
                appearancePack.SkinMaskTexture = maskedTexture;
            }

            b.Draw(appearancePack.SkinMaskTexture, position, sourceRect, appearanceModel.DisableSkinGrayscale ? Color.White : color, rotation, origin, scale, appearanceModel.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }
        #endregion

        #region Helper methods
        private Position GetPositionOffset(AppearanceModel model)
        {
            switch (model)
            {
                case PantsModel pantsModel:
                    return pantsModel.BodyPosition;
                case ShoesModel shoesModel:
                    return shoesModel.BodyPosition;
                case ShirtModel shirtModel:
                    return shirtModel.BodyPosition;
                case AccessoryModel accessoryModel:
                    return accessoryModel.HeadPosition;
                case HairModel hairModel:
                    return hairModel.HeadPosition;
                case SleevesModel sleevesModel:
                    return sleevesModel.BodyPosition;
                case HatModel hatModel:
                    return hatModel.HeadPosition;
            }
            return new Position();
        }

        private Rectangle GetSourceRectangle(AppearanceModel model, Dictionary<AppearanceModel, Rectangle> appearanceTypeToSourceRectangles)
        {
            return appearanceTypeToSourceRectangles[model];
        }

        private Vector2 GetFeatureOffset(int facingDirection, int currentFrame, float scale, FarmerRenderer renderer, AppearanceModel model, bool flip = false)
        {
            Vector2 offset = Vector2.Zero;
            if (model.DisableNativeOffset)
            {
                return offset;
            }

            var type = model.GetPackType();
            if (type is AppearanceContentPack.Type.Hat)
            {
                return new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 4 + (int)renderer.heightOffset.Value);
            }

            if (type is AppearanceContentPack.Type.Shirt)
            {
                switch (facingDirection)
                {
                    case 0:
                        return new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset.Value * scale);
                    case 1:
                        return new Vector2(16f * scale + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), 56f * scale + (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset.Value * scale);
                    case 2:
                        return new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)renderer.heightOffset.Value * scale);
                    case 3:
                        return new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)renderer.heightOffset.Value);
                }
            }
            else if (type is not AppearanceContentPack.Type.Sleeves)
            {
                switch (facingDirection)
                {
                    case 0:
                        offset = new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                    case 1:
                        offset = new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                    case 2:
                        offset = new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                    case 3:
                        offset = new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (flip ? 4 : 0) + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4);
                        break;
                }
            }

            if (type is AppearanceContentPack.Type.Accessory or AppearanceContentPack.Type.AccessorySecondary or AppearanceContentPack.Type.AccessoryTertiary)
            {
                switch (facingDirection)
                {
                    case 0:
                    case 1:
                        break;
                    case 2:
                    case 3:
                        offset.Y += 4;
                        break;
                }

                offset.Y += renderer.heightOffset.Value;
            }

            return offset;
        }

        private Vector2 GetScaledPosition(Vector2 position, AppearanceModel model, bool isDrawingForUI)
        {
            if (isDrawingForUI)
            {
                position.Y += (4f - model.Scale) * 32;
            }

            return position;
        }

        private float IncrementAndGetLayerDepth()
        {
            LayerDepth += 0.000001f;
            return LayerDepth;
        }
        #endregion
    }
}
