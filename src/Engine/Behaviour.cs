namespace Engine
{
    public abstract class Behaviour : Component
    {
        private bool isActive = true;

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

        public bool IsActiveInHierarchy => this.IsActive && this.Owner.IsEnabledInHierarchy;

        protected virtual void Update(){}

        protected virtual void OnEnable(){}

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
