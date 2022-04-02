using System;
using System.Diagnostics;
using JetBrains.Annotations;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine
{
    public abstract class Collider : Behaviour
    {
        [CanBeNull]
        private RigidBody owningRigidBody;

        [CanBeNull]
        private Shape shape;

        private bool isShapeDirty;

        [CanBeNull]
        private Body body;

        [CanBeNull]
        private Fixture fixture;

        internal Shape Shape
        {
            get
            {
                if (this.shape == null)
                {
                    this.shape = this.GetShape();
                }

                return this.shape;
            }
        }

        internal RigidBody OwningRigidBody
        {
            get => this.owningRigidBody;
            set
            {
                if (this.owningRigidBody == value)
                {
                    return;
                }

                if (this.owningRigidBody != null)
                {
                    Debug.Assert(this.owningRigidBody.Body != null);

                    if (this.IsActiveInHierarchy)
                    {
                        this.owningRigidBody.Body.Remove(this.fixture);
                        this.fixture = null;
                    }
                }

                this.owningRigidBody = value;

                if (this.owningRigidBody != null)
                {
                    if (this.body != null)
                    {
                        PhysicsManager.World.Remove(this.body);
                        this.body = null;
                        this.fixture = null;
                    }

                    Debug.Assert(this.owningRigidBody.Body != null);

                    if (this.IsActiveInHierarchy)
                    {
                        this.fixture = this.owningRigidBody.Body.CreateFixture(this.Shape);
                    }
                }
                else if(this.IsActiveInHierarchy)
                {
                    this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation);
                    Debug.Assert(this.body != null);
                    this.fixture = this.body.CreateFixture(this.Shape);
                }
            }
        }

        internal Collider(){}
        
        /// <inheritdoc />
        protected override void OnEnable()
        {
            if (this.shape == null || this.isShapeDirty)
            {
                this.shape = this.GetShape();
                this.isShapeDirty = false;
                Debug.Assert(this.shape != null);
            }

            var rigidBody = this.owningRigidBody;

            if (rigidBody == null)
            {
                rigidBody = this.Owner.GetComponentInParents<RigidBody>();

                // If the rigidBody exists but its body is null, then its OnAwake call has not yet been invoked, so we delay the registration until then
                // This can happen when the scene loads, the rigidBody.OnAwake was not called yet and a collider was added in a child GO during the OnAwake call of another component.
                if (rigidBody != null && rigidBody.Body != null)
                {
                    this.fixture = rigidBody.Body.CreateFixture(this.shape);
                    rigidBody.AddCollider(this);
                    this.owningRigidBody = rigidBody;
                }
            }

            if (rigidBody == null)
            {
                this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation);
                Debug.Assert(this.body != null);
                this.fixture = this.body.CreateFixture(this.shape);
            }
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            if (this.owningRigidBody != null && this.owningRigidBody.Body != null)
            {
                Debug.Assert(this.fixture != null);
                this.owningRigidBody.Body.Remove(this.fixture);
                this.owningRigidBody.RemoveCollider(this);
                this.owningRigidBody = null;
            }
            else
            {
                PhysicsManager.World.Remove(this.body);
                this.body = null;
            }

            this.fixture = null;
        }

        /// <summary>
        /// Creates a shape representation of this collider, will be used in the internal physics calculations.
        /// </summary>
        [NotNull]
        protected abstract Shape GetShape();

        /// <summary>
        /// Forces the collider to update the shape that is being used internally before the next physics loop.
        /// </summary>
        protected void InvalidateShape()
        {
            this.isShapeDirty = true;
            throw new NotImplementedException();
        }
    }
}
