using System;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// The base class for all components, can be added to <see cref="GameObject"/> instances to add logic and data to it.<br/>
    /// WARNING: Do not call the constructor of a component directly, use <see cref="GameObject.AddComponent{T}"/> instead.
    /// </summary>
    public abstract class Component
    {
        private GameObject owner;

        /// <summary>
        /// Gets the <see cref="GameObject"/> to which this <see cref="Component"/> belongs.
        /// </summary>
        public GameObject Owner => this.owner ??
                                   throw new InvalidOperationException(
                                       "The component is in an invalid state and cannot be accessed.");

        /// <summary>
        /// Gets the <see cref="GameObject.Name"/> of the <see cref="Owner"/>.
        /// </summary>
        public string Name => this.Owner.Name;

        /// <summary>
        /// Gets the <see cref="GameObject.Transform"/> of the <see cref="Owner"/>;
        /// </summary>
        public Transform Transform => this.Owner.Transform;

        /// <summary>
        /// Destroys this <see cref="Component"/>, effectively removing it from the game.<br/>
        /// After destroying the component, all references to it must no longer be used.
        /// </summary>
        public void Destroy()
        {
            if (this.Owner == null)
            {
                throw new InvalidOperationException("Can't destroy a component that is in an invalid state.");
            }

            this.Owner.RemoveComponent(this);
        }

        /// <summary>
        /// Called exactly once when the <see cref="Component"/> is awakened for the first time.<br/>
        /// It is guaranteed that <see cref="OnAwake"/> will be called before all other engine callbacks for this component.<br/>
        /// A component becomes awakened once all of the following conditions are met:<br/>
        /// <list type="bullet">
        /// <item>It is added to a <see cref="GameObject"/></item>
        /// <item>The <see cref="Owner"/> is enabled in hierarchy.</item>
        /// <item>The <see cref="Scene"/> containing the <see cref="Owner"/> is loading or loaded</item>
        /// <item>The game was started using <see cref="Game.Run()"/></item>
        /// </list> 
        /// </summary>
        protected virtual void OnAwake(){}

        /// <summary>
        /// Called exactly once at the end of lifetime of this <see cref="Component"/>.<br/>
        /// It is guaranteed that no more engine callbacks will be called after <see cref="OnDestroy"/> for this component.<br/>
        /// A component becomes destroyed in any of the following cases:<br/>
        /// <list type="bullet">
        /// <item><see cref="Destroy"/> is called on this component.</item>
        /// <item><see cref="GameObject.Destroy"/> is called on the <see cref="Owner"/> or any of its <see cref="Engine.Transform.Parent"/>.</item>
        /// <item>The <see cref="Scene"/> containing the <see cref="Owner"/> is unloading.</item>
        /// <item></item>
        /// </list>
        /// </summary>
        protected virtual void OnDestroy(){}

        internal void SetupInternal(GameObject owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        internal void OnAwakeInternal()
        {
            this.OnAwake();
        }

        internal void OnDestroyInternal()
        {
            this.OnDestroy();
            this.owner = null;
        }
    }
}
