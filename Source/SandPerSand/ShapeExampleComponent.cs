using System;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public class ShapeExampleComponent : Behaviour
    {
        private Vector2[] outline;

        public Vector2[] Outline
        {
            get => this.outline;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length < 2)
                {
                    throw new ArgumentException("The outline needs to consist of at least two points.");
                }

                this.outline = value;

                var collider = this.Owner.GetComponent<PolygonCollider>();
                if (collider != null)
                {
                    collider.Outline = this.outline;
                }
            }
        }

        public Color Color { get; set; } = Color.White;

        /// <inheritdoc />
        protected override void OnAwake()
        {
            if (this.Owner.GetComponent<RigidBody>() == null)
            {
                this.Owner.AddComponent<RigidBody>();
            }

            var collider = this.Owner.AddComponent<PolygonCollider>();

            if (this.Outline != null && this.Outline.Length >= 2)
            {
                collider.Outline = this.outline;
            }
        }

        protected override void Update()
        {
            if (this.Outline == null || this.Outline.Length < 2)
            {
                return;
            }

            var p0 = this.Transform.TransformPoint(this.Outline[0]);
            var pCur = p0;

            for (var i = 0; i < this.Outline.Length - 1; i++)
            {
                var pNext = this.Transform.TransformPoint(this.Outline[i + 1]);
                Gizmos.DrawLine(pCur, pNext, this.Color);
                pCur = pNext;
            }

            Gizmos.DrawLine(pCur, p0, this.Color);
        }
    }
}