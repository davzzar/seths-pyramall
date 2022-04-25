using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class Graphics
    {
        private static Graphics instance;
        
        private Camera currentCamera;

        private Matrix3x3 currentWorldToView;

        private bool isRendering;

        private SpriteBatch spriteBatch;

        public static GraphicsDevice GraphicsDevice => GameEngine.Instance.GraphicsDevice;

        internal static SpriteBatch SpriteBatch => instance.spriteBatch;

        public static Camera CurrentCamera => instance.currentCamera;

        public static Color BackgroundColor { get; set; } = new Color(37, 9, 25);

        public static Vector2 ScreenSize
        {
            get
            {
                var viewport = instance.spriteBatch.GraphicsDevice.Viewport;
                return new Vector2(viewport.Width, viewport.Height);
            }
        }

        static Graphics()
        {
            instance = new Graphics();
        }

        private Graphics()
        { }

        public static void Draw(Texture2D texture, Color color, ref Matrix3x3 matrix, float depth)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            scale.X /= texture.Width;
            scale.Y /= texture.Height;
            instance.spriteBatch.Draw(texture, position, null, color, radians, Vector2.Zero, scale, SpriteEffects.None, depth);
        }
        
        public static void Draw(Texture2D texture, Color color, Rectangle sourceRectangle, ref Matrix3x3 matrix, float depth)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            scale.X /= sourceRectangle.Width;
            scale.Y /= sourceRectangle.Height;
            instance.spriteBatch.Draw(texture, position, sourceRectangle, color, radians, Vector2.Zero, scale, SpriteEffects.None, depth);
        }

        public static void DrawText(SpriteFont font, string text, float fontSize, Color color, ref Matrix3x3 matrix, float depth)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            var height = FontManager.GetFontInfo(font).Height;
            scale *= fontSize / height;
            instance.spriteBatch.DrawString(font, text, position, color, radians, Vector2.Zero, scale,
                SpriteEffects.None, depth);
        }

        public static void DrawGuiSprite(Texture2D texture, Color color, Vector2 size, Vector2 position, float rotation)
        {
            var scale = Vector2.One;
            scale.X = size.X / texture.Width;
            scale.Y = size.Y / texture.Height;
            instance.spriteBatch.Draw(texture, position, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public static void DrawGuiSprite(Texture2D texture, Color color, Rectangle sourceRectangle, Vector2 size, Vector2 position, float rotation)
        {
            var scale = Vector2.One;
            scale.X = size.X / sourceRectangle.Width;
            scale.Y = size.Y / sourceRectangle.Height;
            instance.spriteBatch.Draw(texture, position, sourceRectangle, color, 0f,  Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public static void DrawGuiText(SpriteFont font, string text, float fontSize, Color color, Vector2 position, float rotation)
        {
            var height = FontManager.GetFontInfo(font).Height;
            var scale = Vector2.One * (fontSize / height);
            instance.spriteBatch.DrawString(font, text, position, color, rotation, Vector2.Zero, scale,
                SpriteEffects.None, 0f);
        }

        public static void Init() { }

        internal static void BeginRender(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (instance.isRendering)
            {
                throw new InvalidOperationException(
                    "Can't call BeginRender multiple times without calling EndRender in between.");
            }

            if (instance.spriteBatch == null)
            {
                instance.spriteBatch = new SpriteBatch(GraphicsDevice);
            }

            instance.isRendering = true;

            instance.currentCamera = camera;
            instance.currentWorldToView =
                Matrix3x3.CreateScale(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)) *
                CurrentCamera.WorldToCameraSpace;
        }

        internal static void EndRender()
        {
            if (!instance.isRendering)
            {
                throw new InvalidOperationException("Can't call EndRender without calling BeginRender first.");
            }

            instance.currentCamera = null;
            instance.isRendering = false;
        }
    }
}
