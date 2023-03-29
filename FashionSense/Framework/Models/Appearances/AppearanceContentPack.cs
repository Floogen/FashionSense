using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace FashionSense.Framework.Models.Appearances
{
    public abstract class AppearanceContentPack
    {
        public enum Type
        {
            Unknown,
            Hair,
            Accessory,
            AccessorySecondary,
            AccessoryTertiary,
            Hat,
            Shirt,
            Pants,
            Sleeves,
            Shoes,
            Player
        }

        public bool IsLocked { get; set; }
        internal Type PackType { get; set; }
        internal string Owner { get; set; }
        internal string Author { get; set; }
        public string Name { get; set; }
        public Version Format { get; set; } = new Version("1.0.0");
        internal string Id { get; set; }
        internal string PackName { get; set; }
        internal string PackId { get; set; }
        internal Texture2D Texture { get { return _texture; } set { _cachedTexture = value; ResetTexture(); } }
        private Texture2D _texture;
        private Texture2D _cachedTexture;
        internal Texture2D ColorMaskTexture { get; set; }
        internal Texture2D SkinMaskTexture { get; set; }
        internal Texture2D CollectiveMaskTexture { get; set; }
        internal bool IsTextureDirty { get; set; }

        internal abstract void LinkId();

        internal bool ResetTexture()
        {
            try
            {
                if (_texture is null)
                {
                    _texture = new Texture2D(Game1.graphics.GraphicsDevice, _cachedTexture.Width, _cachedTexture.Height);
                }

                Color[] colors = new Color[_cachedTexture.Width * _cachedTexture.Height];
                _cachedTexture.GetData(colors);
                _texture.SetData(colors);

                IsTextureDirty = false;
            }
            catch (Exception ex)
            {
                FashionSense.monitor.Log($"Failed to restore cached texture: {ex}", StardewModdingAPI.LogLevel.Trace);
                return false;
            }

            return true;
        }

        internal Texture2D GetCachedTexture()
        {
            return _cachedTexture;
        }
    }
}
