using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Represents a rectangle shape with rotation.
    /// </summary>
    public struct Rect : IArea
    {
        public static readonly Rect Zero = new Rect(Vector2.Zero, Vector2.Zero);

        private Vector2 center;

        private Vector2 halfSize;

        private float sin;

        private float cos;

        private float rotation;

        public readonly Vector2 Center => this.center;

        public readonly Vector2 Extend => this.halfSize;

        public readonly Vector2 Size => this.halfSize * 2f;

        /// <summary>
        /// The counter clockwise rotation in radians.
        /// </summary>
        public readonly float Rotation => this.rotation;

        /// <inheritdoc />
        public readonly Aabb Bounds
        {
            get
            {
                var sx = MathF.Abs(this.cos * this.halfSize.X) + MathF.Abs(this.sin * this.halfSize.Y);
                var sy = MathF.Abs(this.sin * this.halfSize.X) + MathF.Abs(this.cos * this.halfSize.Y);
                return new Aabb(this.center, new Vector2(sx * 2f, sy * 2f));
            }
        }

        /// <inheritdoc />
        public readonly bool Inverted => false;

        public Rect(in Vector2 center, in Vector2 size)
        {
            this.center = center;
            this.halfSize = size.AbsComponents() / 2f;
            this.sin = 0f;
            this.cos = 1f;
            this.rotation = 0f;
        }

        public Rect(in Vector2 center, in Vector2 size, in float rotation)
        {
            this.center = center;
            this.halfSize = size.AbsComponents() / 2f;
            this.rotation = rotation;
            
            this.sin = MathF.Sin(rotation);
            this.cos = MathF.Cos(rotation);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsRect(in Vector2 start, in Vector2 end)
        {
            return this.ContainsPoint(start) &&
                   this.ContainsPoint(end) &&
                   this.ContainsPoint(new Vector2(start.X, end.Y)) &&
                   this.ContainsPoint(new Vector2(end.X, start.Y));
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsPoint(in Vector2 point)
        {
            var sinX = this.sin * this.halfSize.X;
            var sinY = this.sin * this.halfSize.Y;
            var cosX = this.cos * this.halfSize.X;
            var cosY = this.cos * this.halfSize.Y;

            var bl = this.center + new Vector2(-cosX + sinY, -sinX - cosY);
            var tl = this.center + new Vector2(-cosX - sinY, -sinX + cosY);
            var tr = this.center + new Vector2(cosX - sinY, sinX + cosY);
            var br = this.center + new Vector2(cosX + sinY, sinX - cosY);

            var tlbl = tl - bl;
            var trtl = tr - tl;
            var brtr = br - tr;
            var blbr = bl - br;

            var sideLeft = tlbl.X * (point.Y - bl.Y) - tlbl.Y * (point.X - bl.X);
            var sideTop = trtl.X * (point.Y - tl.Y) - trtl.Y * (point.X - tl.X);
            var sideRight = brtr.X * (point.Y - tr.Y) - brtr.Y * (point.X - tr.X);
            var sideBottom = blbr.X * (point.Y - br.Y) - blbr.Y * (point.X - br.X);

            return sideLeft <= 0 && sideTop <= 0 && sideRight <= 0 && sideBottom <= 0;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IntersectsRect(in Vector2 start, in Vector2 end)
        {
            var sinX = this.sin * this.halfSize.X;
            var sinY = this.sin * this.halfSize.Y;
            var cosX = this.cos * this.halfSize.X;
            var cosY = this.cos * this.halfSize.Y;

            var bl = this.center + new Vector2(-cosX + sinY, -sinX - cosY);
            var tl = this.center + new Vector2(-cosX - sinY, -sinX + cosY);
            var tr = this.center + new Vector2(cosX - sinY, sinX + cosY);
            var br = this.center + new Vector2(cosX + sinY, sinX - cosY);

            if (Math2.InRectMm(bl, start, end) || Math2.InRectMm(tl, start, end) ||
                Math2.InRectMm(tr, start, end) || Math2.InRectMm(br, start, end))
            {
                return true;
            }

            // Left
            if (Math2.RayRectMmIntersection(bl, tl - bl, start, end, out var t))
            {
                if (0f <= t && t <= 1f)
                {
                    return true;
                }
            }

            // Top
            if (Math2.RayRectMmIntersection(tl, tr - tl, start, end, out t))
            {
                if (0f <= t && t <= 1f)
                {
                    return true;
                }
            }

            // Right
            if (Math2.RayRectMmIntersection(tr, br - tr, start, end, out t))
            {
                if (0f <= t && t <= 1f)
                {
                    return true;
                }
            }

            // Bottom
            if (Math2.RayRectMmIntersection(br, bl - br, start, end, out t))
            {
                if (0f <= t && t <= 1f)
                {
                    return true;
                }
            }

            return false;
        }

        
    }
}
