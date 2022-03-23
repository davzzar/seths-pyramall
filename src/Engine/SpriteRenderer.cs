using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class SpriteRenderer : Renderer
    {
        public Texture2D Texture { get; set; }

        public Color Color { get; set; }

        /// <inheritdoc />
        public override void Draw()
        {
            if (this.Texture == null)
            {
                return;
            }

            Graphics.Draw(this.Texture, this.Color, ref this.Transform.LocalToWorld);
        }
    }
}
