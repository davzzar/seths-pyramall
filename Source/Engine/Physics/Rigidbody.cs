using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        
        private float angularDamping;
        private bool freezeRotation;
        private bool ignoreGravity;
        private bool isKinematic;
        private float linearDamping;
        private float mass;
        private bool ignoreCCD;

        public float AngularDamping
        {
            get => this.angularDamping;
            set
            {
                this.angularDamping = value;

                if (this.body != null)
                {
                    this.body.AngularDamping = value;
                }
            }
        }

        public float AngularVelocity
        {
            get => this.body != null ? this.body.AngularVelocity : 0f;
            set
            {
                if (this.body != null)
                {
                    this.body.AngularVelocity = value;
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

        public bool IgnoreGravity
        {
            get => this.ignoreGravity;
            set
            {
                this.ignoreGravity = value;

                if (this.body != null)
                {
                    this.body.IgnoreGravity = value;
                }
            }
        }

        public bool IgnoreCCD
        {
            get => this.ignoreCCD;
            set
            {
                this.ignoreCCD = value;

                if (this.body != null)
                {
                    this.body.IgnoreCCD = value;
                }
            }
        }

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

        public float LinearDamping
        {
            get => this.linearDamping;
            set
            {
                this.linearDamping = value;

                if (this.body != null)
                {
                    this.body.LinearDamping = value;
                }
            }
        }

        public Vector2 LinearVelocity
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

        public float Mass
        {
            get => this.mass;
            set
            {
                this.mass = value;

                if (this.body != null)
                {
                    this.body.Mass = value;
                }
            }
        }

        [Obsolete("Use LinearVelocity instead.")]
        public Vector2 Velocity
        {
            get => this.LinearVelocity;
            set => this.LinearVelocity = value;
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
            this.body.AngularDamping = this.angularDamping;
            this.body.FixedRotation = this.freezeRotation;
            this.body.IgnoreCCD = this.ignoreCCD;
            this.body.IgnoreGravity  = this.ignoreGravity;
            this.body.LinearDamping = this.linearDamping;
            this.body.Mass = this.mass;

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

            Debug.Assert(!this.colliders.Contains(collider));
            this.colliders.Add(collider);
        }

        internal void RemoveCollider(Collider collider)
        {
            if (this.body == null)
            {
                return;
            }

            Debug.Assert(this.colliders.Contains(collider));
            this.colliders.Remove(collider);
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
