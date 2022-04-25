using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    /// <summary>
    /// The camera component defines how the world is rendered to screen.<br/>
    /// It can be thought of as a rectangle that is placed in the world and the content in the rectangle is rendered to screen.
    /// </summary>
    public sealed class Camera : Behaviour
    {
        private float height;

        /// <summary>
        /// Gets or sets the height of the view rectangle in world units (usually meters).
        /// </summary>
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

        /// <summary>
        /// Gets the matrix representing the transformation from world space to camera space.
        /// </summary>
        public Matrix3x3 WorldToCameraSpace
        {
            get
            {
                var scale = new Vector2(this.AspectRatio / this.height, -1f / this.height);
                return Matrix3x3.CreateScale(scale) *
                       Matrix3x3.CreateTranslation(Vector2.One / scale * 0.5f) *
                       Matrix3x3.CreateRotation(-this.Transform.Rotation) *
                       Matrix3x3.CreateTranslation(-this.Transform.Position);
                //return Matrix3x3.CreateTRS(-this.Transform.Position + Vector2.One * 0.5f, -this.Transform.Rotation, new Vector2(this.AspectRatio / this.height, -1f / this.height));
            }
        }

        /// <summary>
        /// Gets the aspect ratio of the <see cref="Camera"/>, depends on the screen width and height.
        /// </summary>
        public float AspectRatio => Graphics.GraphicsDevice.Viewport.Height / (float)Graphics.GraphicsDevice.Viewport.Width;
        
        /// <summary>
        /// Gets the size of the screen area to which this camera is being rendered.
        /// </summary>
        public Vector2 ScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get =>
                new Vector2(Graphics.GraphicsDevice.Viewport.Width, Graphics.GraphicsDevice.Viewport.Height);
        }

        /// <summary>
        /// WARNING: Don't use the constructor, use <see cref="GameObject.AddComponent{Camera}"/> instead.<br/>
        /// Creates a new instance of the camera type.
        /// </summary>
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