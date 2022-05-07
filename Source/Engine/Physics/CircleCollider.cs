using System;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;

namespace Engine
{
    public sealed class CircleCollider : Collider
    {
        private float radius = 1f;

        public float Radius
        {
            get => this.radius;
            set
            {
                if (MathF.Abs(this.radius - value) < 1e-5f)
                {
                    return;
                }

                this.radius = MathF.Abs(value);
                this.InvalidateShape();
            }
        }

        /// <inheritdoc />
        protected override Shape GetShape()
        {
            var scale = this.Transform.Scale;

            // Special case where non-uniform scaling creates an ellipse instead of a circle. Create a polygon shape that approximates it.
            if (MathF.Abs(scale.X / scale.Y - 1f) > 1e-3f)
            {
                return new PolygonShape(
                    PolygonTools.CreateEllipse(this.radius * scale.X / 2f, this.radius * scale.Y / 2f, 32),
                    this.Density);
            }

            return new CircleShape(this.radius * scale.X / 2f, this.Density);
        }

        /// <inheritdoc />
        protected override void DrawGizmos()
        {
            const int numPoints = 16;

            var sX = this.radius * this.Transform.LossyScale.X * 0.5f;
            var sY = this.radius * this.Transform.LossyScale.Y * 0.5f;
            var p0 = this.Transform.TransformPoint(new Vector2(0f, sY));
            var pCur = p0;

            for (var i = 0; i < numPoints; i++)
            {
                var a = MathF.PI * 2f * i / numPoints;
                var pNext = this.Transform.TransformPoint(new Vector2(MathF.Sin(a) * sX, MathF.Cos(a) * sY));

                Gizmos.DrawLine(pCur, pNext, Color.White);
                pCur = pNext;
            }

            Gizmos.DrawLine(pCur, p0, Color.White);
        }
    }
}
