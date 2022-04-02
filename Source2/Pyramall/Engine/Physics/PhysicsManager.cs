using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Dynamics;

namespace Engine.Physics
{
    internal static class PhysicsManager
    {
        private static readonly List<RigidBody> rigidBodies = new List<RigidBody>();

        private static readonly World world = new World();

        internal static World World => world;
        
        private static void SetupWorld()
        {
            world.Clear();
            world.Gravity = new Vector2(0f, -9.81f);
        }
    }
}
