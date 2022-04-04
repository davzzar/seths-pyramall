using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class TileRenderer : Renderer
    {
        public Texture2D Texture { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public Color Color { get; set; } = Color.White;

        public override void Draw()
        {
            if (this.Texture == null)
            {
                return;
            }
            var matrix = this.Transform.LocalToWorld * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));
            Graphics.DrawTile(Texture, SourceRectangle, Color, ref matrix, this.Depth);
        }
    }
}
