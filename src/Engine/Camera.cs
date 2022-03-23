using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class Camera : Behaviour
    {
        private float height;

        public float Height
        {
            get => height;
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                {
                    throw new ArgumentException("The camera height cannot be nan or infinity.");
                }

                if (value < 0.1f)
                {
                    value = 0.1f;
                }

                height = value;
            }
        }

        public Matrix3x3 WorldToCameraSpace
        {
            get
            {
                return Matrix3x3.CreateTRS(this.Transform.Position + Vector2.One * 0.5f, this.Transform.Rotation, new Vector2(this.AspectRatio / this.height, -1f / this.height));
            }
        }

        public float AspectRatio => Graphics.GraphicsDevice.Viewport.Height / (float)Graphics.GraphicsDevice.Viewport.Width;
        

        public Vector2 ScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                new Vector2(Graphics.GraphicsDevice.Viewport.Width, Graphics.GraphicsDevice.Viewport.Height);
        }

        public Camera()
        {
            this.height = 10f;
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            GameEngine.Instance.RenderPipeline.AddCamera(this);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            GameEngine.Instance.RenderPipeline.RemoveCamera(this);
        }
    }
}