using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Engine
{
    public static class SceneManager
    {
        private static Scene activeScene;

        private static readonly List<Scene> openScenes;

        private static readonly List<Scene> scenesToAdd;

        private static readonly List<Scene> scenesToAddBackBuffer;

        private static readonly List<Scene> scenesToRemove;

        private static readonly List<Scene> scenesToRemoveBackBuffer;

        [CanBeNull]
        private static Scene scopedScene;

        internal static bool IsReady { get; private set; }

        public static Scene ActiveScene
        {
            get => activeScene;
            set
            {
                if (activeScene == value)
                {
                    return;
                }

                if (!value.IsLoaded)
                {
                    throw new InvalidOperationException("Can't set a scene as active scene when it's not yet loaded.");
                }

                activeScene = value ?? throw new ArgumentNullException(nameof(value));
                OnActiveSceneChanged();
            }
        }

        /// <summary>
        /// The scene that matches the current context.<br/>
        /// During <see cref="Component.OnAwake"/>, <see cref="Component.OnDestroy"/>, <see cref="Behaviour.Update"/>, <see cref="Behaviour.OnEnable"/>, <see cref="Behaviour.OnDisable"/>, <see cref="Renderer.Draw"/> and similar callbacks, the <see cref="ScopedScene"/> is the scene to which the current component belongs.<br/>
        /// Otherwise it is the <see cref="ActiveScene"/>
        /// </summary>
        [NotNull]
        public static Scene ScopedScene
        {
            get => scopedScene ?? ActiveScene;
            internal set => scopedScene = value;
        }

        public static IReadOnlyList<Scene> OpenScenes { get; }

        public static event EventHandler ActiveSceneChanged;

        static SceneManager()
        {
            openScenes = new List<Scene>();
            OpenScenes = openScenes.AsReadOnly();

            scenesToAdd = new List<Scene>();
            scenesToAddBackBuffer = new List<Scene>();
            scenesToRemove = new List<Scene>();
            scenesToRemoveBackBuffer = new List<Scene>();

            activeScene = new Scene();
            activeScene.Name = "Initial Scene";
            openScenes.Add(activeScene);
        }

        public static void LoadScene(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            scenesToAdd.Clear();
            scenesToRemove.Clear();

            scenesToAdd.Add(scene);
            scenesToRemove.AddRange(openScenes);
        }

        public static void LoadSceneAdditive(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (openScenes.Contains(scene))
            {
                throw new InvalidOperationException("The scene is already loaded.");
            }

            if (scenesToAdd.Contains(scene))
            {
                throw new InvalidOperationException("The scene is already being loaded.");
            }

            scenesToAdd.Add(scene);
        }

        public static void UnloadScene(Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            if (!openScenes.Contains(scene))
            {
                throw new InvalidOperationException("The scene is not loaded.");
            }

            if (scenesToRemove.Contains(scene))
            {
                throw new InvalidOperationException("The scene is already being unloaded.");
            }

            if (openScenes.Count <= 1)
            {
                throw new InvalidOperationException("Can't remove the last remaining scene, use LoadScene to replace a scene instead.");
            }

            scenesToRemove.Add(scene);
        }

        internal static void DoUpdate()
        {
            Debug.Assert(IsReady);

            while (scenesToAdd.Count > 0 || scenesToRemove.Count > 0)
            {
                // Work on a local copy in case the scene loading causes another change in the scene graph
                scenesToAddBackBuffer.Clear();
                scenesToRemoveBackBuffer.Clear();
                scenesToAddBackBuffer.AddRange(scenesToAdd);
                scenesToRemoveBackBuffer.AddRange(scenesToRemove);

                // First unload all scenes that are no longer needed
                foreach (var scene in scenesToRemoveBackBuffer)
                {
                    Debug.Assert(openScenes.Contains(scene));

                    scopedScene = scene;
                    scene.OnUnload();
                    scopedScene = null;

                    openScenes.Remove(scene);

                    if (scene == activeScene)
                    {
                        activeScene = openScenes.Count > 0 ? openScenes[0] : null;
                    }
                }

                // Then load all new scenes
                foreach (var scene in scenesToAddBackBuffer)
                {
                    Debug.Assert(!openScenes.Contains(scene));

                    if (activeScene == null)
                    {
                        activeScene = scene;
                    }

                    openScenes.Add(scene);

                    scopedScene = scene;
                    scene.OnLoad();
                    scopedScene = null;
                }

                scenesToAdd.Clear();
                scenesToRemove.Clear();
                scenesToAddBackBuffer.Clear();
                scenesToRemoveBackBuffer.Clear();
            }

            // Finally, just do a normal update on all open scenes
            foreach(var scene in openScenes)
            {
                scopedScene = scene;
                scene.DoUpdate();
            }

            scopedScene = null;
        }

        internal static void Init()
        {
            IsReady = true;
            activeScene.OnLoad();
        }

        private static void OnActiveSceneChanged()
        {
            ActiveSceneChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
