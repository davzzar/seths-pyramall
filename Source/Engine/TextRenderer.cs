using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Engine
{
    public sealed class TextRenderer : Renderer
    {
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

        public TextRenderer()
        {
            this.text = "";
            this.Color = Color.White;
            this.textSizeDirty = true;
            this.canRender = false;
        }

        protected override void OnAwake()
        {
            if (this.font == null)
            {
                this.Font = FontManager.LoadFont("SanSerif");
            }
        }

        public override void Draw()
        {
            if (this.canRender)
            {
                Graphics.DrawText(this.Font, this.Text, this.Color, ref this.Transform.LocalToWorld, this.Depth);
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
