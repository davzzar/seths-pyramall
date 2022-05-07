using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Engine
{
    public sealed class GuiSpriteRenderer : GuiRenderer
    {
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
        public ScreenPositionMode PositionMode { get; set; } = ScreenPositionMode.Absolute;

        public Color Color { get; set; }
        public Texture2D texture { get; set; }
        public Vector2 size { get; set; }
        public Vector2 sizeUnits { get; set; }
        public Vector2 screenPosition { get; set; }

        //one unit is 10 percent of the height of the screen
        public Vector2 screenPositionUnits { get; set; }
        public Rectangle? sourceRectangle { get; set; }
        public Vector2 rotation { get; set; }

        public int horizontalAlignment { get; set; }

        public int verticalAlignment { get; set; }

        private bool canRender;
        public string path;

        public override void Draw()
        {
            float Unit = Graphics.ScreenSize.Y * 0.1f;
            if (this.texture == null)
            {
                return;
            }

            if (this.canRender)
            {
                var screenPos = this.screenPosition;
                var tmp_size = this.size;

                if (this.PositionMode == ScreenPositionMode.Relative)
                {
                    screenPos = this.screenPosition * Graphics.ScreenSize + screenPositionUnits * Unit;
                    tmp_size = this.size * Graphics.ScreenSize + sizeUnits * Unit;
                }
                else
                {
                    screenPos = this.screenPosition + screenPositionUnits * Unit;
                    tmp_size = this.size + sizeUnits * Unit;
                }

                if (this.sourceRectangle == null)
                {
                    Graphics.DrawGuiSprite(this.texture, this.Color, tmp_size, screenPos, 0f);
                }
                else
                {
                    Graphics.DrawGuiSprite(this.texture, this.Color, this.sourceRectangle.Value, tmp_size, screenPos, 0f);
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
                this.texture = GameEngine.Instance.Content.Load<Texture2D>(this.loadFromContentPath);
                this.loadFromContentPath = null;
            }
            this.canRender = true;
        }

        public void SetSourceRectangle(int tileId, int tileWidth, int tileHeight)
        {
            if (this.texture == null)
            {
                throw new InvalidOperationException("Can't set source rectangle before texture is ready!");
            }

            this.sourceRectangle = new Rectangle(
                    tileId * tileWidth % texture.Width,
                    tileId * tileWidth / texture.Width * tileHeight,
                    tileWidth,
                    tileHeight);
        }


    }
}
