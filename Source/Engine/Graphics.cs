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

        internal static GraphicsDevice GraphicsDevice => GameEngine.Instance.GraphicsDeviceManager.GraphicsDevice;

        internal static SpriteBatch SpriteBatch => instance.spriteBatch;

        public static Camera CurrentCamera => instance.currentCamera;

        static Graphics()
        {
            instance = new Graphics();
        }

        private Graphics()
        { }

        public static void Draw(Texture2D texture, Color color, ref Matrix3x3 matrix)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            scale.X /= texture.Width;
            scale.Y /= texture.Height;
            instance.spriteBatch.Draw(texture, position, null, color, radians, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        public static void DrawText(SpriteFont font, string text, Color color, ref Matrix3x3 matrix)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            var height = FontManager.GetFontInfo(font).Height;
            scale /= height;
            instance.spriteBatch.DrawString(font, text, position, color, radians, Vector2.Zero, scale,
                SpriteEffects.None, 0);
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

            GraphicsDevice.Clear(Color.CornflowerBlue);

            instance.isRendering = true;
            instance.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

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

            instance.spriteBatch.End();
            instance.currentCamera = null;
            instance.isRendering = false;
        }
    }
}
