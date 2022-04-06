using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using BindingFlags = System.Reflection.BindingFlags;

namespace Engine
{
    public sealed class GameObject
    {
        private readonly List<Component> components = new List<Component>();

        private readonly List<Behaviour> behaviors = new List<Behaviour>();

        private bool isEnabled;

        private bool isChangingEnableState;

        private GameObjectState state;
        private int layer;

        internal event EventHandler<(int oldLayer, int newLayer)> LayerChanged;

        public Transform Transform { get; }

        public string Name { get; set; }

        public int Layer
        {
            get => this.layer;
            set
            {
                if (value < 0 || value > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (this.layer == value)
                {
                    return;
                }

                var oldLayer = this.layer;
                this.layer = value;

                this.OnLayerChanged((oldLayer, this.layer));
            }
        }

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

        [NotNull]
        public Scene Scene { get; internal set; }

        internal GameObjectState State => this.state;

        public GameObject() : this("New GameObject", SceneManager.ScopedScene)
        { }

        public GameObject(string name) : this(name, SceneManager.ScopedScene)
        { }

        public GameObject(string name, Scene scene)
        {
            this.state = GameObjectState.Creating;
            this.Transform = new Transform();
            this.components.Add(this.Transform);
            this.Transform.SetupInternal(this);

            this.Name = "New GameObject";
            this.isEnabled = true;
            this.isChangingEnableState = false;
            this.IsAlive = false;

            this.Scene = scene;
            this.Scene.AddGameObject(this);
            this.Transform.SetContainingScene(this.Scene);
            this.state = GameObjectState.Created;

            if (this.Scene.IsLoaded)
            {
                this.OnAwakeInternal();

                if (this.IsEnabledInHierarchy)
                {
                    this.OnEnableInternal();
                }
            }
        }

        public Component AddComponent(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!typeof(Component).IsAssignableFrom(type))
            {
                throw new ArgumentException("The component type needs to inherit from Component.", nameof(type));
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException("The type cannot be abstract.");
            }

            var constructor = type.GetConstructor(Array.Empty<Type>());

            if (constructor == null)
            {
                throw new ArgumentException("The type must have a public parameter-free constructor.");
            }

            var component = (Component)constructor.Invoke(Array.Empty<object>());
            this.AddComponentInternal(component);
            return component;
        }

        [NotNull]
        public T AddComponent<T>() where T : Component, new()
        {
            if (!this.IsAlive && this.Scene.IsLoaded)
            {
                throw new InvalidOperationException("Can't add a component to a dead game object.");
            }

            var t = new T();
            this.AddComponentInternal(t);
            return t;
        }
        
        [CanBeNull]
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

        [NotNull, ItemNotNull]
        public T[] GetComponents<T>() where T : Component
        {
            var result = new List<T>();

            foreach (var c in this.components)
            {
                if (c is T t)
                {
                    result.Add(t);
                }
            }

            return result.ToArray();
        }

        public void GetComponents<T>(IList<T> items) where T : Component
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var c in this.components)
            {
                if (c is T t)
                {
                    items.Add(t);
                }
            }
        }

        [NotNull]
        public T GetOrAddComponent<T>() where T : Component, new()
        {
            var result = this.GetComponent<T>();

            if (result == null)
            {
                result = this.AddComponent<T>();
            }

            return result;
        }

        public void Destroy()
        {
            if (!this.IsAlive && this.Scene.IsLoaded)
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

        public static T FindComponent<T>() where T : Component
        {
            foreach (var scene in SceneManager.OpenScenes)
            {
                foreach (var go in scene.Objects)
                {
                    var t = go.GetComponent<T>();

                    if (t != null)
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        public static void FindComponents<T>(List<T> items) where T : Component
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var scene in SceneManager.OpenScenes)
            {
                foreach (var go in scene.Objects)
                {
                    go.GetComponents<T>(items);
                }
            }
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

        private void AddComponentInternal(Component component)
        {
            component.SetupInternal(this);
            this.components.Add(component);

            if (component is Behaviour b)
            {
                this.behaviors.Add(b);

                if (this.Scene.IsLoaded && this.state != GameObjectState.Awakening)
                {
                    b.OnAwakeInternal();

                    if (this.IsEnabledInHierarchy && b.IsActive && this.state != GameObjectState.Enabling)
                    {
                        b.OnEnableInternal();
                    }
                }
            }
            else if(this.Scene.IsLoaded && this.state != GameObjectState.Awakening)
            {
                component.OnAwakeInternal();
            }
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

        private void OnLayerChanged((int oldLayer, int newLayer) e)
        {
            this.LayerChanged?.Invoke(this, e);
        }
    }
}
