using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SandPerSand.SandSim
{
    public sealed class SandRenderer : Renderer
    {
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

            this.DrawGizmos();

            if (this.sandTexture == null || this.sandTexture.Width != this.SandGrid.ResolutionX ||
                this.sandTexture.Height != this.SandGrid.ResolutionY)
            {
                this.sandTexture?.Dispose();
                this.sandTexture = new Texture2D(Graphics.GraphicsDevice, this.SandGrid.ResolutionX,
                    this.SandGrid.ResolutionY, false, SurfaceFormat.Color);
            }

            if (this.sandTextureData == null ||
                this.sandTextureData.Length != this.sandTexture.Width * this.sandTexture.Height)
            {
                this.sandTextureData = new Color[this.sandTexture.Width * this.sandTexture.Height];
            }

            if (this.StableSandColorOffset == null)
            {
                // Create the random offsets for the texture
                this.StableSandColorOffset = new int[this.sandTexture.Width, this.sandTexture.Height];
                Random r = new Random();
                for (var y = 0; y < this.sandTexture.Height; y++)
                {
                    for (var x = 0; x < this.sandTexture.Width; x++)
                    {
                        this.StableSandColorOffset[x, y] = r.Next(0, 100);
                    }
                }
            }

            //var offset = new Vector2(0, this.SandGrid.CellSize.Y);
            
            for (var y = 0; y < this.SandGrid.ResolutionY; y++)
            {
                for (var x = 0; x < this.SandGrid.ResolutionX; x++)
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
                            // color = this.FlowingSandColor;
                            double RandomValueFromPosition = (double)(StableSandColorOffset[x, (y + (this.FlowingSandOffset)) % this.SandGrid.ResolutionY]);
                            RandomValueFromPosition = 1.0 + ((RandomValueFromPosition - 50) / (1000000)) * this.StableSandColourRange;
                            int Red = (int)(this.FlowingSandColor.R * RandomValueFromPosition) % 256;
                            int Green = (int)(this.FlowingSandColor.G * RandomValueFromPosition) % 256;
                            int Blue = (int)(this.FlowingSandColor.B * RandomValueFromPosition) % 256;
                            color = new Color(Red, Green, Blue);
                        }
                    }
                    else if (cell.HasObstacle && y < this.SandGrid.ResolutionY - 1 && this.SandGrid[x, y+1].HasSand)
                    {
                        color = this.StableSandColor;
                    }
                    else
                    {
                        color = Color.Transparent;
                        // continue;
                    }

                    this.sandTextureData[(this.SandGrid.ResolutionY - y - 1) * this.SandGrid.ResolutionX + x] = color;

                    // var pivot = this.SandGrid.IndexToMinPoint(in x, in y) + offset;
                    // var matrix = Matrix3x3.CreateTRS(pivot, 0f, this.SandGrid.CellSize);
                    // Graphics.Draw(this.Pixel, color, ref matrix, 0.9f);
                }
            }

            this.sandTexture.SetData(this.sandTextureData);

            var size = this.SandGrid.CellSize * new Vector2(this.SandGrid.ResolutionX, this.SandGrid.ResolutionY);
            var pivot = new Vector2(-0.5f, size.Y - 0.5f);// + size * 0.5f;
            var matrix = Matrix3x3.CreateTRS(pivot, 0f, size);
            Graphics.Draw(this.sandTexture, Color.White, ref matrix, 0.9f);
        }

        [Conditional("DEBUG")]
        private void DrawGizmos()
        {
            Gizmos.DrawRect(this.SandGrid.Position + this.SandGrid.Size / 2f, this.SandGrid.Size, Color.Red);

            for (var y = 0; y < this.SandGrid.ResolutionY; y++)
            {
                for (var x = 0; x < this.SandGrid.ResolutionX; x++)
                {
                    var cell = this.SandGrid[x, y];

                    if (cell.HasSand)
                    {
                        var center = this.SandGrid.IndexToCenterPoint(x, y);
                        
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
                        dir *= this.SandGrid.CellSize.Length() * 0.3f;

                        // Draw sand flow direction
                        Gizmos.DrawLine(center, center + dir, Color.Red);
                    }
                }
            }
        }
    }
}
