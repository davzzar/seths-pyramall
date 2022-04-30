using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

namespace Engine
{
    public abstract class Collider : Behaviour
    {
        public static bool ShowGizmos = true;

        [CanBeNull]
        private RigidBody owningRigidBody;

        [CanBeNull]
        private Shape shape;

        private bool isShapeDirty;

        [CanBeNull]
        private Body body;

        [CanBeNull]
        private Fixture fixture;

        private float density = 1f;

        private float friction = 0.1f;
        private bool isTrigger;

        /// <summary>
        /// Gets or sets the density of the collider.<br/>
        /// The collider mass is equals to <c>Density * Area</c>.
        /// </summary>
        public float Density
        {
            get => this.density;
            set
            {
                if (MathF.Abs(this.density - value) < 1e-5f)
                {
                    return;
                }

                this.density = value;

                if (this.shape != null)
                {
                    this.shape.Density = value;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the friction of this collider.<br/>
        /// A higher friction will cause more energy loss along the collision tangents.
        /// </summary>
        public float Friction
        {
            get => this.friction;
            set
            {
                if (MathF.Abs(this.friction - value) < 1e-5f)
                {
                    return;
                }

                this.friction = value;

                if (this.fixture != null)
                {
                    this.fixture.Friction = this.friction;
                }
            }
        }

        public bool IsTrigger
        {
            get => this.isTrigger;
            set => this.isTrigger = value;
        }

        /// <summary>
        /// Gets the rigid-body to which this collider belongs.<br/>
        /// Returns <c>null</c> if this collider doesn't belong to any rigidbody.
        /// </summary>
        [CanBeNull]
        public RigidBody OwningRigidBody
        {
            get => this.owningRigidBody;
            internal set
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
                else if (this.IsActiveInHierarchy)
                {
                    this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation);
                    Debug.Assert(this.body != null);
                    this.fixture = this.body.CreateFixture(this.Shape);
                }
            }
        }

        internal Shape Shape
        {
            get
            {
                if (this.shape == null || this.isShapeDirty)
                {
                    this.shape = this.GetShape();
                    this.isShapeDirty = false;
                    Debug.Assert(this.shape != null);
                }

                return this.shape;
            }
        }

        internal Collider()
        { }

        public event EventHandler<Collider> CollisionEnter;

        public event EventHandler<Collider> CollisionExit;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            var rigidBody = this.owningRigidBody;

            if (rigidBody == null)
            {
                rigidBody = this.Owner.GetComponentInParents<RigidBody>();

                // If the rigidBody exists but its body is null, then its OnAwake call has not yet been invoked, so we delay the registration until then
                // This can happen when the scene loads, the rigidBody.OnAwake was not called yet and a collider was added in a child GO during the OnAwake call of another component.
                if (rigidBody != null && rigidBody.Body != null)
                {
                    this.fixture = this.CreateFixture(rigidBody.Body);

                    rigidBody.AddCollider(this);
                    this.owningRigidBody = rigidBody;
                }
            }

            if (rigidBody == null)
            {
                this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation);
                Debug.Assert(this.body != null);
                this.fixture = this.CreateFixture(this.body);
            }

            this.Owner.LayerChanged += this.OwnerOnLayerChanged;

#if DEBUG
            this.OnDrawGizmos += this.DrawGizmosInternal;
#endif
        }
        
        /// <inheritdoc />
        protected override void OnDisable()
        {
#if DEBUG
            this.OnDrawGizmos -= this.DrawGizmosInternal;
#endif

            this.Owner.LayerChanged -= this.OwnerOnLayerChanged;

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

            this.DestroyCurrentFixture();
        }

        /// <summary>
        /// Creates a shape representation of this collider, will be used in the internal physics calculations.
        /// </summary>
        [NotNull]
        protected abstract Shape GetShape();

        /// <summary>
        /// Called once per frame when the gizmos is being drawn.
        /// </summary>
        protected abstract void DrawGizmos();

        /// <summary>
        /// Forces the collider to update the shape that is being used internally before the next physics loop.
        /// </summary>
        protected void InvalidateShape()
        {
            // Next call to this.Shape will create a fresh shape
            this.isShapeDirty = true;

            if (this.shape == null || !this.IsActiveInHierarchy)
            {
                return;
            }

            var bodyInUse = this.owningRigidBody != null ? this.owningRigidBody.Body : this.body;

            Debug.Assert(bodyInUse != null);
            Debug.Assert(this.fixture != null);

            bodyInUse.Remove(this.fixture);
            this.DestroyCurrentFixture();
            this.fixture = this.CreateFixture(bodyInUse);
        }

#if DEBUG

        private void DrawGizmosInternal()
        {
            if (ShowGizmos)
            {
                this.DrawGizmos();
            }
        }

#endif

        private void OwnerOnLayerChanged(object sender, (int oldLayer, int newLayer) e)
        {
            if (this.fixture != null)
            {
                this.fixture.CollisionCategories = (Category)(1 << this.Owner.Layer);
            }
        }

        private Fixture CreateFixture([NotNull]Body body)
        {
            Debug.Assert(this.fixture == null);
            Debug.Assert(body != null);

            var result = body.CreateFixture(this.Shape);
            result.Tag = this;
            result.CollisionCategories = (Category)(1 << this.Owner.Layer);
            result.Friction = this.friction;
            result.OnCollision += this.OnCollision;
            result.OnSeparation += this.OnSeparation;

            return result;
        }

        private void DestroyCurrentFixture()
        {
            Debug.Assert(this.fixture != null);

            this.fixture.OnCollision += this.OnCollision;
            this.fixture.OnSeparation += this.OnSeparation;
            this.fixture = null;
        }

        private void OnSeparation(Fixture sender, Fixture other, Contact contact)
        {
            this.OnCollisionExit((Collider)other.Tag);
        }

        private bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            this.OnCollisionEnter((Collider)other.Tag);

            return !this.IsTrigger;
        }

        private void OnCollisionEnter(Collider e)
        {
            this.CollisionEnter?.Invoke(this, e);
        }

        private void OnCollisionExit(Collider e)
        {
            this.CollisionExit?.Invoke(this, e);
        }
    }
}
