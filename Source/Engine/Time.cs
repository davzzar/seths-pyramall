using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class Time
    {
        private static double gameTime;

        private static float deltaTime;

        public static float GameTime
        {
            get => (float)gameTime;
        }

        public static float DeltaTime => deltaTime;

        internal static void StartNextFrame(double newDeltaTime)
        {
            deltaTime = (float)newDeltaTime;
            gameTime += newDeltaTime;
        }

        internal static void Init()
        {
            deltaTime = 1 / 60f;
            gameTime = 0f;
        }
    }
}
