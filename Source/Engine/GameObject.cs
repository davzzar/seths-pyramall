using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using BindingFlags = System.Reflection.BindingFlags;

namespace Engine
{
    /// <summary>
    /// The game object represents a single entity and can contain logic and data in the form of <see cref="Component"/>s.<br/>
    /// Every game object contains a <see cref="Transform"/>, belongs to a specific <see cref="Scene"/> and has some metadata such as the <see cref="Name"/> and <see cref="Layer"/>.<br/>
    /// To add logic and data, add components using <see cref="AddComponent{T}"/>.<br/>
    /// To get one or multiple reference(s) to other components on a game object instance, use <see cref="GetComponent{T}"/> or <see cref="GetComponents{T}()"/> respectively.
    /// </summary>
    public class GameObject
    {
        #region Private Fields

        /// <summary>
        /// Contains all components that belong to this game object (including behaviours)
        /// </summary>
        private readonly List<Component> components = new List<Component>();

        /// <summary>
        /// Contains all behaviours that belong to this game object, they are stored separately to make the update call faster.
        /// </summary>
        private readonly List<Behaviour> behaviours = new List<Behaviour>();

        private bool isEnabled;

        private bool isChangingEnableState;

        private GameObjectState state;

        private int layer;

        #endregion

        #region Events

        internal event EventHandler<(int oldLayer, int newLayer)> LayerChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the <see cref="Transform"/> that belongs to this game object instance.
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// Gets or sets a name for this game object instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layer for this game object instance (range:[0, 31], default: 0)
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether this game object is enabled.<br/> 
        /// </summary>
        /// <seealso cref="IsEnabledInHierarchy"/>
        /// <seealso cref="Behaviour.IsActive"/>
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

                this.UpdateEnabledState();

