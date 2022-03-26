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

        private static double frameUpdateTime = 0.001f;

        private static double frameDrawTime = 0.001f;

        public static float GameTime => (float)gameTime;

        public static float DeltaTime => deltaTime;

        public static float AvgFrameUpdateTime => (float)frameUpdateTime;

        public static float AvgFrameDrawTime => (float)frameDrawTime;

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

        internal static void OnFrameUpdateFinished(TimeSpan elapsed)
        {
            frameUpdateTime = (elapsed.TotalSeconds + frameUpdateTime * 59.0) / 60.0;
        }

        internal static void OnFrameDrawFinished(TimeSpan elapsed)
        {
            frameDrawTime = (elapsed.TotalSeconds + frameDrawTime * 59.0) / 60.0;
        }
    }
}
