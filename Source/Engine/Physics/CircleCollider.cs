using System;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;

namespace Engine
{
    public sealed class CircleCollider : Collider
    {
        private float radius = 0.5f;

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
            // TODO: Support elliptic circle shape to work with offset
            /*if (MathF.Abs(scale.X / scale.Y - 1f) > 1e-3f)
            {
                var polyShape = new PolygonShape(
                    PolygonTools.CreateEllipse(this.radius * scale.X, this.radius * scale.Y, 32),
                    this.Density);
            }*/

            return new CircleShape(this.radius * scale.X, this.Density) { Position = this.Transform.Position };
        }

        /// <inheritdoc />
        protected override void DrawGizmos()
        {
            const int numPoints = 16;

            var sx = this.Transform.Scale.X * this.radius;
            var sy = this.Transform.Scale.Y * this.radius;

            var p0 = this.Transform.TransformPoint(new Vector2(0f, sy));
            var pCur = p0;

            for (var i = 0; i < numPoints; i++)
            {
                var a = MathF.PI * 2f * i / numPoints;
                var pNext = this.Transform.TransformPoint(new Vector2(MathF.Sin(a) * sx, MathF.Cos(a) * sy));

                Gizmos.DrawLine(pCur, pNext, Color.Cyan);
                pCur = pNext;
            }

            Gizmos.DrawLine(pCur, p0, Color.Cyan);
        }
    }
}
