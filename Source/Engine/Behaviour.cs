namespace Engine
{
    /// <summary>
    /// Abstract class for all components that require a callback for <see cref="Update"/>, <see cref="OnEnable"/> and <see cref="OnDisable"/> events.<br/>
    /// Components of this type can be disabled with the <see cref="IsActive"/> flag, preventing <see cref="Update"/> callback from being invoked.
    /// </summary>
    public abstract class Behaviour : Component
    {
        private bool isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Behaviour"/> is active and can receive <see cref="Update"/> callbacks if the <see cref="Component.Owner"/> is enabled in hierarchy.
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                var wasActive = this.IsActiveInHierarchy;
                this.isActive = value;

                if (this.isActive != wasActive)
                {
                    if (this.isActive)
                    {
                        this.OnEnableInternal();
                    }
                    else
                    {
                        this.OnDisableInternal();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Behaviour"/> is active and the <see cref="Component.Owner"/> and all its <see cref="Transform.Parent"/> are enabled.
        /// </summary>
        public bool IsActiveInHierarchy => this.IsActive && this.Owner.IsEnabledInHierarchy;

        /// <summary>
        /// Called once per update loop if <see cref="IsActiveInHierarchy"/> is <c>true</c>.
        /// </summary>
        protected virtual void Update(){}

        /// <summary>
        /// Called once when this <see cref="Behaviour"/> becomes active in hierarchy.
        /// </summary>
        protected virtual void OnEnable(){}

        /// <summary>
        /// Called once when this <see cref="Behaviour"/> becomes inactive in hierarchy.
        /// </summary>
        protected virtual void OnDisable(){}

        internal void UpdateInternal()
        {
            this.Update();
        }

        internal void OnEnableInternal()
        {
            this.OnEnable();
        }

        internal void OnDisableInternal()
        {
            this.OnDisable();
        }
    }
}
