using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;

namespace Engine
{
    public sealed class PolygonCollider : Collider
    {
        private List<Vector2> outline;

        [NotNull]
        public Vector2[] Outline
        {
            // Only return a copy to prevent aliasing and undetected edits outside the class scope.
            get => this.outline.ToArray();
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length < 2)
                {
                    throw new ArgumentException("The outline needs to consist of at least two vertices.",
                        nameof(value));
                }

                if (value.Length > this.outline.Capacity)
                {
                    this.outline.Capacity = value.Length;
                }

                this.outline.Clear();
                this.outline.AddRange(value);
                this.InvalidateShape();
            }
        }

        public Vertices Polygon
        {
            get
            {
                var vertices = new Vertices(this.outline);
                vertices.Scale(this.Transform.Scale);
                vertices.Rotate(this.Transform.Rotation);
                vertices.Translate(this.Transform.Position);
                return vertices;
            }
        }

        public PolygonCollider()
        {
            this.outline = new List<Vector2>();
            this.MakeRectangle(Vector2.One, Vector2.Zero);
        }
        
        public void MakeRectangle(in Vector2 size)
        {
            this.MakeRectangle(in size, Vector2.Zero);
        }

        public void MakeRectangle(in Vector2 size, in Vector2 center)
        {
            if (this.outline.Capacity < 4)
            {
                this.outline.Capacity = 4;
            }

            var halfSize = size / 2f;

            this.outline.Clear();
            this.outline.Add(center - halfSize);
            this.outline.Add(center + new Vector2(halfSize.X, -halfSize.Y));
            this.outline.Add(center + halfSize);
            this.outline.Add(center + new Vector2(-halfSize.X, halfSize.Y));
            
            this.InvalidateShape();
        }

        public void MakeTriangle(in Vector2 p0, in Vector2 p1, in Vector2 p2)
        {
            if (this.outline.Capacity < 3)
            {
                this.outline.Capacity = 3;
            }

            this.outline.Clear();
            this.outline.Add(p0);
            this.outline.Add(p1);
            this.outline.Add(p2);

            this.InvalidateShape();
        }

        /// <inheritdoc />
        protected override Shape GetShape()
        {
            if (this.outline.Count == 2)
            {
                return new EdgeShape(this.outline[0], this.outline[1]);
            }

            var vertices = new Vertices(this.outline);
            vertices.Scale(this.Transform.Scale);

            return new PolygonShape(vertices, this.Density);
        }

        /// <inheritdoc />
        protected override void DrawGizmos()
        {
            var p0 = this.Transform.TransformPoint(this.outline[0]);
            var pCur = p0;

            for (var i = 0; i < this.outline.Count - 1; i++)
            {
                var pNext = this.Transform.TransformPoint(this.outline[i + 1]);
                Gizmos.DrawLine(pCur, pNext, Color.White);
                pCur = pNext;
            }

            Gizmos.DrawLine(pCur, p0, Color.White);
        }
    }
}
