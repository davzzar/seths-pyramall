using System;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine
{
    public sealed class CircleCollider : Collider
    {
        private float radius = 1f;

        public float Radius
        {
            get => this.radius;
            set => this.radius = MathF.Abs(value);
        }

        /// <inheritdoc />
        protected override Shape GetShape()
        {
            return new CircleShape(this.radius, 1f);
        }
    }
}
