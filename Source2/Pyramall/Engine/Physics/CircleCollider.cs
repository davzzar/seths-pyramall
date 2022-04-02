using System;
using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine.Physics
{
    public sealed class CircleCollider : Collider
    {
        private Body body;

        /// <inheritdoc />
        protected override void OnEnable()
        {

        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
        }
    }
}
