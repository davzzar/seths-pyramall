using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SandPerSand.SandSim
{
    public sealed class SandRenderer : Renderer
    {
        private Task[] taskBuffer = Array.Empty<Task>();

        private readonly List<RenderTaskEntry> renderTaskEntryCache = new List<RenderTaskEntry>();

        private readonly Stopwatch stopwatch = new Stopwatch();

        private Texture2D sandTexture;

        private Color[] sandTextureData;

        public SandGrid SandGrid { get; set; }

        public Texture2D Pixel { get; private set; }

        public Color SandSourceColor { get; set; } = Color.Orange;

        public Color StableSandColor { get; set; } = new Color(202, 103, 2);
        public int[,] StableSandColorOffset;
        public int StableSandColourRange = 2000;

        public Color FlowingSandColor { get; set; } = new Color(196, 88, 47);
        public uint FlowingSandOffset = 0;

        public int MaxLayer { get; set; } = 1;

        /// <inheritdoc />
        protected override void OnAwake()
        {
            base.OnAwake();

            if (this.Pixel == null)
            {
                this.Pixel = new Texture2D(Graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                this.Pixel.SetData(new[] { Color.White });
            }
        }

        /// <inheritdoc />
        public override void Draw()
        {
            FlowingSandOffset++;

            if (this.SandGrid == null)
            {
                return;
            }
            
            this.stopwatch.Restart();

            this.DrawGizmos();
            
            if (this.sandTexture == null || this.sandTexture.Width != this.SandGrid.ResolutionX ||
                this.sandTexture.Height != this.SandGrid.ResolutionY)
            {
                this.sandTexture?.Dispose();

                this.sandTexture = new Texture2D(Graphics.GraphicsDevice, this.SandGrid.ResolutionX,
                    this.SandGrid.ResolutionY, false, SurfaceFormat.Color);
            }

            if (this.sandTextureData == null ||
                this.sandTextureData.Length != this.SandGrid.ResolutionX * this.SandGrid.ResolutionY)
            {
                this.sandTextureData = new Color[this.SandGrid.ResolutionX * this.SandGrid.ResolutionY];
            }

            if (this.StableSandColorOffset == null)
            {
                // Create the random offsets for the texture
                this.StableSandColorOffset = new int[this.SandGrid.ResolutionX, this.SandGrid.ResolutionY];
                Random r = new Random();
                for (var y = 0; y < this.SandGrid.ResolutionY; y++)
                {
                    for (var x = 0; x < this.SandGrid.ResolutionX; x++)
                    {
                        this.StableSandColorOffset[x, y] = r.Next(0, 100);
                    }
                }
            }

            var tc = Math.Min(4, ThreadPool.ThreadCount);
            var range = this.SandGrid.ResolutionY / tc;

            if (this.taskBuffer.Length != tc - 1)
            {
                this.taskBuffer = new Task[tc - 1];
            }

            while (this.renderTaskEntryCache.Count < tc - 1)
            {
                this.renderTaskEntryCache.Add(new RenderTaskEntry(this));
            }

            for (var i = 0; i < tc - 1; i++)
            {
                //var i1 = i;
                //tasks[i] = Task.Run(() => this.DrawSandBufferPart(range * i1, range * (i1 + 1)));

                var renderTaskEntry = this.renderTaskEntryCache[i];
                renderTaskEntry.Start = range * i;
                renderTaskEntry.End = range * (i + 1);
                this.taskBuffer[i] = Task.Run(renderTaskEntry.RunAction);
            }

            this.DrawSandBufferPart(range * (tc - 1), this.SandGrid.ResolutionY);

            for (var i = 0; i < this.taskBuffer.Length; i++)
            {
                this.taskBuffer[i].Wait();
            }

            this.sandTexture.SetData(this.sandTextureData);

            var size = this.SandGrid.CellSize * new Vector2(this.SandGrid.ResolutionX, this.SandGrid.ResolutionY);
            var pivot = new Vector2(-0.5f, size.Y - 0.5f);// + size * 0.5f;
            var matrix = Matrix3x3.CreateTRS(pivot, 0f, size);
            Graphics.Draw(this.sandTexture, Color.White, ref matrix, 0.9f);

            this.stopwatch.Stop();
            FpsCounterComponent.SandTime = (float)this.stopwatch.Elapsed.TotalSeconds;
        }

        private class RenderTaskEntry
        {
            public int Start, End;

            public readonly SandRenderer Owner;

            public readonly Action RunAction;

            public RenderTaskEntry(SandRenderer owner)
            {
                this.Start = 0;
                this.End = 1;
                this.Owner = owner;
                this.RunAction = this.Run;
            }

            public void Run()
            {
                this.Owner.DrawSandBufferPart(this.Start, this.End);
            }
        }

        [Conditional("DEBUG")]
        private void DrawGizmos()
        {
            Gizmos.DrawRect(this.SandGrid.Position + this.SandGrid.Size / 2f, this.SandGrid.Size, Color.Red);

            // Draw the sand flow as directional lines
            /*var resX = this.SandGrid.ResolutionX;
            var resY = this.SandGrid.ResolutionY;
            var arrowLength = this.SandGrid.CellSize.Length() * 0.3f;

            for (var y = 0; y < resY; y++)
            {
                for (var x = 0; x < resX; x++)
                {
                    var cell = this.SandGrid[x, y];

                    if (cell.HasSand)
                    {
                        var hFlow = cell.HorizontalFlow;
                        var vFlow = cell.VerticalFlow;

                        if (hFlow == HorizontalFlow.None && vFlow == VerticalFlow.None)
                        {
                            continue;
                        }

                        var dir = Vector2.Zero;

                        if (hFlow != HorizontalFlow.None)
                        {
                            dir.X = ((int)hFlow) - 2;
                        }

                        if (vFlow != VerticalFlow.None)
                        {
                            dir.Y = ((int)vFlow) - 2;
                        }

                        dir.Normalize();
                        dir *= arrowLength;

                        var center = this.SandGrid.IndexToCenterPoint(x, y);

                        // Draw sand flow direction
                        Gizmos.DrawLine(center, center + dir, Color.Red);
                    }
                }
            }*/
        }

        private void DrawSandBufferPart(int startY, int endY)
        {
            var resX = this.SandGrid.ResolutionX;
            var resY = this.SandGrid.ResolutionY;

            for (var y = startY; y < endY; y++)
            {
                for (var x = 0; x < resX; x++)
                {
                    var cell = this.SandGrid[x, y];
                    Color color;

                    if (cell.HasSand)
                    {
                        if (cell.IsSandSource)
                        {
                            color = this.SandSourceColor;
                        }
                        else if (cell.IsSandStable)
                        {
                            // color = this.StableSandColor;
                            double RandomValueFromPosition = (double)(StableSandColorOffset[x, y]);
                            RandomValueFromPosition = 1.0 + ((RandomValueFromPosition - 50) / (1000000)) * this.StableSandColourRange;
                            int Red   = (int)(this.StableSandColor.R * RandomValueFromPosition) % 256;
                            int Green = (int)(this.StableSandColor.G * RandomValueFromPosition) % 256;
                            int Blue  = (int)(this.StableSandColor.B * RandomValueFromPosition) % 256;
                            color = new Color(Red, Green, Blue);
                        }
                        else
                        {
                            // color = Color.Lerp(this.StableSandColor, this.FlowingSandColor, (cell.Layer + 1) / (float)this.MaxLayer);
                            // color = this.FlowingSandColor;
                            double RandomValueFromPosition = (double)(StableSandColorOffset[x, (y + (this.FlowingSandOffset)) % this.SandGrid.ResolutionY]);
                            RandomValueFromPosition = 1.0 + ((RandomValueFromPosition - 50) / (1000000)) * this.StableSandColourRange;
                            int Red = (int)(this.FlowingSandColor.R * RandomValueFromPosition) % 256;
                            int Green = (int)(this.FlowingSandColor.G * RandomValueFromPosition) % 256;
                            int Blue = (int)(this.FlowingSandColor.B * RandomValueFromPosition) % 256;
                            color = new Color(Red, Green, Blue);
                        }
                    }
                    else if (cell.HasObstacle && y < resY - 1 && this.SandGrid[x, y+1].HasSand)
                    {
                        color = this.StableSandColor;
                    }
                    else
                    {
                        color = Color.Transparent;
                        // continue;
                    }

                    this.sandTextureData[(resY - y - 1) * resX + x] = color;

                    // var pivot = this.SandGrid.IndexToMinPoint(in x, in y) + offset;
                    // var matrix = Matrix3x3.CreateTRS(pivot, 0f, this.SandGrid.CellSize);
                    // Graphics.Draw(this.Pixel, color, ref matrix, 0.9f);
                }
            }
        }
    }
}
