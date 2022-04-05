using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Engine
{
    public sealed class Scene
    {
        private readonly List<GameObject> allObjects = new List<GameObject>();

        private readonly List<GameObject> objectsToAdd = new List<GameObject>();

        private readonly List<GameObject> objectsToRemove = new List<GameObject>();

        private bool isLoaded;

        public string Name { get; set; }

        public bool IsActive => SceneManager.ActiveScene == this;

        public bool IsLoaded => this.isLoaded;

        public IReadOnlyList<GameObject> Objects => this.allObjects.AsReadOnly();

        public IEnumerable<GameObject> RootObjects => this.allObjects.Where(go => go.Transform.Parent == null);

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
                if (go.IsEnabledInHierarchy && go.State == GameObject.GameObjectState.Disabled)
                {
                    go.OnEnableInternal();
                }
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
                    go.OnDisableInternal();
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
                go.DoUpdate();
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
