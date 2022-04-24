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

        /// <summary>
        /// Gets or sets the minimum point of the <see cref="Aabb"/>, this is always equals to <see cref="Center"/> - <see cref="Extend"/>.
        /// </summary>
        public Vector2 Min
        {
            readonly get => this.min;
            set
            {
                this.min = value;
                this.max = Vector2.Max(this.min, this.max);
            }
        }

        /// <summary>
        /// Gets or sets the maximum point of the <see cref="Aabb"/>, this is always equals to <see cref="Center"/> + <see cref="Extend"/>.
        /// </summary>
        public Vector2 Max
        {
            readonly get => this.max;
            set
            {
                this.max = value;
                this.min = Vector2.Min(this.min, this.max);
            }
        }

        /// <summary>
        /// Gets the center of the <see cref="Aabb"/>.
        /// </summary>
        public readonly Vector2 Center => (this.min + this.max) / 2f;

        /// <summary>
        /// Gets the extend of the <see cref="Aabb"/>, this is always equals to <see cref="Size"/> / 2.
        /// </summary>
        public readonly Vector2 Extend => (this.max - this.min) / 2f;

        /// <summary>
        /// Gets the size of the <see cref="Aabb"/>, this is always equals to <see cref="Extend"/> * 2.
        /// </summary>
        public readonly Vector2 Size => (this.max - this.min);

        /// <inheritdoc />
        public readonly Aabb Bounds => this;

        /// <summary>
        /// Initializes a new instance of <see cref="Aabb"/> with the given center point and size.
        /// </summary>
        public Aabb(float centerX, float centerY, float sizeX, float sizeY)
        {
            var center = new Vector2(centerX, centerY);
            var extend = new Vector2(MathF.Abs(sizeX), MathF.Abs(sizeY)) / 2f;
            this.min = center - extend;
            this.max = center + extend;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Aabb"/> with the given center point and size.
        /// </summary>
        public Aabb(in Vector2 center, in Vector2 size)
        {
            var extend = new Vector2(MathF.Abs(size.X), MathF.Abs(size.Y)) / 2f;
            this.min = center - extend;
            this.max = center + extend;
        }

        /// <summary>
        /// Finds the closest point on the bounding box.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 ClosestPoint(in Vector2 point)
        {
            return new Vector2(
                MathHelper.Clamp(point.X, this.min.X, this.max.X),
                MathHelper.Clamp(point.Y, this.min.Y, this.max.Y));
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

        /// <summary>
        /// Grows the <see cref="Aabb"/> to encapsulate the point.
        /// </summary>
        public void Encapsulate(in Vector2 point)
        {
            this.min = Vector2.Min(this.min, point);
            this.max = Vector2.Max(this.max, point);
        }

        /// <summary>
        /// Grows the <see cref="Aabb"/> to encapsulate the aabb.
        /// </summary>
        public void Encapsulate(in Aabb aabb)
        {
            this.min = Vector2.Min(this.min, aabb.min);
            this.max = Vector2.Max(this.max, aabb.max);
        }

        /// <summary>
        /// Expands the <see cref="Aabb"/> by increasing its size by amount along each axis.
        /// </summary>
        public void Expand(in Vector2 amount)
        {
            var absAmount = amount.AbsComponents() / 2f;
            this.min -= absAmount;
            this.max += absAmount;
        }

        /// <summary>
        /// Expands the <see cref="Aabb"/> by increasing its size by amount along each axis.
        /// </summary>
        public void Expand(in float amount)
        {
            var absAmount = new Vector2(MathF.Abs(amount) / 2f);
            this.min -= absAmount;
            this.max += absAmount;
        }

        /// <summary>
        /// Constructs a new <see cref="Aabb"/> from a minimum and maximum point.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The minimum point must be component wise less than or equal to the maximum point.
        /// </exception>
        public static Aabb FromMinMax(in Vector2 min, in Vector2 max)
        {
            if (min.X > max.X || min.Y > max.Y)
            {
                throw new ArgumentException(
                    "The minimum point must be component wise less than or equal to the maximum point.");
            }

            return FromMinMaxInternal(in min, in max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Aabb FromMinMaxInternal(in Vector2 min, in Vector2 max)
        {
            return new Aabb { min = min, max = max };
        }
    }
}
