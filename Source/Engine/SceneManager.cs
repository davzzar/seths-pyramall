using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Engine
{
    public sealed class SceneManager
    {
        private static SceneManager instance;

        static SceneManager()
        {
            instance = new SceneManager();
        }

        private Scene activeScene;

        private readonly List<Scene> openScenes;

        private readonly IReadOnlyList<Scene> readonlyOpenScenes;

        private bool isReady;

        internal static bool IsReady => instance.isReady;

        public static Scene ActiveScene
        {
            get => instance.activeScene;
            set
            {
                if (instance.activeScene == value)
                {
                    return;
                }

                if (!value.IsLoaded)
                {
                    throw new InvalidOperationException("Can't set a scene as active scene when it's not yet loaded.");
                }

                instance.activeScene = value ?? throw new ArgumentNullException(nameof(value));
                OnActiveSceneChanged();
            }
        }

        public static IReadOnlyList<Scene> OpenScenes => instance.readonlyOpenScenes;

        public static event EventHandler ActiveSceneChanged;

        private SceneManager()
        {
            this.openScenes = new List<Scene>();
            this.readonlyOpenScenes = this.openScenes.AsReadOnly();

            this.activeScene = new Scene();
            this.openScenes.Add(this.activeScene);
        }

        public static void LoadScene(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            foreach(var openScene in instance.openScenes)
            {
                openScene.OnUnload();
            }

            instance.openScenes.Clear();
            
            instance.openScenes.Add(scene);
            instance.activeScene = scene;
            scene.OnLoad();
        }

        public static void LoadSceneAdditive(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (instance.openScenes.Contains(scene))
            {
                throw new InvalidOperationException("The scene is already loaded.");
            }

            instance.openScenes.Add(scene);
            scene.OnLoad();
        }

        public static void UnloadScene(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (!instance.openScenes.Contains(scene))
            {
                throw new InvalidOperationException("The scene is not loaded.");
            }

            if (instance.openScenes.Count <= 1)
            {
                throw new InvalidOperationException("Can't remove the last remaining scene, use LoadScene to replace a scene instead.");
            }

            instance.openScenes.Remove(scene);
            scene.OnUnload();

            if (instance.activeScene == scene)
            {
                instance.activeScene = instance.openScenes[0];
            }
        }

        internal static void DoUpdate()
        {
            Debug.Assert(instance.isReady);

            foreach (var scene in OpenScenes)
            {
                scene.DoUpdate();
            }
        }

        internal static void Init()
        {
            instance.isReady = true;
            instance.activeScene.OnLoad();
        }

        private static void OnActiveSceneChanged()
        {
            ActiveSceneChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
