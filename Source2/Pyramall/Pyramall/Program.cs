using System;
using System.Runtime.InteropServices;
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
            textComp.Depth = 1f;

            var cameraGo = new GameObject();
            var cameraComp = cameraGo.AddComponent<Camera>();
            cameraComp.Height = 10;
            var cameraSway = cameraGo.AddComponent<SwayComponent>();
            cameraSway.MaxSway = MathF.PI * 0.25f;

            var gridGo = new GameObject();
            var gridComp = gridGo.AddComponent<DrawGridComponent>();
            gridComp.Color = Color.White;
            gridComp.Thickness = 0.05f;

            var tileMapGo = new GameObject();
            var tileMapComp = tileMapGo.AddComponent<TileMap>();
            tileMapComp.LoadFromContent("test_map", "sandpersand");

            const int count = 1;

            var smileyParent = new GameObject();
            var parentSway = smileyParent.AddComponent<SwayComponent>();

            for (var i = 0; i < count; i++)
            {
                var smileyGo = new GameObject();
                smileyGo.Transform.Parent = smileyParent.Transform;
                smileyGo.Transform.LocalPosition = Vector2.UnitX;
                smileyGo.Transform.LossyScale = new Vector2(1, 1);
                var smiley = smileyGo.AddComponent<SpriteRenderer>();
                smiley.LoadFromContent("Smiley");
                smiley.Depth = 0f;
                //var smileySway = smileyGo.AddComponent<SwayComponent>();
                //smileySway.SwaySpeed *= 1f + i / (float)count;
            }

            var fpsGo = new GameObject();
            fpsGo.Transform.Position = new Vector2(-2f, 3f);
            fpsGo.AddComponent<FpsCounterComponent>();

            // Hide the console
            // --- Delete if kernel32.dll or user32.dll isn't available on linux
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            // --- End of delete

            // Start the engine, this call blocks until the game is closed
            engine.Run();
        }


        // --- Delete if kernel32.dll or user32.dll isn't available on linux
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        // --- End of delete
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