                /*if (!this.isChangingEnableState && wasEnabled != newEnabled)
                {
                    if (newEnabled)
                    {
                        this.EnableHierarchyRecursive();
                    }
                    else
                    {
                        this.DisableHierarchyRecursive();
                    }
                }*/
            }
        }

        /// <summary>
        /// Gets a value indicating whether this game object is enabled in the hierarchy.<br/>
        /// Components of this game object will only receive callbacks if <see cref="IsEnabledInHierarchy"/> is <b>true</b>.
        /// </summary>
        /// <seealso cref="IsEnabled"/>
        /// <seealso cref="Engine.Transform.Parent"/>
        /// <seealso cref="Behaviour.IsActiveInHierarchy"/>/>
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

        /// <summary>
        /// Gets a value indicating whether this game object is alive and can be used.
        /// </summary>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// Gets the scene to which this game object belongs.
        /// </summary>
        [NotNull]
        public Scene Scene { get; internal set; }

        internal GameObjectState State => this.state;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new game object and adds it to the <see cref="SceneManager.ScopedScene"/>.
        /// </summary>
        public GameObject() : this("New GameObject", SceneManager.ScopedScene)
        { }

        /// <summary>
        /// Creates a new game object with a specific name and adds it to the <see cref="SceneManager.ScopedScene"/>
        /// </summary>
        public GameObject(string name) : this(name, SceneManager.ScopedScene)
        { }

        /// <summary>
        /// Creates a new game object with a specific name and adds it to the given scene.
        /// </summary>
        public GameObject(string name, [NotNull]Scene scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            this.state = GameObjectState.Creating;
            this.Transform = new Transform();
            this.components.Add(this.Transform);
            this.Transform.SetupInternal(this);

            this.Name = name ?? string.Empty;
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
                this.UpdateEnabledState();

                /*if (this.IsEnabledInHierarchy)
                {
                    this.OnEnableInternal();
                }*/
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a <see cref="Component"/> of the given type to the game object and returns a reference to the constructed component.
        /// </summary>
        /// <exception cref="ArgumentNullException">The type cannot be null.</exception>
        /// <exception cref="ArgumentException">The type needs to inherit from <see cref="Component"/>, cannot be abstract and must have a public constructor without arguments.</exception>
        /// <exception cref="InvalidOperationException">Can't add a component to a dead game object.</exception>
        /// <seealso cref="AddComponent{T}"/>
        /// <seealso cref="GetOrAddComponent{T}"/>
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

            if (!this.IsAlive && this.Scene.IsLoaded)
            {
                throw new InvalidOperationException("Can't add a component to a dead game object.");
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

        /// <summary>
        /// Adds a <see cref="Component"/> of the given type to the game object and returns a reference to the constructed component.
        /// </summary>
        /// <exception cref="InvalidOperationException">Can't add a component to a dead game object.</exception>
        /// <seealso cref="AddComponent"/>
        /// <seealso cref="GetOrAddComponent{T}"/>
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
        
        /// <summary>
        /// Gets a <see cref="Component"/> of the specific type that is part of this game object or <b>null</b> if the game object doesn't contain a component of that specific type.<br/>
        /// If the game object contains multiple components of that specific type, the first found instance will be returned.
        /// </summary>
        /// <seealso cref="GetComponents{T}()"/>
        /// <seealso cref="GetComponents{T}(IList{T})"/>
        /// <seealso cref="GetOrAddComponent{T}"/>
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

        /// <summary>
        /// Gets all <see cref="Component"/>s of the specific type that are part of this game object.<br/>
        /// Consider using <see cref="GetComponents{T}(IList{T})"/> instead to prevent internal memory allocation.
        /// </summary>
        /// <seealso cref="GetComponent{T}"/>
        /// <seealso cref="GetComponents{T}()"/>
        /// <seealso cref="GetOrAddComponent{T}"/>
        [NotNull, ItemNotNull]
        public T[] GetComponents<T>() where T : Component
        {
            var result = new List<T>();
            this.GetComponents(result);
            return result.ToArray();
        }

        /// <summary>
        /// Gets all <see cref="Component"/>s of the specific type that are part of this game object without allocating extra memory.
        /// </summary>
        /// <exception cref="ArgumentNullException">The items cannot be null.</exception>
        /// <seealso cref="GetComponent{T}"/>
        /// <seealso cref="GetComponents{T}(IList{T})"/>
        /// <seealso cref="GetOrAddComponent{T}"/> 
        public void GetComponents<T>([NotNull]IList<T> items) where T : Component
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

        /// <summary>
        /// Tries to get a <see cref="Component"/> of the specific type or creates a new component if the game object doesn't have one. 
        /// </summary>
        /// <seealso cref="AddComponent"/>
        /// <seealso cref="AddComponent{T}"/>
        /// <seealso cref="GetComponent{T}"/>
        /// <seealso cref="GetComponents{T}()"/>
        /// <seealso cref="GetComponents{T}(IList{T})"/>
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

        /// <summary>
        /// Destroys this game object, all contained components and all children, effectively removing them from the game.
        /// </summary>
        /// <exception cref="InvalidOperationException">Can't destroy a dead game object.</exception>
        public void Destroy()
        {
            if (!this.IsAlive && this.Scene.IsLoaded)
            {
                throw new InvalidOperationException("Can't destroy a dead game object.");
            }

            for (var i = this.Transform.ChildCount - 1; i >= 0; i--)
            {
                this.Transform.Children[i].Owner.Destroy();
            }

            if (this.IsEnabledInHierarchy)
            {
                this.isEnabled = false;
                this.OnDisableInternal(true);
            }

            this.OnDestroyInternal();

            this.Transform.Parent = null;
            this.Transform.containingScene.RemoveGameObject(this);
        }

        /// <summary>
        /// Finds a single <see cref="Component"/> of the specific type in the game, returns the first found instance or <b>null</b> if there exists no instance.
        /// </summary>
        /// <seealso cref="FindComponents{T}()"/>
        /// <seealso cref="FindComponents{T}(IList{T})"/>
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

        /// <summary>
        /// Finds and returns all <see cref="Component"/>s of the specific type in the game.<br/>
        /// Consider using <see cref="FindComponents{T}(IList{T})"/> instead to prevent internal memory allocation.
        /// </summary>
        /// <seealso cref="FindComponent{T}"/>
        /// <seealso cref="FindComponents{T}(IList{T})"/>
        public static T[] FindComponents<T>() where T : Component
        {
            var items = new List<T>();
            FindComponents<T>(items);
            return items.ToArray();
        }

        /// <summary>
        /// Finds and returns all <see cref="Component"/>s of the specific type in the game without allocating extra memory.
        /// </summary>
        /// <exception cref="ArgumentNullException">The items cannot be null.</exception>
        /// <seealso cref="FindComponent{T}"/>
        /// <seealso cref="FindComponents{T}()"/>
        public static void FindComponents<T>(IList<T> items) where T : Component
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

        #endregion

        #region Internal Methods

        internal void DoUpdate()
        {
            Debug.Assert(this.state == GameObjectState.Enabled);

            for (var i = 0; i < this.behaviours.Count; i++)
            {
                var behavior = this.behaviours[i];
                if (behavior.IsActive)
                {
                    behavior.UpdateInternal();
                }
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
                this.behaviours.RemoveSwapBack(b);
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

            for (var i = 0; i < this.behaviours.Count; i++)
            {
                var b = this.behaviours[i];

                if (b.IsActive)
                {
                    b.OnEnableInternal();
                }
            }

            for (var i = this.Transform.ChildCount - 1; i >= 0; i--)
            {
                var go = this.Transform.GetChild(i).Owner;

                if (go.isEnabled)
                {
                    go.OnEnableInternal();
                }
            }

            this.state = GameObjectState.Enabled;
            this.isChangingEnableState = false;

            if (!this.IsEnabledInHierarchy && this.IsAlive)
            {
                // A component might have disabled the game object
                this.OnDisableInternal(false);
            }
        }

        internal void OnDisableInternal(bool isDestroying)
        {
            Debug.Assert(this.state == GameObjectState.Enabled);

            this.isChangingEnableState = true;
            //this.isEnabled = false;
            this.state = GameObjectState.Disabling;

            if (isDestroying)
            {
                this.isEnabled = false;
            }

            for (var i = 0; i < this.behaviours.Count; i++)
            {
                var b = this.behaviours[i];

                if (b.IsActive)
                {
                    b.OnDisableInternal();
                }
            }

            for (var i = this.Transform.ChildCount - 1; i >= 0; i--)
            {
                var go = this.Transform.GetChild(i).Owner;

                if (go.isEnabled)
                {
                    go.OnDisableInternal(isDestroying);
                }
            }

            this.state = GameObjectState.Disabled;
            this.isChangingEnableState = false;

            if (this.IsEnabledInHierarchy && this.IsAlive)
            {
                if (isDestroying)
                {
                    throw new InvalidOperationException(
                        "Can't re-enable a game object during the OnDisable call of it's destruction process.");
                }

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

        internal void UpdateEnabledState()
        {
            if (this.state == GameObjectState.Disabled)
            {
                if (this.IsEnabledInHierarchy)
                {
                    this.isChangingEnableState = true;
                    this.state = GameObjectState.Enabling;

                    for (var i = 0; i < this.behaviours.Count; i++)
                    {
                        var b = this.behaviours[i];

                        if (b.IsActive)
                        {
                            b.OnEnableInternal();
                        }
                    }

                    for (var i = this.Transform.ChildCount - 1; i >= 0; i--)
                    {
                        var go = this.Transform.GetChild(i).Owner;

                        go.UpdateEnabledState();
                    }

                    this.state = GameObjectState.Enabled;
                    this.isChangingEnableState = false;

                    // A behavior might have disabled the game object during the enabling procedure
                    this.UpdateEnabledState();
                }
            }
            else if (this.state == GameObjectState.Enabled)
            {
                if (!this.IsEnabledInHierarchy)
                {
                    this.isChangingEnableState = true;
                    //this.isEnabled = false;
                    this.state = GameObjectState.Disabling;

                    for (var i = 0; i < this.behaviours.Count; i++)
                    {
                        var b = this.behaviours[i];

                        if (b.IsActive)
                        {
                            b.OnDisableInternal();
                        }
                    }

                    for (var i = this.Transform.ChildCount - 1; i >= 0; i--)
                    {
                        var go = this.Transform.GetChild(i).Owner;

                        if (go.isEnabled)
                        {
                            go.UpdateEnabledState();
                        }
                    }

                    this.state = GameObjectState.Disabled;
                    this.isChangingEnableState = false;

                    // A behavior might have enabled the game object during the disabling procedure
                    this.UpdateEnabledState();
                }
            }
            else
            {
                throw new InvalidOperationException("The game object state is invalid for updating the enabled state.");
            }
        }

        #endregion

        #region Private Methods
        
        private void AddComponentInternal(Component component)
        {
            component.SetupInternal(this);
            this.components.Add(component);

            if (component is Behaviour b)
            {
                this.behaviours.Add(b);

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
        
        private void OnLayerChanged((int oldLayer, int newLayer) e)
        {
            this.LayerChanged?.Invoke(this, e);
        }

        #endregion

        #region Data Structures and Enums

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

        #endregion
    }
}
