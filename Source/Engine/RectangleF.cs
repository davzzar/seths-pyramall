using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    public struct RectangleF
    {
        private Vector2 center;
        private Vector2 size;

        public Vector2 Center
        {
            readonly get => this.center;
            set => this.center = value;
        }

        public Vector2 Size
        {
            readonly get => this.size;
            set => this.size = new Vector2(MathF.Abs(value.X), MathF.Abs(value.Y));
        }

        public Vector2 Min => this.center - this.size / 2f;
        
        public Vector2 Max => this.center + this.size / 2f;

        public RectangleF(float centerX, float centerY, float sizeX, float sizeY)
        {
            this.center = new Vector2(centerX, centerY);
            this.size = new Vector2(MathF.Abs(sizeX), MathF.Abs(sizeY));
        }

        public RectangleF(Vector2 center, Vector2 size)
        {
            this.center = center;
            this.size = new Vector2(MathF.Abs(size.X), MathF.Abs(size.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsPoint(Vector2 point)
        {
            point -= this.Min;
            return point.X >= 0f && point.Y >= 0f && point.X <= this.Size.X && point.Y <= this.Size.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsRect(in RectangleF other)
        {
            return this.ContainsPoint(other.Min) && this.ContainsPoint(other.Max);
        }
    }
}
