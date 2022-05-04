using System;
using Engine;
using Microsoft.Xna.Framework;
using Ray = Engine.Ray;

namespace SandPerSand
{
    /// <summary>
    /// Gives the ability to perform ground collision checks for a given game object.
    /// </summary>
    class GroundCheckComponent : Component
    {
        private readonly LayerMask groundLayer = LayerMask.FromLayers(0);
        public int Resolution = 3;
        public float SkinWidth = 0.35f; //Offset of the ray casts 
        public float MaxIncline = MathF.Sin(MathF.PI/4.0f);
        public float RayExtrusion = 0.1f;

        /// <summary>
        /// A check which casts rays along the bottom of the parent according to a spatial resolution.
        /// </summary>
        /// <returns>True if any of the cast rays intersect with the ground layer (layer 0).</returns>
        public bool IsGrounded()
        {
            var size = Transform.Scale;
            var pos0 = Transform.Position;
            pos0.X = (pos0.X - size.X / 2f) + SkinWidth;
            var maxRayLength = size.Y / 2f + RayExtrusion;

            var didCollide = false;
            for (var i = 0; i < Resolution; i++)
            {
                var origin = pos0 + i *  Vector2.UnitX * (size.X - 2 * SkinWidth) / (Resolution - 1);
                var ray = new Ray(origin, -Vector2.UnitY);
                Gizmos.DrawLine(origin, origin - Vector2.UnitY * maxRayLength, Color.Yellow); 
                
                // if there was no collision, skip
                if (!Physics.RayCast(ray, out var hit, maxRayLength, groundLayer)) continue;
                Gizmos.DrawLine(origin, hit.Point, Color.Red);
                // check the angle of hit to see if we're colliding with a floor.   
                didCollide |= hit.Normal.Y >= MaxIncline;
            }
            return didCollide;
        }
    }
}
