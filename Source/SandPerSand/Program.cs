using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Engine;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using SandPerSand.SandSim;
using AppContext = System.AppContext;

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

            #if DEBUG
            CreateFpsText();
            Collider.ShowGizmos = true;
            #endif

            var sceneManagerGo = new GameObject("Scene Manager");
            var sceneManagerComp = sceneManagerGo.AddComponent<SceneManagerComponent>();
            sceneManagerComp.SceneLoaderTypes.AddRange(new[] {typeof(MainMenu), typeof(LoadSceneMultiplayer), typeof(LoadScene1), typeof(LoadScene2), typeof(LoadScene0) });

            CreateGUI();
            CreateMultiGamePadTest();
            CreateBackground();


            // If needed, uncomment the following lines to disable the frame lock (60 fps), required for performance tests
            //engine.VSync = false;
            //engine.IsFixedTimeStep = false;

            // Start the engine, this call blocks until the game is closed
            engine.Run();

        }

        private static GameObject CreateCamera()
        {
            var cameraGo = new GameObject();
            var cameraComp = cameraGo.AddComponent<Camera>();
            cameraComp.Height = 50;
            var cameraController = cameraGo.AddComponent<CameraController2>();
            cameraController.Bounds = Aabb.FromMinMax(new Vector2(-2, -2f), new Vector2(51, 100f));
            //var cameraSway = cameraGo.AddComponent<SwayComponent>();
            //cameraSway.MaxSway = MathF.PI * 0.25f;
            //cameraSway.SwaySpeed = 0f; //MathF.PI * 0.05f;

            return cameraGo;
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

        private static void CreateMap(string mapName)
        {
            var tileMapGo = new GameObject();

            var tileMapComp = tileMapGo.AddComponent<TileMap<MyLayer>>();
            tileMapComp.LoadFromContent(mapName);

            //var rightBorderGo = new GameObject("Right border");
            //rightBorderGo.Transform.Position = new Vector2(-2, 0);
            //var rightBorderComp = rightBorderGo.AddComponent<CameraControlPoint>();
            //rightBorderComp.AffectsVertical = false;
            //
            //var leftBorderGo = new GameObject("Left border");
            //leftBorderGo.Transform.Position = new Vector2(61, 0);
            //var leftBorderComp = leftBorderGo.AddComponent<CameraControlPoint>();
            //leftBorderComp.AffectsVertical = false;
        }

        private static void CreateGUI()
        {
            var guiGo = new GameObject();
            var guiComp = guiGo.AddComponent<GraphicalUserInterface>();
        }

        private static void CreateBackground()
        {
            var backgroundParent = new GameObject();
            var parentSway = backgroundParent.AddComponent<SwayComponent>();

            var backgroundGo = new GameObject();
            //backgroundGo.Transform.Parent = backgroundParent.Transform;
            backgroundGo.Transform.LocalPosition = new Vector2(29.5f,29.5f);
            backgroundGo.Transform.LossyScale = new Vector2(60, 60);
            var background = backgroundGo.AddComponent<SpriteRenderer>();
            background.LoadFromContent("background");
            background.Depth = 2f;
        }

        private static void CreateMultiGamePadTest()
        {
            var managerGo = new GameObject();
            managerGo.AddComponent<GameStateManager>();
            managerGo.AddComponent<PlayersManager>();
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
            var map = GameObject.FindComponent<TileMap<MyLayer>>();
            sandSim.Size = map.Size;
            sandSim.ResolutionX = 200;
            sandSim.ResolutionY = 200;
            sandSim.SimulationStepTime = 1f / 40;
            sandSim.MaxLayer = 4;
            sandSim.ColliderLayerMask = LayerMask.FromLayers(0);
            
            sandSim.AddSandSource(new Aabb(12f, 18f, 0.5f, 0.5f));
            sandSim.AddSandSource(new Aabb(5f, 16f, 0.5f, 0.5f));
        }

        private static void CreateSandPhysics_level_1()
        {
            var sandGo = new GameObject("Sand");
            var sandSim = sandGo.AddComponent<SandSimulation>();
            sandSim.Min = new Vector2(-.5f, -.5f);
            var map = GameObject.FindComponent<TileMap<MyLayer>>();
            sandSim.Size = map.Size;
            Debug.Print(map.Size.ToString());
            sandSim.ResolutionX = 300;
            sandSim.ResolutionY = 300;
            sandSim.SimulationStepTime = 1f / 80;
            sandSim.MaxLayer = 1;
            sandSim.ColliderLayerMask = LayerMask.FromLayers(0);

            sandSim.AddSandSource(new Aabb(29f, 48.5f, 0.2f, 0.2f));
            sandSim.AddSandSource(new Aabb(17f, 52f, 0.5f, 0.5f));
            sandSim.AddSandSource(new Aabb(42f, 52f, 0.5f, 0.5f));

            
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

            public void LoadAt(int index)
            {
                this.RunSceneLoader(index);
            }

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
                CreateMap("debug_map");
                CreateCamera();
                CreateSandPhysics();
                //PlayerGo.Create(PlayerIndex.One, new Vector2(5,5));

                Debug.Print("Loaded Scene 0: Debug with Sand");
            }
        }

        /// <summary>
        /// Loads the extended scene with a map, player, camera and lots of rigid bodies.
        /// </summary>
        private class LoadScene1 : Component
        {
            protected override void OnAwake()
            {
                CreateMap("debug_map");
                CreateCamera();
                //PlayerGo.Create(PlayerIndex.One, new Vector2(5, 5));
                CreatePhysicsTest3(10, 20, 10, 10);
                
                Debug.Print("Loaded Scene 1: Debug with Physics");
            }
        }

        private class LoadScene2 : Component
        {
            protected override void OnAwake()
            {
                CreateMap("controller_testing_map");
                var cameraGo = CreateCamera();
                //var playerGo = PlayerGo.Create(PlayerIndex.One, new Vector2(10, 1));

                var cameraSwitcherComp = cameraGo.AddComponent<CameraSwitcherComponent>();
                //cameraSwitcherComp.Parent = playerGo;
                /*cameraSwitcherComp.GlobalPosition = new Vector2(24.5f, 2.5f);
                cameraSwitcherComp.GlobalHeight = 30f;*/
                cameraSwitcherComp.GlobalPosition = new Vector2(24.5f/2f, 2.5f);
                cameraSwitcherComp.GlobalHeight = 15f;
                cameraSwitcherComp.InputHandler = new InputHandler(PlayerIndex.One);

                // add camera as a player child GO as a hacky way to make it follow them
                //cameraGo.Transform.Parent = playerGo.Transform;
                cameraGo.Transform.LocalPosition = Vector2.Zero; // center camera on player

                Debug.Print("Loaded Scene 2: Controller Testing");
            }
        }

        /// <summary>
        /// Loads the scene with multiplayer test; multiple players would be
        /// automatically created with regard to number of connected GamePad.
        /// </summary>
        private class LoadSceneMultiplayer : Component
        {
            protected override void OnAwake()
            {
                Debug.Print("Loaded Scene 2");
                Debug.Print("Loaded Scene Multiplayer");
                for (int i = 0; i < 4; ++i)
                {
                    Debug.Print("GetState " + i + ":" + GamePad.GetState(i));
                    Debug.Print("GetCap " + i + ":" + GamePad.GetCapabilities(i));
                }

                CreateMap("test_level_1");
                CreateCamera();
                CreateSandPhysics_level_1();
            }
        }

        private class MainMenu : Component
        {
            protected override void OnAwake()
            {
                // The vertical stack panel widget places the children in a vertical line with some spacing
                var panel = new VerticalStackPanel()
                {
                    Spacing = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                // Create a start button with green text
                var startButton = new TextButton
                {
                    Text = "Start",
                    TextColor = Color.LimeGreen,
                    Padding = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ContentHorizontalAlignment = HorizontalAlignment.Center
                };

                // When the start button is clicked, remove the GUI by setting UI.Root to null
                startButton.Click += (sender, e) =>
                {
                    var loadManager = GameObject.FindComponent<SceneManagerComponent>();
                    loadManager.LoadAt(1);
                    UI.Root = null;
                };

                // Add a non-functional settings button
                var settingsButton = new TextButton
                {
                    Text = "Settings",
                    TextColor = Color.CornflowerBlue,
                    Padding = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ContentHorizontalAlignment = HorizontalAlignment.Center
                };

                // Lastly an exit button that kills the app dead
                var exitButton = new TextButton
                {
                    Text = "Exit",
                    TextColor = Color.Red,
                    Padding = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ContentHorizontalAlignment = HorizontalAlignment.Center
                };

                exitButton.Click += (sender, e) =>
                {
                    GameEngine.Instance.Exit();
                };

                // Add the buttons in the correct order
                panel.AddChild(startButton);
                panel.AddChild(settingsButton);
                panel.AddChild(exitButton);

                UI.Root = panel;
                UI.IsMouseVisible = true;
            }
        }
    }
}

