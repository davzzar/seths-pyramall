using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Engine
{
    public sealed class Transform : Component
    {
        private Transform parent;

        internal Scene containingScene;

        private readonly List<Transform> children = new List<Transform>();

        private Matrix3x3 localToWorld;

        private Matrix3x3 localToParent;

        private Vector2 localPosition;

        private float localRotation;

        private Vector2 lossyScale;

        private DirtyFlags dirty;

        public Transform()
        {
            this.localPosition = Vector2.Zero;
            this.localRotation = 0f;
            this.lossyScale = Vector2.One;
            this.dirty = DirtyFlags.Both;
        }

        public Transform Parent
        {
            get => this.parent;
            set
            {
                if (this.parent == value)
                {
                    return;
                }

                var current = value;
                while (current != null)
                {
                    if (current == this)
                    {
                        throw new InvalidOperationException("Can't set transform or child of transform as parent of transform.");
                    }

                    current = current.parent;
                }
                
                this.parent?.children.Remove(this);
                this.parent = value;
                this.parent?.children.Add(this);

                this.MarkLocalToWorldDirty();
            }
        }

        public ref Matrix3x3 LocalToWorld
        {
            get
            {
                this.UpdateLocalToWorldOnDemand();
                return ref this.localToWorld;
            }
        }

        public ref Matrix3x3 WorldToLocal
        {
            get
            {
                // TODO: FIX SHIT
                this.UpdateLocalToParentOnDemand();
                return ref this.localToParent;
            }
        }

        public Vector2 Position
        {
            get
            {
                this.UpdateLocalToWorldOnDemand();
                this.localToWorld.DecomposeTRS(out var pos, out _, out _);
                return pos;
            }
            set
            {
                if (this.parent == null)
                {
                    this.LocalPosition = value;
                }
                else
                {
                    var parentWorldToLocal = this.parent.LocalToWorld.Invert();
                    var localPos = parentWorldToLocal.TransformPoint(value);
                    this.LocalPosition = localPos;
                }
            }
        }

        public Vector2 LocalPosition
        {
            get => this.localPosition;
            set
            {
                this.localPosition = value;
                this.MarkLocalToParentDirty();
            }
        }

        public float Rotation
        {
            get
            {
                this.UpdateLocalToWorldOnDemand();
                this.localToWorld.DecomposeTRS(out _, out var rotation, out _);
                return rotation;
            }
        }

        public float LocalRotation
        {
            get => this.localRotation;
            set
            {
                this.localRotation= value;
                this.MarkLocalToParentDirty();
            }
        }

        public Vector2 Scale
        {
            get
            {
                this.UpdateLocalToWorldOnDemand();
                this.localToWorld.DecomposeTRS(out _, out _, out var scale);
                return scale;
            }
        }

        public Vector2 LossyScale
        {
            get => this.lossyScale;
            set
            {
                this.lossyScale = value;
                this.MarkLocalToParentDirty();
            }
        }

        public IReadOnlyList<Transform> Children => this.children.AsReadOnly();

        public int ChildCount => this.children.Count;

        public Transform GetChild(int index)
        {
            return this.children[index];
        }

        internal void SetContainingScene(Scene scene)
        {
            this.containingScene = scene;
        }

        private void UpdateLocalToParentOnDemand()
        {
            if((this.dirty & DirtyFlags.LocalToParent) == 0)
            {
                return;
            }

            this.localToParent = Matrix3x3.CreateTRS(in this.localPosition, this.localRotation, in this.lossyScale);
            this.dirty &= ~DirtyFlags.LocalToParent;
        }

        private void UpdateLocalToWorldOnDemand()
        {
            if((this.dirty & DirtyFlags.LocalToWorld) == 0)
            {
                return;
            }

            this.UpdateLocalToParentOnDemand();

            if (this.parent != null)
            {
                this.localToWorld = this.parent.LocalToWorld * this.localToParent;
            }
            else
            {
                this.localToWorld = this.localToParent;
            }

            this.dirty &= ~DirtyFlags.LocalToWorld;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MarkLocalToParentDirty()
        {
            this.dirty = DirtyFlags.LocalToParent;
            this.MarkLocalToWorldDirty();
        }

        private void MarkLocalToWorldDirty()
        {
            if((this.dirty & DirtyFlags.LocalToWorld) != 0)
            {
                // Can stop recursive marking, as children must be dirty already.
                return;
            }

            this.dirty |= DirtyFlags.LocalToWorld;

            foreach (var child in this.children)
            {
                child.MarkLocalToWorldDirty();
            }
        }

        [Flags]
        private enum DirtyFlags : int
        {
            LocalToParent = 1 << 0,
            LocalToWorld = 1 << 1,
            Both = LocalToParent | LocalToWorld
        }
    }
}
