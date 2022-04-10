using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Represents a 2D circle shape.
    /// </summary>
    public struct Circle : IArea
    {
        private Vector2 center;

        private float radius;

        public Vector2 Center
        {
            readonly get => this.center;
            set => this.center = value;
        }

        public float Radius
        {
            readonly get => this.radius;
            set => this.radius = MathF.Abs(value);
        }

        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = MathF.Abs(radius);
        }

        /// <inheritdoc />
        public readonly Aabb Bounds => new Aabb(this.center, new Vector2(2f * this.radius));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsPoint(in Vector2 point)
        {
            var sqDist = Vector2.DistanceSquared(this.center, point);
            return sqDist < this.radius * this.radius;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsRect(in Vector2 min, in Vector2 max)
        {
            var x = MathF.Max(MathF.Abs(min.X - this.Center.X), MathF.Abs(max.X - this.Center.X));
            var y = MathF.Max(MathF.Abs(min.Y - this.Center.Y), MathF.Abs(max.Y - this.Center.Y));

            var sqrDist = x * x + y * y;
            return sqrDist <= this.Radius * this.Radius;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsRect(in Vector2 min, in Vector2 max)
        {
            var x = MathHelper.Clamp(this.Center.X, min.X, max.X) - this.Center.X;
            var y = MathHelper.Clamp(this.Center.Y, min.Y, max.Y) - this.Center.Y;

            var sqrDist = x * x + y * y;
            return sqrDist <= this.Radius * this.Radius;
        }
    }
}
