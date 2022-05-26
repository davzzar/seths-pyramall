using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class Graphics
    {
        private static Graphics instance;
        
        [CanBeNull]
        private Camera currentCamera;

        private Matrix3x3 currentWorldToView;

        private bool isRendering;

        private SpriteBatch spriteBatch;

        internal static SpriteBatch SpriteBatch => instance.spriteBatch;

        /// <summary>
        /// Gets the graphics device that is currently being used to render the scene.
        /// </summary>
        public static GraphicsDevice GraphicsDevice => GameEngine.Instance.GraphicsDevice;

        /// <summary>
        /// Gets the camera that is currently being used for rendering.
        /// </summary>
        [CanBeNull]
        public static Camera CurrentCamera => instance.currentCamera;

        /// <summary>
        /// Gets the current world to view matrix.
        /// </summary>
        public static ref Matrix3x3 CurrentWorldToView => ref instance.currentWorldToView;

        /// <summary>
        /// Gets or sets the background color for the scene.
        /// </summary>
        public static Color BackgroundColor { get; set; } = new Color(37, 9, 25);

        /// <summary>
        /// Gets the size of the screen in pixels.
        /// </summary>
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

        /// <summary>
        /// Draws a texture with respect to the current camera view.
        /// </summary>
        public static void Draw(Texture2D texture, Color color, ref Matrix3x3 matrix, float depth, SpriteEffects effect = SpriteEffects.None)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            scale.X /= texture.Width;
            scale.Y /= texture.Height;
            instance.spriteBatch.Draw(texture, position, null, color, radians, Vector2.Zero, scale, effect, depth);
        }
        
        /// <summary>
        /// Draws a texture with respect to the current camera view.
        /// </summary>
        public static void Draw(Texture2D texture, Color color, Rectangle sourceRectangle, ref Matrix3x3 matrix, float depth, SpriteEffects effect = SpriteEffects.None)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            scale.X /= sourceRectangle.Width;
            scale.Y /= sourceRectangle.Height;
            instance.spriteBatch.Draw(texture, position, sourceRectangle, color, radians, Vector2.Zero, scale, effect, depth);
        }

        /// <summary>
        /// Draws text with respect to the current camera view.
        /// </summary>
        public static void DrawText(SpriteFont font, string text, float fontSize, Color color, ref Matrix3x3 matrix, float depth)
        {
            var viewSpace = instance.currentWorldToView * matrix;
            viewSpace.DecomposeTRS(out var position, out var radians, out var scale);
            var height = FontManager.GetFontInfo(font).Height;
            scale *= fontSize / height;
            instance.spriteBatch.DrawString(font, text, position, color, radians, Vector2.Zero, scale,
                SpriteEffects.None, depth);
        }

        /// <summary>
        /// Draws a texture in screen space.
        /// </summary>
        public static void DrawGuiSprite(Texture2D texture, Color color, Vector2 size, Vector2 position, float rotation, SpriteEffects effect = SpriteEffects.None)
        {
            var scale = Vector2.One;
            scale.X = size.X / texture.Width;
            scale.Y = size.Y / texture.Height;
            instance.spriteBatch.Draw(texture, position, null, color, 0f, Vector2.Zero, scale, effect, 0f);
        }

        /// <summary>
        /// Draws a texture in screen space.
        /// </summary>
        public static void DrawGuiSprite(Texture2D texture, Color color, Rectangle sourceRectangle, Vector2 size, Vector2 position, float rotation, SpriteEffects effect = SpriteEffects.None)
        {
            var scale = Vector2.One;
            scale.X = size.X / sourceRectangle.Width;
            scale.Y = size.Y / sourceRectangle.Height;
            instance.spriteBatch.Draw(texture, position, sourceRectangle, color, 0f,  Vector2.Zero, scale, effect, 0f);
        }

        /// <summary>
        /// Draws text in screen space.
        /// </summary>
        public static void DrawGuiText(SpriteFont font, string text, float fontSize, Color color, Vector2 position, float rotation)
        {
            var height = FontManager.GetFontInfo(font).Height;
            var scale = Vector2.One * (fontSize / height);
            instance.spriteBatch.DrawString(font, text, position, color, rotation, Vector2.Zero, scale,
                SpriteEffects.None, 0f);
        }

        internal static void Init() { }

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
            instance.currentWorldToView = Matrix3x3.Identity;
            instance.isRendering = false;
        }
    }
}
