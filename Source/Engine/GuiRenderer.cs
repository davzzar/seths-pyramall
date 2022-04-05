using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    public abstract class GuiRenderer : Behaviour
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
            GameEngine.Instance.RenderPipeline.AddGuiRenderer(this);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            GameEngine.Instance.RenderPipeline.RemoveGuiRenderer(this);
        }
    }
}
