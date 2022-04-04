using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public class FpsCounterComponent : Behaviour
    {
        private GuiTextRenderer textRenderer;

        private float avgDeltaTime;
        private float fontSize;

        public float Fps { get; private set; }

        public float FontSize
        {
            get => this.fontSize;
            set
            {
                this.fontSize = value;

                if (this.textRenderer != null)
                {

                }
            }
        }

        /// <inheritdoc />
        protected override void Update()
        {
            this.avgDeltaTime = (Time.DeltaTime + this.avgDeltaTime * 9f) / 10f;
            this.Fps = 1f / this.avgDeltaTime;

            this.textRenderer.Text = $"Total FPS: {MathF.Round(1f / this.avgDeltaTime, 3)}\n" +
                                     $"Update FPS: {MathF.Round(1f / Time.AvgFrameUpdateTime, 3)}\n" +
                                     $"Draw FPS: {MathF.Round(1f / Time.AvgFrameDrawTime, 3)}";
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            this.textRenderer = this.Owner.GetOrAddComponent<GuiTextRenderer>();
            this.textRenderer.FontSize = 16;
            this.textRenderer.ScreenPosition = Vector2.One * 20;
        }
    }
}
