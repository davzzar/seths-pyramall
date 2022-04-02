using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine
{
    public class RigidBody : Behaviour
    {
        [CanBeNull]
        private Body body;

        private readonly List<Collider> colliders = new List<Collider>();

        [CanBeNull]
        internal Body Body => this.body;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            PhysicsManager.Add(this);

            this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation,
                BodyType.Dynamic);

            this.Owner.GetComponentsInChildren(this.colliders);

            foreach (var collider in this.colliders)
            {
                collider.OwningRigidBody = this;
            }
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            PhysicsManager.Remove(this);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            
        }

        internal void AddCollider(Collider collider)
        {
            if (this.body == null)
            {
                return;
            }

            throw new NotImplementedException();
        }

        internal void RemoveCollider(Collider collider)
        {
            if (this.body == null)
            {
                return;
            }

            throw new NotImplementedException();
        }

        internal void OnBeforePhysicsStep()
        {
            Debug.Assert(this.body != null);
            this.body.Position = this.Transform.Position;
            this.body.Rotation = this.Transform.Rotation;
        }

        internal void OnAfterPhysicsStep()
        {
            Debug.Assert(this.body != null);
            this.Transform.Position = this.body.Position;
            this.Transform.LocalRotation = this.body.Rotation;
            // TODO: Should be world rotation, fix after testing!
        }
    }
}
