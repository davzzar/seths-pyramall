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
        public SandGrid SandGrid { get; set; }

        public Texture2D ObstacleTexture { get; set; }

        public Texture2D SandTexture { get; set; }

        public int MaxLayer { get; set; } = 1;

        /// <inheritdoc />
        protected override void OnAwake()
        {
            base.OnAwake();

            if (this.ObstacleTexture == null)
            {
                this.ObstacleTexture = new Texture2D(Graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                this.ObstacleTexture.SetData(new[] { Color.White });
            }
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (this.SandGrid == null)
            {
                return;
            }

            this.DrawGizmos();

            var offset = new Vector2(0, this.SandGrid.CellSize.Y);
            
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
                            color = Color.Orange;
                        }
                        else if (cell.IsSandStable)
                        {
                            color = Color.Yellow;
                        }
                        else
                        {
                            color = Color.Lerp(Color.Yellow, Color.LightYellow, (cell.Layer + 1) / (float)this.MaxLayer);
                        }
                    }
                    else
                    {
                        continue;
                    }

                    var pivot = this.SandGrid.IndexToMinPoint(in x, in y) + offset;
                    var matrix = Matrix3x3.CreateTRS(pivot, 0f, this.SandGrid.CellSize);
                    Graphics.Draw(this.ObstacleTexture, color, ref matrix, 0.9f);
                }
            }
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
