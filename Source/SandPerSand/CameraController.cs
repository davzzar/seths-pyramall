using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    public class CameraController : Behaviour
    {
        protected override void OnEnable()
        {
            // Get all renderers in the open scenes
            var renderers = new List<Renderer>();
            GameObject.FindComponents(renderers);

            // Find the bounds of the scene
            var min = Vector2.Zero;
            var max = Vector2.Zero;
            foreach (var renderer in renderers)
            {
                var p = renderer.Transform.Position;
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }

            var camera = GameObject.FindComponent<Camera>();

            // Place the camera accordingly
            camera.Transform.Position = (min + max) / 2f;
            camera.Height = max.Y - min.Y + 2f;

            this.Destroy();
        }
    }
}