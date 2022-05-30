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
        private static readonly string[] postFixes = new[] { "B", "kB", "MB", "GB", "TB" };

        private GuiTextRenderer textRenderer;

        private float avgDeltaTime;
        private float fontSize;

        private StringBuilder builder = new StringBuilder();

        public float Fps { get; private set; }

        public static float SandTime { get; set; }

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
            this.avgDeltaTime = (Time.UnscaledDeltaTime + this.avgDeltaTime * 9f) / 10f;
            this.Fps = 1f / this.avgDeltaTime;

            this.builder.Clear();
            
            this.builder.Append("Total FPS: ");
            this.builder.Append(MathF.Round(1f / this.avgDeltaTime, 3));
            this.builder.AppendLine("ms");

            this.builder.Append("Update: ");
            this.builder.Append(MathF.Round(Time.AvgFrameUpdateTime * 1000, 3));
            this.builder.AppendLine("ms");

            this.builder.Append("Draw: ");
            this.builder.Append(MathF.Round(Time.AvgFrameDrawTime * 1000, 3));
            this.builder.AppendLine("ms");

            this.builder.Append("Sand: ");
            this.builder.Append(MathF.Round(SandTime * 1000, 3));
            this.builder.AppendLine("ms");

            this.builder.Append("Memory: ");
            this.AppendMemoryString();
            this.builder.AppendLine();

            this.textRenderer.Text = this.builder.ToString();
            //$"Total FPS: {MathF.Round(1f / this.avgDeltaTime, 3)}\n" +
            //$"Update: {MathF.Round(Time.AvgFrameUpdateTime * 1000, 3)}ms\n" +
            //$"Draw: {MathF.Round(Time.AvgFrameDrawTime * 1000, 3)}ms\n" +
            //$"Sand: {MathF.Round(SandTime * 1000, 3)}ms\n" +
            //$"Memory: {this.AppendMemoryString()}";
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            this.textRenderer = this.Owner.GetOrAddComponent<GuiTextRenderer>();
            this.textRenderer.FontSize = 16;
            this.textRenderer.ScreenPosition = Vector2.One * 20;
        }

        private void AppendMemoryString()
        {
            var bytes = (double)GC.GetTotalMemory(false);
            int postFixIndex;

            for (postFixIndex = 0; postFixIndex < postFixes.Length - 1; postFixIndex++)
            {
                if (bytes < 1024)
                {
                    break;
                }

                bytes /= 1024;
            }

            this.builder.Append(Math.Round(bytes, 2).ToString(CultureInfo.InvariantCulture));
            this.builder.Append(postFixes[postFixIndex]);

            //return Math.Round(bytes, 2).ToString(CultureInfo.InvariantCulture) + postFixes[postFixIndex];
        }
    }
}
