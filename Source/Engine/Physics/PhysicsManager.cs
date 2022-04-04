using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine
{
    internal static class PhysicsManager
    {
        private static readonly List<RigidBody> rigidBodies = new List<RigidBody>();

        private static readonly World world = new World();

        internal static World World => world;

        public static void Step()
        {
            foreach (var rb in rigidBodies)
            {
                rb.OnBeforePhysicsStep();
            }

            world.Step(Time.DeltaTime);

            foreach (var rb in rigidBodies)
            {
                rb.OnAfterPhysicsStep();
            }
        }

        public static void Init()
        {
            world.Clear();
            //world.Gravity = new Vector2(0f, -9.81f);
        }

        public static void Add([NotNull]RigidBody rigidBody)
        {
            Debug.Assert(rigidBody != null);
            Debug.Assert(!rigidBodies.Contains(rigidBody));
            rigidBodies.Add(rigidBody);
        }

        public static void Remove([NotNull] RigidBody rigidBody)
        {
            Debug.Assert(rigidBody != null);
            Debug.Assert(rigidBodies.Contains(rigidBody));
            rigidBodies.Remove(rigidBody);
        }
    }
}
