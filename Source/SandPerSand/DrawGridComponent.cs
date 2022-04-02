using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    /// <summary>
    /// Very simple example component that draws a minimalistic grid... In fact, atm. it's more like a cross but it's an example, okay!?
    /// </summary>
    public class DrawGridComponent : Renderer
    {
        public Color Color { get; set; }

        public float Thickness { get; set; }

        /// <inheritdoc />
        public override void Draw()
        {
            Gizmos.DrawLine(-Vector2.UnitX, Vector2.UnitX, Color.Blue);
            Gizmos.DrawLine(Vector2.UnitX - Vector2.UnitY * 0.05f, Vector2.UnitX + Vector2.UnitY * 0.05f, Color.Blue);
            Gizmos.DrawLine(Vector2.UnitX - Vector2.UnitY * 0.05f, Vector2.UnitX + Vector2.UnitX * 0.1f, Color.Blue);
            Gizmos.DrawLine(Vector2.UnitX + Vector2.UnitY * 0.05f, Vector2.UnitX + Vector2.UnitX * 0.1f, Color.Blue);

            Gizmos.DrawLine(-Vector2.UnitY, Vector2.UnitY, Color.Red);
            Gizmos.DrawLine(Vector2.UnitY - Vector2.UnitX * 0.05f, Vector2.UnitY + Vector2.UnitX * 0.05f, Color.Red);
            Gizmos.DrawLine(Vector2.UnitY - Vector2.UnitX * 0.05f, Vector2.UnitY + Vector2.UnitY * 0.1f, Color.Red);
            Gizmos.DrawLine(Vector2.UnitY + Vector2.UnitX * 0.05f, Vector2.UnitY + Vector2.UnitY * 0.1f, Color.Red);
        }
    }
}