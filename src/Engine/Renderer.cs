using Microsoft.Xna.Framework;

namespace Engine
{
    public abstract class Renderer : Behaviour
    {
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
