using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;

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
            
            CreateFpsText();

            Collider.ShowGizmos = true;
            

            var sceneManagerGo = new GameObject("Scene Manager");
            var sceneManagerComp = sceneManagerGo.AddComponent<SceneManagerComponent>();
            sceneManagerComp.SceneLoaderTypes.AddRange(new[] { typeof(LoadScene0), typeof(LoadScene1) });

            // If needed, uncomment the following lines to disable the frame lock (60 fps), required for performance tests
            //engine.VSync = false;
            //engine.IsFixedTimeStep = false;

            // Create InputHandler and PlayerComponent
            
            // Start the engine, this call blocks until the game is closed
            engine.Run();
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject();
            var cameraComp = cameraGo.AddComponent<Camera>();
            cameraComp.Height = 50;
            var cameraController = cameraGo.AddComponent<CameraController>();
            //var cameraSway = cameraGo.AddComponent<SwayComponent>();
            //cameraSway.MaxSway = MathF.PI * 0.25f;
            //cameraSway.SwaySpeed = 0f; //MathF.PI * 0.05f;
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
            fpsGo.Transform.LossyScale = Vector2.One * 1f;
            fpsGo.AddComponent<FpsCounterComponent>();
        }

        private static void CreateMap()
        {
            var tileMapGo = new GameObject();

            var tileMapComp = tileMapGo.AddComponent<TileMap>();
            tileMapComp.LoadFromContent("test_map");
        }
        
        private static void CreateGamePadTest()
        {
            var playerGo = new GameObject();
            playerGo.Transform.Position = new Vector2(5, 5);

            var playerRenderer = playerGo.AddComponent<SpriteRenderer>();
            playerRenderer.LoadFromContent("Smiley");
            
            var playerCollider = playerGo.AddComponent<PolygonCollider>();
            playerCollider.Outline = new[]
            {
                new Vector2(-0.5f, -0.5f), 
                new Vector2(0.5f, -0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f)
            };
            playerCollider.Friction = 0.0f;

            var playerSandTest = playerGo.AddComponent<SandInteractionTest>();
            
            var playerRB = playerGo.AddComponent<RigidBody>();
            playerRB.IsKinematic = false;
            playerRB.FreezeRotation = true;
            
            playerGo.AddComponent<PlayerControlComponent>();

            // And some ground
            //var groundGo = new GameObject();
            //groundGo.Transform.LocalPosition = new Vector2(0f, -302);
            //groundGo.Transform.LossyScale = new Vector2(600, 600);

            //var groundColl = groundGo.AddComponent<CircleCollider>();
            //groundColl.Radius = 1;

            //var groundRenderer = groundGo.AddComponent<SpriteRenderer>();
            //groundRenderer.LoadFromContent("Smiley");

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

        /// <summary>
        /// Creates a physics test scene with circle collider
        /// </summary>
        /// <param name="countX">The amount of collider to place horizontally.</param>
        /// <param name="countY">The amount of collider to place vertically.</param>
        private static void CreatePhysicsTest(int countX, int countY)
        {
            // Create a large smiley as ground, a round ground makes for more interesting collider behavior
            var groundGo = new GameObject();
            groundGo.Transform.LocalPosition = new Vector2(0f, -30);
            groundGo.Transform.LossyScale = new Vector2(60, 60);

            var groundCol = groundGo.AddComponent<CircleCollider>();
            groundCol.Radius = 1;

            var groundRndr = groundGo.AddComponent<SpriteRenderer>();
            groundRndr.LoadFromContent("Smiley");
            
            // Create the rigidBody colliders
            for (var y = 0; y < countY; y++)
            {
                for (var i = 0; i < countX; i++)
                {
                    var circleGo = new GameObject();
                    circleGo.Transform.Position = new Vector2(-countX / 2f + i + 0.5f, 1f + y);
                    circleGo.Transform.LossyScale = new Vector2(2, 1) * 0.6f;

                    // The collider represents the shape of the object, used by the physics engine for correct simulation and queries
                    var circleCol = circleGo.AddComponent<CircleCollider>();
                    circleCol.Radius = 1;

                    // The rigidBody allows the game object to be moved around by the physics engine, makes the collider dynamic
                    var circleRb = circleGo.AddComponent<RigidBody>();
                    //circleRb.IsKinematic = y % 2 == 1;
                    circleRb.FreezeRotation = y % 2 == 0;

                    var circleRndr = circleGo.AddComponent<SpriteRenderer>();
                    circleRndr.Depth = 1f;
                    circleRndr.LoadFromContent("Smiley");
                }
            }
        }

        /// <summary>
        /// Creates a physics test scene with polygon collider
        /// </summary>
        /// <param name="countX">The amount of collider to place horizontally.</param>
        /// <param name="countY">The amount of collider to place vertically.</param>
        private static void CreatePhysicsTest2(int countX, int countY)
        {
            // Create a large smiley as ground, a round ground makes for more interesting collider behavior
            var groundGo = new GameObject();
            groundGo.Transform.LocalPosition = new Vector2(0f, -30);
            groundGo.Transform.LossyScale = new Vector2(60, 60);

            var groundCol = groundGo.AddComponent<CircleCollider>();
            groundCol.Radius = 1;

            var groundRndr = groundGo.AddComponent<SpriteRenderer>();
            groundRndr.LoadFromContent("Smiley");
            
            // Create the rigidBody colliders
            for (var y = 0; y < countY; y++)
            {
                for (var i = 0; i < countX; i++)
                {
                    var shapeGo = new GameObject();
                    shapeGo.Transform.Position = new Vector2(-countX / 2f + i + 0.5f, 1f + y);
                    shapeGo.Transform.LossyScale = new Vector2(2, 1) * 0.6f;    // Stretch the shape, the collider will adapt

                    // The ShapeExampleComponent will create the collider and rigidBody for us, we just need to define the outline
                    var shape = shapeGo.AddComponent<ShapeExampleComponent>();
                    shape.Color = Color.White;
                    shape.Outline = new[]
                    {
                        new Vector2(-0.5f, -0.5f), 
                        //new Vector2(0.5f, -0.5f),
                        new Vector2(0.5f, 0.5f), 
                        new Vector2(-0.5f, 0.5f)
                    };
                }
            }
        }

        private static void CreatePhysicsTest3(int offsetX, int offsetY, int countX, int countY)
        {
            // Create a large smiley as ground, a round ground makes for more interesting collider behavior

            // Create the rigidBody colliders
            for (var y = 0; y < countY; y++)
            {
                for (var i = 0; i < countX; i++)
                {
                    var shapeGo = new GameObject();
                    shapeGo.Transform.Position = new Vector2(offsetX - countX / 2f + i + 0.5f, offsetY + 1f + y);
                    shapeGo.Transform.LossyScale = new Vector2(2, 1) * 0.6f;    // Stretch the shape, the collider will adapt

                    // The ShapeExampleComponent will create the collider and rigidBody for us, we just need to define the outline
                    var shape = shapeGo.AddComponent<ShapeExampleComponent>();
                    shape.Color = Color.Cyan;
                    shape.Outline = new[]
                    {
                        new Vector2(-0.5f, -0.5f),
                        //new Vector2(0.5f, -0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(-0.5f, 0.5f)
                    };
                }
            }
        }

        private static void CreateSandPhysics()
        {
            var sandGo = new GameObject("Sand");
            var sandSim = sandGo.AddComponent<SandSimulation>();
            sandSim.Min = new Vector2(-.5f, -.5f);
            sandSim.Size = Vector2.One * 20f;
            sandSim.ResolutionX = 200;
            sandSim.ResolutionY = 200;
            sandSim.SimulationStepTime = 1f / 40;
            sandSim.MaxLayer = 4;
            sandSim.ColliderLayerMask = LayerMask.FromLayers(0);
            
            sandSim.AddSandSource(new Aabb(12f, 18f, 0.5f, 0.5f));
            sandSim.AddSandSource(new Aabb(5f, 16f, 0.5f, 0.5f));
        }

        /// <summary>
        /// The scene manager component uses loading components to load scenes.<br/>
        /// Add scene loaders by adding the component type to SceneLoaderTypes,<br/>
        /// Once the user presses the number key relating to the index of that scene loader, it will be executed.
        /// </summary>
        private class SceneManagerComponent : Behaviour
        {
            public readonly List<Type> SceneLoaderTypes = new List<Type>();

            private int loadedSceneIndex = -1;

            private Scene loadedScene;

            /// <inheritdoc />
            protected override void Update()
            {
                if (this.loadedSceneIndex == -1 && this.SceneLoaderTypes.Count > 0)
                {
                    this.RunSceneLoader(0);
                    return;
                }

                var state = Keyboard.GetState();

                for (var i = 0; i < this.SceneLoaderTypes.Count && i < 10; i++)
                {
                    var key = (Keys)(Keys.D1 + i);
                    if (state.IsKeyDown(key))
                    {
                        this.RunSceneLoader(i);
                        return;
                    }
                }
            }

            private void RunSceneLoader(int index)
            {
                if (this.loadedSceneIndex == index)
                {
                    return;
                }

                if (this.loadedScene != null)
                {
                    SceneManager.UnloadScene(this.loadedScene);
                }

                var scene = new Scene();

                var loaderGo = new GameObject($"Loader for Scene {index}", scene);
                loaderGo.AddComponent(this.SceneLoaderTypes[index]);

                this.loadedScene = scene;
                this.loadedSceneIndex = index;

                SceneManager.LoadSceneAdditive(this.loadedScene);
            }
        }

        /// <summary>
        /// Loads the default scene with a map, player and camera.
        /// </summary>
        private class LoadScene0 : Component
        {
            protected override void OnAwake()
            {
                Debug.Print("Loaded Scene 0");

                CreateMap();
                CreateCamera();
                CreateSandPhysics();
                CreateGamePadTest();
            }
        }

        /// <summary>
        /// Loads the extended scene with a map, player, camera and lots of rigid bodies.
        /// </summary>
        private class LoadScene1 : Component
        {
            protected override void OnAwake()
            {
                Debug.Print("Loaded Scene 1");
               
                CreateMap();
                CreateCamera();
                CreateGamePadTest();
                CreatePhysicsTest3(10, 20, 10, 10);
            }
        }
    }
}
