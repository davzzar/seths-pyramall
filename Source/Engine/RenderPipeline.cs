using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public sealed class RenderPipeline
    {
        private static readonly RenderPipeline instance = new RenderPipeline();

        private readonly List<Camera> cameras = new List<Camera>();

        private readonly List<Renderer> renderers = new List<Renderer>();

        private readonly List<GuiRenderer> guiRenderers = new List<GuiRenderer>();
        
        private bool isRendering;

        internal event Action OnDrawGizmos;
        
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

        internal void AddGuiRenderer(GuiRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (!renderer.Owner.IsAlive)
            {
                throw new InvalidOperationException("Can't register a gui renderer of a dead game object.");
            }

            if (this.isRendering)
            {
                throw new InvalidOperationException("Can't register a gui renderer in the render loop.");
            }

            Debug.Assert(!this.guiRenderers.Contains(renderer));
            this.guiRenderers.Add(renderer);
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
                throw new InvalidOperationException("Can't remove a renderer in the render loop.");
            }

            Debug.Assert(this.renderers.Contains(renderer));
            this.renderers.RemoveSwapBack(renderer);
        }

        internal void RemoveGuiRenderer(GuiRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (this.isRendering)
            {
                throw new InvalidOperationException("Can't remove a gui renderer in the render loop.");
            }

            Debug.Assert(this.guiRenderers.Contains(renderer));
            this.guiRenderers.RemoveSwapBack(renderer);
        }

        internal void Render()
        {
            this.isRendering = true;

            Graphics.GraphicsDevice.Clear(Graphics.BackgroundColor);

            foreach (var camera in this.cameras)
            {
                Graphics.BeginRender(camera);

                // Draw scene
                if (this.renderers.Count > 0)
                {
                    Graphics.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
                    
                    foreach (var renderer in this.renderers)
                    {
                        renderer.Draw();
                    }

                    Graphics.SpriteBatch.End();
                }

                // Draw Gizmos if needed, don't sort draw calls
                #if DEBUG

                this.InvokeOnDrawGizmos();

                if (Gizmos.CommandBufferCount > 0)
                {
                    Graphics.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    Gizmos.OnRender();
                    Graphics.SpriteBatch.End();
                }

                #endif

                // Draw gui elements
                if (this.guiRenderers.Count > 0)
                {
                    Graphics.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    foreach (var renderer in this.guiRenderers)
                    {
                        renderer.Draw();
                    }

                    Graphics.SpriteBatch.End();
                }

                Graphics.EndRender();
            }

            this.isRendering = false;
        }

        private void InvokeOnDrawGizmos()
        {
            this.OnDrawGizmos?.Invoke();
        }
    }
}
