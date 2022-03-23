using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    public sealed class GameObject
    {
        private readonly List<Component> components = new List<Component>();

        private readonly List<Behaviour> behaviors = new List<Behaviour>();

        private bool isEnabled;

        private bool isChangingEnableState;

        private GameObjectState state;

        public Transform Transform { get; }

        public string Name { get; set; }

        public bool IsEnabled
        {
            get => this.isEnabled && this.IsAlive;
            set
            {
                if (!this.IsAlive)
                {
                    throw new InvalidOperationException("Can't change the IsEnabled state of a dead game object.");
                }

                if (this.isEnabled == value)
                {
                    return;
                }

                var wasEnabled = this.IsEnabledInHierarchy;
                this.isEnabled = value;
                var newEnabled = this.IsEnabledInHierarchy;

                if (!this.isChangingEnableState && wasEnabled != newEnabled)
                {
                    if (newEnabled)
                    {
                        this.EnableHierarchyRecursive();
                    }
                    else
                    {
                        this.DisableHierarchyRecursive();
                    }
                }
            }
        }

        public bool IsEnabledInHierarchy
        {
            get
            {
                if (!this.IsEnabled)
                {
                    return false;
                }

                var t = this.Transform.Parent;

                while (t != null)
                {
                    if (!t.Owner.IsEnabled)
                    {
                        return false;
                    }

                    t = t.Parent;
                }

                return true;
            }
        }

        public bool IsAlive { get; private set; }

        public Scene Scene { get; internal set; }

        internal GameObjectState State => this.state;

        public GameObject()
        {
            this.state = GameObjectState.Creating;
            this.Transform = new Transform();
            this.components.Add(this.Transform);
            this.Transform.SetupInternal(this);

            this.Name = "New GameObject";
            this.isEnabled = true;
            this.isChangingEnableState = false;
            this.IsAlive = false;

            var containingScene = SceneManager.ActiveScene;
            containingScene.AddGameObject(this);
            this.Transform.SetContainingScene(containingScene);
            this.state = GameObjectState.Created;

            if (SceneManager.IsReady)
            {
                this.OnAwakeInternal();

                if (this.IsEnabledInHierarchy)
                {
                    this.OnEnableInternal();
                }
            }
        }

        public T AddComponent<T>() where T : Component, new()
        {
            if (!this.IsAlive && SceneManager.IsReady)
            {
                throw new InvalidOperationException("Can't add a component to a dead game object.");
            }

            var t = new T();
            t.SetupInternal(this);
            this.components.Add(t);

            if (t is Behaviour b)
            {
                this.behaviors.Add(b);

                if (SceneManager.IsReady)
                {
                    b.OnAwakeInternal();

                    if (this.IsEnabledInHierarchy && b.IsActive)
                    {
                        b.OnEnableInternal();
                    }
                }
            }
            else if(SceneManager.IsReady)
            {
                t.OnAwakeInternal();
            }

            return t;
        }
        
        public T GetComponent<T>() where T : Component
        {
            foreach (var c in this.components)
            {
                if (c is T t)
                {
                    return t;
                }
            }

            return null;
        }

        public void Destroy()
        {
            if (!this.IsAlive && SceneManager.IsReady)
            {
                throw new InvalidOperationException("Can't destroy a dead game object.");
            }

            if (this.IsEnabledInHierarchy)
            {
                this.OnDisableInternal();
            }

            this.OnDestroyInternal();
            this.Transform.containingScene.RemoveGameObject(this);
        }

        internal void DoUpdate()
        {
            Debug.Assert(this.state == GameObjectState.Enabled);

            for (var i = 0; i < this.behaviors.Count; i++)
            {
                var behavior = this.behaviors[i];
                behavior.UpdateInternal();
            }
        }

        internal void RemoveComponent(Component component)
        {
            if (component is Behaviour b)
            {
                if (this.IsEnabledInHierarchy)
                {
                    b.OnDisableInternal();
                }

                component.OnDestroyInternal();
                this.components.Remove(component);
                this.behaviors.RemoveSwapBack(b);
            }
            else
            {
                component.OnDestroyInternal();
                this.components.Remove(component);
            }
        }

        internal void OnAwakeInternal()
        {
            Debug.Assert(this.state == GameObjectState.Created);

            this.IsAlive = true;
            this.state = GameObjectState.Awakening;

            for (var i = 0; i < this.components.Count; i++)
            {
                var c = this.components[i];
                c.OnAwakeInternal();
            }

            this.state = GameObjectState.Disabled;
        }

        internal void OnEnableInternal()
        {
            Debug.Assert(this.state == GameObjectState.Disabled);

            this.isChangingEnableState = true;
            this.isEnabled = true;
            this.state = GameObjectState.Enabling;

            for (var i = 0; i < this.behaviors.Count; i++)
            {
                var b = this.behaviors[i];
                b.OnEnableInternal();
            }

            this.state = GameObjectState.Enabled;
            this.isChangingEnableState = false;

            if (!this.IsEnabledInHierarchy && this.IsAlive)
            {
                // A component might have disabled the game object
                this.OnDisableInternal();
            }
        }

        internal void OnDisableInternal()
        {
            Debug.Assert(this.state == GameObjectState.Enabled);

            this.isChangingEnableState = true;
            this.isEnabled = false;
            this.state = GameObjectState.Disabling;

            for (var i = 0; i < this.behaviors.Count; i++)
            {
                var b = this.behaviors[i];
                b.OnDisableInternal();
            }

            this.state = GameObjectState.Disabled;
            this.isChangingEnableState = false;

            if (this.IsEnabledInHierarchy && this.IsAlive)
            {
                // A component might have enabled the game object
                this.OnEnableInternal();
            }
        }

        internal void OnDestroyInternal()
        {
            Debug.Assert(this.state == GameObjectState.Disabled);

            this.IsAlive = false;
            this.state = GameObjectState.Destroying;

            for (var i = 0; i < this.components.Count; i++)
            {
                var c = this.components[i];
                c.OnDestroyInternal();
            }

            this.state = GameObjectState.Destroyed;
        }

        private void EnableHierarchyRecursive()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            Debug.Assert(this.IsEnabledInHierarchy);

            this.OnEnableInternal();

            for (var i = 0; i < this.Transform.ChildCount; i++)
            {
                this.Transform.GetChild(i).Owner.EnableHierarchyRecursive();
            }
        }

        private void DisableHierarchyRecursive()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            Debug.Assert(this.IsEnabledInHierarchy);

            this.OnDisableInternal();

            for (var i = 0; i < this.Transform.ChildCount; i++)
            {
                this.Transform.GetChild(i).Owner.DisableHierarchyRecursive();
            }
        }

        internal enum GameObjectState
        {
            /// <summary>
            /// The game object is still inside the constructor call.
            /// </summary>
            Creating,

            /// <summary>
            /// The game object was created, but OnAwake has not yet been called.
            /// </summary>
            Created,

            /// <summary>
            /// The game object is being awakened.
            /// </summary>
            Awakening,

            /// <summary>
            /// The game object is being enabled.
            /// </summary>
            Enabling,

            /// <summary>
            /// The game object is enabled.
            /// </summary>
            Enabled,

            /// <summary>
            /// The game object is enabled and currently updating.
            /// </summary>
            Updating,

            /// <summary>
            /// The game object is being disabled.
            /// </summary>
            Disabling,

            /// <summary>
            /// The game object is disabled.
            /// </summary>
            Disabled,

            /// <summary>
            /// The game object is being destroyed.
            /// </summary>
            Destroying,

            /// <summary>
            /// The game object is destroyed.
            /// </summary>
            Destroyed
        }
    }
}
