using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

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
                else if (this.IsActiveInHierarchy)
                {
                    this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation);
                    Debug.Assert(this.body != null);
                    this.fixture = this.body.CreateFixture(this.Shape);
                }
            }
        }

        internal Collider()
        { }

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
                    this.fixture = rigidBody.Body.CreateFixture(this.Shape);
                    this.fixture.CollisionCategories = (Category)(1 << this.Owner.Layer);
                    this.fixture.Friction = this.friction;

                    rigidBody.AddCollider(this);
                    this.owningRigidBody = rigidBody;
                }
            }

            if (rigidBody == null)
            {
                this.body = PhysicsManager.World.CreateBody(this.Transform.Position, this.Transform.Rotation);
                Debug.Assert(this.body != null);
                this.fixture = this.body.CreateFixture(this.Shape);
                this.fixture.CollisionCategories = (Category)(1 << this.Owner.Layer);
                this.fixture.Friction = this.friction;
            }

            this.Owner.LayerChanged += this.OwnerOnLayerChanged;
        }
        
        /// <inheritdoc />
        protected override void OnDisable()
        {
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

            this.fixture = null;
        }

        #if DEBUG

        protected override void Update()
        {
            if (ShowGizmos)
            {
                this.DrawGizmos();

                var p0 = this.Transform.TransformPoint(Vector2.Zero);
                var pRight = this.Transform.TransformPoint(Vector2.UnitX * 0.3f);
                var pUp = this.Transform.TransformPoint(Vector2.UnitY * 0.3f);
                Gizmos.DrawLine(p0, pRight, Color.Blue);
                Gizmos.DrawLine(p0, pUp, Color.Red);
            }
        }

        #endif

        /// <summary>
        /// Creates a shape representation of this collider, will be used in the internal physics calculations.
        /// </summary>
        [NotNull]
        protected abstract Shape GetShape();

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
            this.fixture = bodyInUse.CreateFixture(this.Shape);
            this.fixture.CollisionCategories = (Category)(1 << this.Owner.Layer);
            this.fixture.Friction = this.friction;
        }

        private void OwnerOnLayerChanged(object sender, (int oldLayer, int newLayer) e)
        {
            if (this.fixture != null)
            {
                this.fixture.CollisionCategories = (Category)(1 << this.Owner.Layer);
            }
        }
    }
}
