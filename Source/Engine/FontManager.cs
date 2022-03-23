using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class FontManager
    {
        private static readonly Dictionary<string, SpriteFont> loadedFonts = new Dictionary<string, SpriteFont>();

        private static readonly Dictionary<SpriteFont, FontInfo> fontInfos = new Dictionary<SpriteFont, FontInfo>();

        public static SpriteFont LoadFont(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (loadedFonts.TryGetValue(name, out var font))
            {
                return font;
            }

            font = GameEngine.Instance.Content.Load<SpriteFont>(name);

            if (font == null)
            {
                throw new ArgumentException("No font with the given name was found. Name: " + name);
            }

            loadedFonts.Add(name, font);
            return font;
        }

        public static FontInfo GetFontInfo(SpriteFont font)
        {
            if (font == null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            if (!fontInfos.TryGetValue(font, out var info))
            {
                info = new FontInfo(font);
                fontInfos.Add(font, info);
            }

            return info;
        }

        public readonly struct FontInfo
        {
            private const string TestString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            public readonly SpriteFont Font;

            public readonly float Height;

            internal FontInfo(SpriteFont font)
            {
                this.Font = font;
                this.Height = font.MeasureString(TestString).Y;
            }
        }
    }
}
