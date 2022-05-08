using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Engine
{
    /// <summary>
    /// Represents a collection of game objects.
    /// </summary>
    public sealed class Scene
    {
        private readonly List<GameObject> allObjects = new List<GameObject>();

        private readonly List<GameObject> objectsToAdd = new List<GameObject>();

        private readonly List<GameObject> objectsToRemove = new List<GameObject>();

        private bool isLoaded;

        /// <summary>
        /// Gets or sets a name for this scene, can be used to differentiate various scenes.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a value indicating whether this scene is the active scene, this is always true if this scene is the only loaded scene.
        /// </summary>
        public bool IsActiveScene => SceneManager.ActiveScene == this;

        /// <summary>
        /// Gets a value indicating whether this scene has been loaded and is currently getting event callbacks from the game engine.
        /// </summary>
        public bool IsLoaded => this.isLoaded;

        /// <summary>
        /// Gets a read-only accessor to all game objects that are part of this scene.
        /// </summary>
        public IReadOnlyList<GameObject> Objects => this.allObjects.AsReadOnly();

        /// <summary>
        /// Gets a read-only accessor to all game objects that are part of this scene and don't have a parent.
        /// </summary>
        public IEnumerable<GameObject> RootObjects => this.allObjects.Where(go => go.Transform.Parent == null);

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> type.
        /// </summary>
        public Scene()
        {
            this.Name = "New Scene";
        }

        internal void OnLoad()
        {
            if (!SceneManager.IsReady)
            {
                return;
            }

            this.isLoaded = true;

            // Use for instead of foreach to support game object creation in OnAwake
            for (var i = 0; i < this.allObjects.Count; i++)
            {
                var go = this.allObjects[i];

                if (go.State == GameObject.GameObjectState.Created)
                {
                    go.OnAwakeInternal();
                }
            }

            // Same for OnEnable
            for (var i = 0; i < this.allObjects.Count; i++)
            {
                var go = this.allObjects[i];
                go.UpdateEnabledState();
                /*if (go.IsEnabledInHierarchy && go.State == GameObject.GameObjectState.Disabled)
                {
                    go.OnEnableInternal();
                }*/
            }
        }

        internal void OnUnload()
        {
            if (!SceneManager.IsReady)
            {
                return;
            }

            for (var i = 0; i < this.allObjects.Count; i++)
            {
                var go = this.allObjects[i];
                if (go.IsEnabledInHierarchy)
                {
                    go.OnDisableInternal(true);
                }
            }

            for (var i = 0; i < this.allObjects.Count; i++)
            {
                var go = this.allObjects[i];
                go.OnDestroyInternal();
            }

            this.isLoaded = false;
        }

        internal void DoUpdate()
        {
            for (var i = 0; i < this.allObjects.Count; i++)
            {
                var go = this.allObjects[i];

                if (go.State == GameObject.GameObjectState.Enabled)
                {
                    go.DoUpdate();
                }
            }
        }

        internal void AddGameObject(GameObject go)
        {
            if (go == null)
            {
                throw new ArgumentNullException(nameof(go));
            }

            Debug.Assert(!this.allObjects.Contains(go));

            go.Scene = this;
            this.allObjects.Add(go);
        }

        internal void RemoveGameObject(GameObject go)
        {
            if (go == null)
            {
                throw new ArgumentNullException(nameof(go));
            }

            Debug.Assert(this.allObjects.Contains(go));

            this.allObjects.RemoveSwapBack(go);
        }
    }
}
