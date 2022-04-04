using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine
{
    public class RigidBody : Behaviour
    {
        [CanBeNull]
        private Body body;

        private readonly List<Collider> colliders = new List<Collider>();

        private bool isKinematic;
        private bool freezeRotation;

        public bool IsKinematic
        {
            get => this.isKinematic;
            set
            {
                if (this.isKinematic == value)
                {
                    return;
                }

                this.isKinematic = value;

                if (this.body != null)
                {
                    this.body.BodyType = this.isKinematic ? BodyType.Kinematic : BodyType.Dynamic;
                }
            }
        }

        public bool FreezeRotation
        {
            get => this.freezeRotation;
            set
            {
                if (this.freezeRotation == value)
                {
                    return;
                }

                this.freezeRotation = value;

                if (this.body != null)
                {
                    this.body.FixedRotation = this.freezeRotation;
                }
            }
        }

        public Vector2 Velocity
        {
            get => this.body != null ? this.body.LinearVelocity : Vector2.Zero;
            set
            {
                if (this.body != null)
                {
                    this.body.LinearVelocity = value;
                }
            }
        }

        [CanBeNull]
        internal Body Body => this.body;
        
        public void ApplyAngularImpulse(float impulse)
        {
            if (this.body != null)
            {
                this.body.ApplyAngularImpulse(impulse);
            }
        }

        public void ApplyForce(in Vector2 force)
        {
            if (this.body != null)
            {
                this.body.ApplyForce(force);
            }
        }

        public void ApplyForce(in Vector2 force, in Vector2 point)
        {
            if (this.body != null)
            {
                this.body.ApplyForce(force, point);
            }
        }

        public void ApplyLinearImpulse(in Vector2 impulse)
        {
            if (this.body != null)
            {
                this.body.ApplyLinearImpulse(impulse);
            }
        }

        public void ApplyLinearImpulse(in Vector2 impulse, in Vector2 point)
        {
            if (this.body != null)
            {
                this.body.ApplyLinearImpulse(impulse, point);
            }
        }

        public void ApplyTorque(float torque)
        {
            if (this.body != null)
            {
                this.body.ApplyTorque(torque);
            }
        }
        
        /// <inheritdoc />
        protected override void OnEnable()
        {
            PhysicsManager.Add(this);

            this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation,
                this.isKinematic ? BodyType.Kinematic : BodyType.Dynamic);

            Debug.Assert(this.body != null);
            this.body.FixedRotation = this.freezeRotation;

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
