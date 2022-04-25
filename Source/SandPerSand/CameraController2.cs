using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public sealed class CameraController2 : Behaviour
    {
        private static readonly List<CameraControlPoint> controlPoints = new List<CameraControlPoint>();

        private Camera camera;

        public Vector2 MinCameraSize { get; set; } = Vector2.One * 5;

        public Aabb Bounds { get; set; } = new Aabb(0f, 0f, float.PositiveInfinity, float.PositiveInfinity);

        public static void AddControlPoint([NotNull]CameraControlPoint controlPoint)
        {
            if (controlPoint == null)
            {
                throw new ArgumentNullException(nameof(controlPoint));
            }

            Debug.Assert(!controlPoints.Contains(controlPoint));
            controlPoints.Add(controlPoint);
        }

        public static void RemoveControlPoint([NotNull] CameraControlPoint controlPoint)
        {
            if (controlPoint == null)
            {
                throw new ArgumentNullException(nameof(controlPoint));
            }

            controlPoints.Remove(controlPoint);
        }

        protected override void OnEnable()
        {
            this.camera = this.Owner.GetOrAddComponent<Camera>();
        }

        protected override void Update()
        {
            if (this.camera == null)
            {
                return;
            }

            if (controlPoints.Count == 0)
            {
                return;
            }

            var min = new Vector2(float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity);

            // Create rectangle from control points
            foreach (var controlPoint in controlPoints)
            {
                var pos = controlPoint.Transform.Position;

                if (controlPoint.AffectsHorizontal)
                {
                    min.X = MathF.Min(min.X, pos.X);
                    max.X = MathF.Max(max.X, pos.X);
                }

                if (controlPoint.AffectsVertical)
                {
                    min.Y = MathF.Min(min.Y, pos.Y);
                    max.Y = MathF.Max(max.Y, pos.Y);
                }
            }

            if (max.X < min.X)
            {
                min.X = 0f;
                max.X = 0f;
            }

            if (max.Y < min.Y)
            {
                min.Y = 0f;
                max.Y = 0f;
            }

            var center = (min + max) / 2f;
            var size = Vector2.Max(max - min, this.MinCameraSize);
            size.Y = MathF.Max(size.Y, size.X * this.camera.AspectRatio);

            if (center.Y + size.Y / 2f > this.Bounds.Max.Y)
            {
                center.Y = this.Bounds.Min.Y - size.Y / 2f;
            }

            if (center.Y - size.Y / 2f < this.Bounds.Min.Y)
            {
                center.Y = this.Bounds.Min.Y + size.Y / 2f;
            }

            this.camera.Transform.Position = center;
            this.camera.Height = size.Y;
        }
    }

    public sealed class CameraControlPoint : Component
    {
        public bool AffectsHorizontal { get; set; } = true;

        public bool AffectsVertical { get; set; } = true;

        /// <inheritdoc />
        protected override void OnAwake()
        {
            CameraController2.AddControlPoint(this);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            CameraController2.RemoveControlPoint(this);
        }
    }
}
