using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class GuiTextRenderer : GuiRenderer
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

        private string text;
        private SpriteFont font;
        private Vector2 textSize;
        private bool textSizeDirty;
        private bool canRender;

        public string Text
        {
            get => this.text;
            set
            {
                if (value == null)
                {
                    value = "";
                }

                if (this.text == value)
                {
                    return;
                }

                this.text = value;
                this.textSizeDirty = true;
                this.UpdateCanRender();
            }
        }

        public SpriteFont Font
        {
            get => this.font;
            set
            {
                if (this.font == value)
                {
                    return;
                }

                this.font = value ?? throw new ArgumentNullException(nameof(value));
                this.textSizeDirty = true;
                this.UpdateCanRender();
            }
        }

        public float FontSize { get; set; }

        public Color Color { get; set; }

        /// <summary>
        /// The position of the text pivot on the screen.
        /// </summary>
        public Vector2 ScreenPosition { get; set; }

        /// <summary>
        /// Gets or sets how the <see cref="ScreenPosition"/> is interpreted.
        /// </summary>
        public ScreenPositionMode PositionMode { get; set; } = ScreenPositionMode.Absolute;

        /// <summary>
        /// Transform independent text size
        /// </summary>
        public Vector2 TextSize
        {
            get
            {
                if (this.textSizeDirty)
                {
                    this.UpdateTextSize();
                }

                return this.textSize;
            }
        }
        
        public GuiTextRenderer()
        {
            this.text = "";
            this.Color = Color.White;
            this.FontSize = 24f;
            this.textSizeDirty = true;
            this.canRender = false;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (this.canRender)
            {
                var screenPos = this.ScreenPosition;

                if (this.PositionMode == ScreenPositionMode.Relative)
                {
                    screenPos *= Graphics.ScreenSize;
                }

                Graphics.DrawGuiText(this.Font, this.Text, this.FontSize, this.Color, screenPos, this.Depth);
            }
        }

        protected override void OnAwake()
        {
            if (this.font == null)
            {
                this.Font = FontManager.LoadFont("SanSerif");
            }
        }

        private void UpdateTextSize()
        {
            if (this.font != null)
            {
                this.textSize = this.font.MeasureString(this.text);
            }
            else
            {
                this.textSize = Vector2.Zero;
            }

            this.textSizeDirty = false;
        }

        private void UpdateCanRender()
        {
            this.canRender = this.font != null && !string.IsNullOrWhiteSpace(this.text);
        }
    }
}
