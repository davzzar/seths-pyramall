using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Engine
{
    /// <summary>
    /// Renderer class for rendering a single sprite in screen space
    /// </summary>
    public sealed class GuiSpriteRenderer : GuiRenderer
    {
        /// <summary>
        /// Enum used to differentiate between relative and absolute coordinate modes.
        /// </summary>
        public enum ScreenPositionMode
        {
            /// <summary>
            /// The position is relative to the actual screen size where <b>(0, 0)</b> is the top left corner and <b>(1, 1)</b> is the bottom right corner of the screen.
            /// </summary>
            Relative,

            /// <summary>
            /// The position is absolute in pixel coordinates where <b>(0, 0)</b> is the top left corner and <b>(ScreenSize.Width, ScreenSize.Height)</b> is the bottom right corner of the screen.
            /// </summary>
            Absolute
        }

        private string loadFromContentPath;
        
        /// <summary>
        /// Gets or sets how the position should be interpreted.
        /// </summary>
        public ScreenPositionMode PositionMode { get; set; } = ScreenPositionMode.Absolute;

        /// <summary>
        /// Gets or sets a color tint for the sprite.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the sprite texture.
        /// </summary>
        public Texture2D Texture { get; set; }

        public Vector2 Size { get; set; }

        public Vector2 SizeUnits { get; set; }

        public Vector2 ScreenPosition { get; set; }

        //one unit is 10 percent of the height of the screen
        public Vector2 ScreenPositionUnits { get; set; }

        public Rectangle? SourceRectangle { get; set; }

        public Vector2 Rotation { get; set; }

        public int HorizontalAlignment { get; set; }

        public int VerticalAlignment { get; set; }

        private bool canRender;
        private string path;

        public override void Draw()
        {
            float Unit = Graphics.ScreenSize.Y * 0.1f;
            if (this.Texture == null)
            {
                return;
            }

            if (this.canRender)
            {
                var screenPos = this.ScreenPosition;
                var tmp_size = this.Size;

                if (this.PositionMode == ScreenPositionMode.Relative)
                {
                    screenPos = this.ScreenPosition * Graphics.ScreenSize + this.ScreenPositionUnits * Unit;
                    tmp_size = this.Size * Graphics.ScreenSize + this.SizeUnits * Unit;
                }
                else
                {
                    screenPos = this.ScreenPosition + this.ScreenPositionUnits * Unit;
                    tmp_size = this.Size + this.SizeUnits * Unit;
                }

                if (this.SourceRectangle == null)
                {
                    Graphics.DrawGuiSprite(this.Texture, this.Color, tmp_size, screenPos, 0f);
                }
                else
                {
                    Graphics.DrawGuiSprite(this.Texture, this.Color, this.SourceRectangle.Value, tmp_size, screenPos, 0f);
                }

            }
        }

        public GuiSpriteRenderer()
        {
            this.Color = Color.White;
        }

        //public GuiSpriteRenderer(string path, GuiSpriteRenderer.ScreenPositionMode positionMode, Vector2 screenPosition, Rectangle sourceWindow, Vector2 size)
        //{
        //}

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            this.LoadFromContentPath();
        }

        public void LoadFromContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.loadFromContentPath = path;

            this.LoadFromContentPath();
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPath != null && this.Owner.Scene.IsLoaded && this.IsActiveInHierarchy)
            {
                this.Texture = GameEngine.Instance.Content.Load<Texture2D>(this.loadFromContentPath);
                this.loadFromContentPath = null;
            }
            this.canRender = true;
        }

        public void SetSourceRectangle(int tileId, int tileWidth, int tileHeight)
        {
            if (this.Texture == null)
            {
                throw new InvalidOperationException("Can't set source rectangle before texture is ready!");
            }

            this.SourceRectangle = new Rectangle(
                    tileId * tileWidth % this.Texture.Width,
                    tileId * tileWidth / this.Texture.Width * tileHeight,
                    tileWidth,
                    tileHeight);
        }


    }
}
