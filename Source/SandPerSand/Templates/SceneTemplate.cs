using System;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using SandPerSand.SandSim;
using SandPerSand.Scenes;

namespace SandPerSand
{
    public static partial class Template
    {
        public static void MakeMainController()
        {
            var sceneManagerGo = new GameObject("Scene Manager");
            var sceneManagerComp = sceneManagerGo.AddComponent<SceneLoadManager>();

            var managerGo = new GameObject();
            managerGo.AddComponent<GameStateManager>();
            managerGo.AddComponent<PlayersManager>();
        }

        public static (GameObject go, GraphicalUserInterface guiComp) MakeGameGUI()
        {
            var guiGo = new GameObject();
            var guiComp = guiGo.AddComponent<GraphicalUserInterface>();

            return (guiGo, guiComp);
        }

        public static void MakeGameMap(string mapName)
        {
            var sandGo = new GameObject("Sand");
            var sandSim = sandGo.AddComponent<SandSimulation>();

            var tileMapGo = new GameObject();
            var tileMapComp = tileMapGo.AddComponent<TileMap<MyLayer>>();
            tileMapComp.LoadFromContent(mapName);

            sandSim.Min = new Vector2(-.5f, -.5f);
            var map = GameObject.FindComponent<TileMap<MyLayer>>();
            sandSim.Size = map.Size;
            Debug.Print(map.Size.ToString());
            sandSim.ResolutionX = (int)(map.Size.X * 5);
            sandSim.ResolutionY = (int)(map.Size.Y * 5);
            sandSim.MaxLayer = 1;
            sandSim.ColliderLayerMask = LayerMask.FromLayers(0);
        }

        public static GameObject MakeGameCamera()
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
    }
}
