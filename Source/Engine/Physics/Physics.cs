using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine
{
    public static class Physics
    {
        private static RayCastHit hitResult;

        private static bool hasHit;

        private static float minFraction;

        private static LayerMask rayMask;

        // Cache the callback to prevent repeated delegate allocations
        private static RayCastReportFixtureDelegate singleRayCast = RayCastCallback;

        public static bool RayCast(in Ray ray, out RayCastHit hit) => 
            RayCast(in ray, out hit, 1000f, LayerMask.All);

        public static bool RayCast(in Ray ray, out RayCastHit hit, float maxDistance) =>
            RayCast(in ray, out hit, maxDistance, LayerMask.All);

        public static bool RayCast(in Ray ray, out RayCastHit hit, float maxDistance, LayerMask mask)
        {
            maxDistance = MathF.Abs(maxDistance);

            hasHit = false;
            rayMask = mask;
            minFraction = float.PositiveInfinity;

            PhysicsManager.World.RayCast(singleRayCast, ray.Origin, ray.Origin + ray.Direction * maxDistance);
            hit = hitResult;
            hitResult = default;    // Reset hitResult to remove reference to fixture that would prevent GC from collecting it.
            return hasHit;
        }

        private static float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            if ((rayMask.Value & (int)fixture.CollisionCategories) == 0)
            {
                return -1f;
            }

            hasHit = true;

            if (fraction < minFraction)
            {
                minFraction = fraction;
                hitResult = new RayCastHit(fixture, point, normal, fraction);
            }

            return 1f;
        }
    }
}
