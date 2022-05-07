using System;
using System.Diagnostics;
using Engine;
using Microsoft.Xna.Framework;
using Ray = Engine.Ray;

namespace SandPerSand
{
    /// <summary>
    /// Gives the ability to perform ground collision checks for a given game object.
    /// Also keeps track of slopes the game objects is on.
    /// </summary>
    class GroundCheckComponent : Behaviour
    {
        private readonly LayerMask groundLayer = LayerMask.FromLayers(0);

        public bool IsGrounded;
        public bool IsOnSlope;

        public int RayResolution = 3;
        public const float SkinWidth = 0.35f; //Offset of the ray casts 
        public float MaxIncline = MathF.Sin(MathF.PI/4.0f);
        public const float RayExtrusion = 0.05f;
        private const float SlopeRayExtension= 0.3f;
        private float MaxDownRayLength => Transform.Scale.Y / 2f + RayExtrusion;
        private float MaxSideRayLength => Transform.Scale.X / 2f + RayExtrusion;

        public Vector2 SlopeDirection;
        public float SlopeAngleDown;
        private float slopeAngleDownPrev;

        /// <summary>
        /// A check which casts rays along the bottom of the parent according to a spatial resolution.
        /// </summary>
        /// <returns>True if any of the cast rays intersect with the ground layer (layer 0).</returns>
        protected override void Update()
        {
            CheckGrounded();
        }

        private void CheckGrounded()
        {
            var size = Transform.Scale;
            var pos0 = Transform.Position;
            pos0.X = (pos0.X - size.X / 2f) + SkinWidth;

            IsGrounded = false;
            bool didSlopeCheck = false;
            for (var i = 0; i < RayResolution; i++)
            {
                var origin = pos0 + i * Vector2.UnitX * (size.X - 2 * SkinWidth) / (RayResolution - 1);
                var ray = new Ray(origin, -Vector2.UnitY);
                Gizmos.DrawLine(origin, origin - Vector2.UnitY * MaxDownRayLength, Color.Yellow);

                // if there was no collision, skip
                if (!Physics.RayCast(ray, out var hit, MaxDownRayLength, groundLayer)) continue;
                Gizmos.DrawLine(origin, hit.Point, Color.Red);
                // check the angle of hit to see if we're colliding with a floor.   
                IsGrounded |= hit.Normal.Y >= MaxIncline;
                
                // check slope angle on first contact with ground
                if (didSlopeCheck) continue;
                CheckSlopeDownward(hit);
                didSlopeCheck = true;
            }
        }

        // Check the slope angle directly below the game object. Not sure how to do that with multiple casts.
        // TODO Check for slope in the direction of movement!
        private void CheckSlopeDownward(RayCastHit hit)
        {
            // get slope normal
            SlopeDirection = new Vector2(hit.Normal.Y, -hit.Normal.X);
            Gizmos.DrawLine(hit.Point, hit.Point + SlopeDirection, Color.Green, 3.0f);
            Gizmos.DrawLine(hit.Point, hit.Point + hit.Normal, Color.Red, 3.0f);
            // get slope angle
            SlopeAngleDown = MathUtils.AngleBetween(Vector2.UnitY, hit.Normal);

            // if the angle changes from zero, we are on a slope
            if (Math.Abs(SlopeAngleDown - slopeAngleDownPrev) > 1e-05)
            {
                IsOnSlope = true;
                Debug.WriteLine($"Slope Angle: {SlopeAngleDown}");
            }
            else if (SlopeAngleDown == 0) IsOnSlope = false;

            slopeAngleDownPrev = SlopeAngleDown;
        }
    }
}
