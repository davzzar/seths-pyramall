using System;

namespace Engine
{
    public abstract class Component
    {
        private GameObject owner;

        public GameObject Owner => this.owner ??
                                   throw new InvalidOperationException(
                                       "The component is in an invalid state and cannot be accessed.");

        public string Name => this.Owner.Name;

        public Transform Transform => this.Owner.Transform;

        internal void SetupInternal(GameObject owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public void Destroy()
        {
            if (this.Owner == null)
            {
                throw new InvalidOperationException("Can't destroy a component that is in an invalid state.");
            }

            this.Owner.RemoveComponent(this);
        }

        protected virtual void OnAwake(){}

        protected virtual void OnDestroy(){}

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
