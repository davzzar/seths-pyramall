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
        public GroundCheckComponent() 
        {
        }


        // TODO find a better way of setting this without assuming ground
        // layer intex to be zero.
        private LayerMask groundLayer = LayerMask.FromLayers(0);
        private float colliderOffset = 0.01f;
        private const int resolution = 8;
        private const float minShallowness = 0.9f;

        /// <summary>
        /// A check which casts rays along the bottom of the parent according to a spatial resolution.
        /// </summary>
        /// <returns>True if any of the cast rays intersect with the ground layer (layer 0).</returns>
        public bool IsGrounded()
        {
            var size = this.Transform.Scale;
            var pos0 = this.Transform.Position;
            pos0.X -= size.X / 2f;
            var maxRayLength = size.Y / 2f + 0.1f;

            bool didCollide = false;
            for (var i = 0; i < resolution; i++)
            {
                var origin = pos0 + Vector2.UnitX * (i / (float)(resolution - 1));
                var ray = new Ray(origin, -Vector2.UnitY);
                Gizmos.DrawLine(origin, origin - Vector2.UnitY * maxRayLength, Color.Yellow);
                // if there was a collision
                    if (Physics.RayCast(ray, out var hit, maxRayLength, groundLayer))
                {
                    Gizmos.DrawLine(origin, hit.Point, Color.Red);
                    // check the angle of hit to see if we're colliding with a floor.   
                    didCollide |= hit.Normal.Y >= minShallowness;
                }
            }
            return didCollide;
        }
    }
}
