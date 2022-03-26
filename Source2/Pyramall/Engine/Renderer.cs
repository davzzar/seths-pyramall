using Microsoft.Xna.Framework;

namespace Engine
{
    public abstract class Renderer : Behaviour
    {
        private float depth;

        public float Depth
        {
            get => this.depth;
            set => this.depth = MathHelper.Clamp(value, 0f, 1f);
        }

        public abstract void Draw();

        /// <inheritdoc />
        protected override void OnEnable()
        {
            GameEngine.Instance.RenderPipeline.AddRenderer(this);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            GameEngine.Instance.RenderPipeline.RemoveRenderer(this);
        }
    }
}
