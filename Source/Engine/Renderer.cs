using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Base class for all renderer that need to render with respect to the camera view.
    /// </summary>
    public abstract class Renderer : Behaviour
    {
        private float depth;

        /// <summary>
        /// Gets or sets the depth value (range: [0, 1]), where 0 is at the front and 1 is at the back.
        /// </summary>
        public float Depth
        {
            get => this.depth;
            set => this.depth = MathHelper.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// This method is called when the renderer needs to draw its content.<br/>
        /// IMPORTANT: Don't call this method yourself! 
        /// </summary>
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
