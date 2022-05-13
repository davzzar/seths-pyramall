using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// The base class for all components, can be added to <see cref="GameObject"/> instances to add logic and data to it.<br/>
    /// WARNING: Do not call the constructor of a component directly, use <see cref="GameObject.AddComponent{T}"/> instead.
    /// </summary>
    public abstract class Component
    {
#if DEBUG

        private readonly List<Action> drawGizmosCallbacks = new List<Action>();

        private bool canRegisterGizmosCallbacks;

#endif
        
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

        public bool IsAlive => this.owner?.IsAlive ?? false;

        /// <summary>
        /// Event that is invoked directly before drawing the gizmos, can be used to inject gizmos draw calls that depend on state affected by other components and physics.
        /// </summary>
        public event Action OnDrawGizmos
        {
            add
            {
#if DEBUG

                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (this.drawGizmosCallbacks.Contains(value))
                {
                    return;
                }

                this.drawGizmosCallbacks.Add(value);

                if (this.canRegisterGizmosCallbacks)
                {
                    GameEngine.Instance.RenderPipeline.OnDrawGizmos += value;
                }

#endif
            }
            remove
            {
#if DEBUG

                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (this.drawGizmosCallbacks.RemoveSwapBack(value) && this.canRegisterGizmosCallbacks)
                {
                    GameEngine.Instance.RenderPipeline.OnDrawGizmos -= value;
                }

#endif
            }
        }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.GetType().Name} \"{this.Name}\"";
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

#if DEBUG
            this.canRegisterGizmosCallbacks = true;

            foreach (var drawGizmosCallback in this.drawGizmosCallbacks)
            {
                GameEngine.Instance.RenderPipeline.OnDrawGizmos += drawGizmosCallback;
            }

#endif
        }

        internal void OnDestroyInternal()
        {
            this.OnDestroy();
            this.owner = null;

            #if DEBUG

            foreach (var drawGizmosCallback in this.drawGizmosCallbacks)
            {
                GameEngine.Instance.RenderPipeline.OnDrawGizmos -= drawGizmosCallback;
            }

            this.canRegisterGizmosCallbacks = false;
            this.drawGizmosCallbacks.Clear();

            #endif
        }
    }
}
