using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Represents a 2D rectangle shape that is axis aligned.
    /// </summary>
    public struct Aabb : IArea
    {
        private Vector2 min;

        private Vector2 max;

        public Vector2 Min
        {
            readonly get => this.min;
            set => this.min = value;
        }

        public Vector2 Max
        {
            readonly get => this.max;
            set => this.max = value;
        }

        public readonly Vector2 Center => (this.min + this.max) / 2f;

        public readonly Vector2 Extend => (this.max - this.min) / 2f;

        public readonly Vector2 Size => (this.max - this.min);

        /// <inheritdoc />
        public readonly Aabb Bounds => this;

        public Aabb(float centerX, float centerY, float sizeX, float sizeY)
        {
            var center = new Vector2(centerX, centerY);
            var extend = new Vector2(MathF.Abs(sizeX), MathF.Abs(sizeY)) / 2f;
            this.min = center - extend;
            this.max = center + extend;
        }

        public Aabb(in Vector2 center, in Vector2 size)
        {
            var extend = new Vector2(MathF.Abs(size.X), MathF.Abs(size.Y)) / 2f;
            this.min = center - extend;
            this.max = center + extend;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsPoint(in Vector2 point)
        {
            return point.X >= this.min.X && point.Y >= this.min.Y &&
                   point.X <= this.max.X && point.Y <= this.max.Y;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IntersectsRect(in Vector2 start, in Vector2 end)
        {
            return this.min.X <= end.X && this.min.Y <= end.Y &&
                   this.max.X >= start.X && this.max.Y >= start.Y;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsRect(in Vector2 start, in Vector2 end)
        {
            return this.min.X <= start.X && this.min.Y <= start.Y &&
                   this.max.X >= end.X && this.max.Y >= end.Y;
        }
    }
}
