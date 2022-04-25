namespace Engine
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public sealed class GuiTextRenderer : GuiRenderer
    {
        private string text;
        private SpriteFont font;
        private Vector2 textSize;
        private bool textSizeDirty;
        private bool canRender;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get => this.text;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
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

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public float FontSize { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the position of the text pivot on the screen.
        /// </summary>
        public Vector2 ScreenPosition { get; set; }

        /// <summary>
        /// Gets or sets how the <see cref="ScreenPosition"/> is interpreted.
        /// </summary>
        public ScreenPositionMode PositionMode { get; set; } = ScreenPositionMode.Absolute;

        /// <summary>
        /// Gets the transform independent text size
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GuiTextRenderer"/> class.
        /// </summary>
        public GuiTextRenderer()
        {
            this.text = string.Empty;
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

        /// <inheritdoc />
        protected override void OnAwake()
        {
            if (this.font == null)
            {
                this.Font = FontManager.LoadFont("SanSerif");
            }
        }

        /// <summary>
        /// Updates the text size with the exact pixel size.
        /// </summary>
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

        /// <summary>
        /// Updates the canRender flag.
        /// </summary>
        private void UpdateCanRender()
        {
            this.canRender = this.font != null && !string.IsNullOrWhiteSpace(this.text);
        }

        /// <summary>
        /// The screen position mode.
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
    }
}
