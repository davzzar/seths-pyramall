using System;
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
    }
}
