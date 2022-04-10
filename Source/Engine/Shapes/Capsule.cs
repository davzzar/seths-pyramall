using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    /// <summary>
    /// Represents a 2D capsule shape with rotation.
    /// </summary>
    public struct Capsule : IArea
    {
        private Vector2 center;

        private float rectHalfHeight;

        private float radius;

        private float sin;

        private float cos;

        private float rotation;

        public readonly Vector2 Center => this.center;

        public readonly float Height => (this.rectHalfHeight + this.radius) * 2f;

        public readonly float Radius => this.radius;

        public readonly float Rotation => this.rotation;

        public readonly Vector2 BottomHalfCircleCenter => new Vector2(this.center.X - this.sin * this.rectHalfHeight, this.center.Y - this.cos * this.rectHalfHeight);

        public readonly Vector2 TopHalfCircleCenter => new Vector2(this.center.X + this.sin * this.rectHalfHeight, this.center.Y + this.cos * this.rectHalfHeight);

        public Capsule(in Vector2 center, in float height, in float radius)
        {
            this.center = center;
            this.radius = MathF.Abs(radius);
            this.rectHalfHeight = MathF.Abs(height) / 2f;
            this.rectHalfHeight = MathF.Max(0f, this.rectHalfHeight - this.radius);
            this.sin = 0f;
            this.cos = 1f;
            this.rotation = 0f;
        }

        public Capsule(in Vector2 center, in float height, in float radius, in float rotation)
        {
            this.center = center;
            this.radius = MathF.Abs(radius);
            this.rectHalfHeight = MathF.Abs(height) / 2f;
            this.rectHalfHeight = MathF.Max(0f, this.rectHalfHeight - this.radius);
            this.sin = MathF.Sin(rotation);
            this.cos = MathF.Cos(rotation);
            this.rotation = rotation;
        }
        
        /// <inheritdoc />
        public readonly Aabb Bounds
        {
            get
            {
                if (this.Height < 1e-5f)
                {
                    return new Aabb(in this.center, new Vector2(this.radius * 2f));
                }

                var sinH = this.sin * this.rectHalfHeight;
                var cosH = this.cos * this.rectHalfHeight;
                var size = new Vector2((this.radius + MathF.Abs(sinH)) * 2f, (this.radius + MathF.Abs(cosH)) * 2f);

                return new Aabb(in this.center, in size);
            }
        }

        /// <inheritdoc />
        public readonly bool Inverted => false;

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
        public readonly bool ContainsPoint(in Vector2 point)
        {
            var sinH = this.sin * this.rectHalfHeight;
            var cosH = this.cos * this.rectHalfHeight;

            var c1 = new Vector2(this.center.X - sinH, this.center.Y + cosH);
            var c2 = new Vector2(this.center.X + sinH, this.center.Y - cosH);
            var r2 = this.radius * this.radius;

            // Test for end circles
            if ((point - c1).LengthSquared() <= r2)
            {
                return true;
            }

            if (this.Height < 1e-5f)
            {
                return false;
            }

            if ((point - c2).LengthSquared() <= r2)
            {
                return true;
            }

            // Test for inner rect
            var sinR = this.sin * this.radius;
            var cosR = this.cos * this.radius;

            var bl = new Vector2(c2.X - cosR, c2.Y - sinR);
            var tl = new Vector2(c1.X - cosR, c1.Y - sinR);
            var tr = new Vector2(c1.X + cosR, c1.Y + sinR);
            var br = new Vector2(c2.X + cosR, c2.Y + sinR);

            var tlbl = tl - bl;
            var trtl = tr - tl;
            var brtr = br - tr;
            var blbr = bl - br;

            var sideLeft   = tlbl.X * (point.Y - bl.Y) - tlbl.Y * (point.X - bl.X);
            var sideTop    = trtl.X * (point.Y - tl.Y) - trtl.Y * (point.X - tl.X);
            var sideRight  = brtr.X * (point.Y - tr.Y) - brtr.Y * (point.X - tr.X);
            var sideBottom = blbr.X * (point.Y - br.Y) - blbr.Y * (point.X - br.X);

            return sideLeft <= 0 && sideTop <= 0 && sideRight <= 0 && sideBottom <= 0;
        }

        /// <inheritdoc />
        public readonly bool IntersectsRect(in Vector2 start, in Vector2 end)
        {
            var sinH = this.sin * this.rectHalfHeight;
            var cosH = this.cos * this.rectHalfHeight;

            var c1 = new Vector2(this.center.X - sinH, this.center.Y + cosH);
            var c2 = new Vector2(this.center.X + sinH, this.center.Y - cosH);
            var r2 = this.radius * this.radius;

            // Test for end circles
            var x = MathHelper.Clamp(c1.X, start.X, end.X) - c1.X;
            var y = MathHelper.Clamp(c1.Y, start.Y, end.Y) - c1.Y;
            var sqrDist = x + y;

            if (sqrDist * sqrDist <= r2)
            {
                return true;
            }

            if (this.Height < 1e-5f)
            {
                return false;
            }

            x = MathHelper.Clamp(c2.X, start.X, end.X) - c2.X;
            y = MathHelper.Clamp(c2.Y, start.Y, end.Y) - c2.Y;
            sqrDist = x + y;

            if (sqrDist * sqrDist <= r2)
            {
                return true;
            }

            var sinR = this.sin * this.radius;
            var cosR = this.cos * this.radius;

            var bl = new Vector2(c2.X - cosR, c2.Y - sinR);
            var tl = new Vector2(c1.X - cosR, c1.Y - sinR);
            var tr = new Vector2(c1.X + cosR, c1.Y + sinR);
            var br = new Vector2(c2.X + cosR, c2.Y + sinR);
            var side = tl - bl;

            if (Math2.InRectMm(bl, start, end) || Math2.InRectMm(tl, start, end) ||
                Math2.InRectMm(tr, start, end) || Math2.InRectMm(br, start, end))
            {
                return true;
            }

            // Left
            if (Math2.RayRectMmIntersection(bl, side, start, end, out var t))
            {
                if (0f <= t && t <= 1f)
                {
                    return true;
                }
            }

            // Right
            if (Math2.RayRectMmIntersection(br, side, start, end, out t))
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
