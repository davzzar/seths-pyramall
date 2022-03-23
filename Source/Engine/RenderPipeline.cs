using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    public sealed class RenderPipeline
    {
        private static readonly RenderPipeline instance = new RenderPipeline();

        private readonly List<Camera> cameras = new List<Camera>();

        private readonly List<Renderer> renderers = new List<Renderer>();
        
        private bool isRendering;
        
        internal void Init()
        {
            instance.cameras.Clear();
            instance.renderers.Clear();
            instance.isRendering = false;
        }

        internal void AddCamera(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (!camera.Owner.IsAlive)
            {
                throw new InvalidOperationException("Can't register a camera of a dead game object.");
            }

            if (this.isRendering)
            {
                throw new InvalidOperationException("Can't register a camera in the render loop.");
            }
            
            Debug.Assert(!this.cameras.Contains(camera));
            this.cameras.Add(camera);
        }

        internal void AddRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (!renderer.Owner.IsAlive)
            {
                throw new InvalidOperationException("Can't register a renderer of a dead game object.");
            }

            if (this.isRendering)
            {
                throw new InvalidOperationException("Can't register a renderer in the render loop.");
            }

            Debug.Assert(!this.renderers.Contains(renderer));
            renderers.Add(renderer);
        }

        internal void RemoveCamera(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (this.isRendering)
            {
                throw new InvalidOperationException("Can't remove a camera in the render loop.");
            }
            
            Debug.Assert(this.cameras.Contains(camera));
            this.cameras.Remove(camera);
        }
        
        internal void RemoveRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (this.isRendering)
            {
                throw new InvalidOperationException("Can't register a camera in the render loop.");
            }

            Debug.Assert(this.renderers.Contains(renderer));
            renderers.RemoveSwapBack(renderer);
        }

        internal void Render()
        {
            this.isRendering = true;

            foreach (var camera in this.cameras)
            {
                Graphics.BeginRender(camera);

                foreach (var renderer in this.renderers)
                {
                    renderer.Draw();
                }

                Graphics.EndRender();
            }

            this.isRendering = false;
        }
    }
}
