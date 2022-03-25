using System;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var engine = new GameEngine();

            // Initialize the scene by adding some game objects and components
            var textGo = new GameObject();
            textGo.Transform.LocalPosition = new Vector2(0.1f, -0.1f);
            
            var textComp = textGo.AddComponent<TextRenderer>();
            textComp.Text = "(0, 0)";
            textComp.Color = Color.Red;
            textComp.Transform.LossyScale = Vector2.One * 0.2f;
            textComp.Depth = 0f;

            var cameraGo = new GameObject();
            cameraGo.Transform.LocalPosition = new Vector2(-2f, 0);
            cameraGo.Transform.LocalRotation = MathHelper.Pi * 0.25f;
            var cameraComp = cameraGo.AddComponent<Camera>();
            cameraComp.Height = 5;
            //cameraGo.AddComponent<SwayComponent>();

            var gridGo = new GameObject();
            var gridComp = gridGo.AddComponent<DrawGridComponent>();
            gridComp.Color = Color.White;
            gridComp.Thickness = 0.05f;

            var smileyGo = new GameObject();
            smileyGo.Transform.LossyScale = new Vector2(2, 1);
            var smiley = smileyGo.AddComponent<SpriteRenderer>();
            smiley.LoadFromContent("Smiley");
            smiley.Depth = 1f;
            smileyGo.AddComponent<SwayComponent>();

            // Start the engine, this call blocks until the game is closed
            engine.Run();
        }
    }

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
            Gizmos.DrawRect(Vector2.Zero, Vector2.One, 0f, Color.Gray);
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
