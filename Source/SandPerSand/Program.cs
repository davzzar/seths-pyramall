using System;
using System.Runtime.CompilerServices;
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
            CreateCamera();
            CreateOriginMarker();
            CreateFpsText();
            //CreatePerformanceTest(10000);
            CreatePhysicsTest(5, 2);

            // Start the engine, this call blocks until the game is closed
            //engine.VSync = false;
            //engine.IsFixedTimeStep = false;
            
            engine.Run();
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject();
            var cameraComp = cameraGo.AddComponent<Camera>();
            cameraComp.Height = 10;
            var cameraSway = cameraGo.AddComponent<SwayComponent>();
            cameraSway.MaxSway = MathF.PI * 0.25f;
            cameraSway.SwaySpeed = 0f; //MathF.PI * 0.05f;
        }

        private static void CreateOriginMarker()
        {
            var textGo = new GameObject();
            textGo.Transform.LocalPosition = new Vector2(0.1f, -0.1f);
            
            var textComp = textGo.AddComponent<TextRenderer>();
            textComp.Text = "(0, 0)";
            textComp.Color = Color.Red;
            textComp.Transform.LossyScale = Vector2.One * 0.2f;
            textComp.Depth = 1f;

            var gridGo = new GameObject();
            var gridComp = gridGo.AddComponent<DrawGridComponent>();
            gridComp.Color = Color.White;
            gridComp.Thickness = 0.05f;
        }

        private static void CreateFpsText()
        {
            var fpsGo = new GameObject();
            fpsGo.Transform.Position = new Vector2(-2f, 3f);
            fpsGo.Transform.LossyScale = Vector2.One * 100f;
            fpsGo.AddComponent<FpsCounterComponent>();
        }

        private static void CreatePerformanceTest(int count)
        {
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
        }

        private static void CreatePhysicsTest(int countX, int countY)
        {
            var groundGo = new GameObject();
            groundGo.Transform.LocalPosition = new Vector2(0f, -30);
            groundGo.Transform.LossyScale = new Vector2(60, 60);

            var groundCol = groundGo.AddComponent<CircleCollider>();
            groundCol.Radius = 1;

            var groundRndr = groundGo.AddComponent<SpriteRenderer>();
            groundRndr.LoadFromContent("Smiley");
            
            for (var y = 0; y < countY; y++)
            {
                for (var i = 0; i < countX; i++)
                {
                    var circleGo = new GameObject();
                    circleGo.Transform.Position = new Vector2(-countX / 2f + i + 0.5f, 1f + y);
                    circleGo.Transform.LossyScale = new Vector2(2, 1) * 0.6f;

                    var circleCol = circleGo.AddComponent<CircleCollider>();
                    circleCol.Radius = 1;

                    var circleRb = circleGo.AddComponent<RigidBody>();

                    var circleRndr = circleGo.AddComponent<SpriteRenderer>();
                    circleRndr.Depth = 1f;
                    circleRndr.LoadFromContent("Smiley");
                }
            }
        }
    }
}
