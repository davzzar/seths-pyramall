using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class Time
    {
        private static double gameTime;

        private static float deltaTime;

        private static float timeScale = 1f;

        private static double unscaledGameTime;

        private static float unscaledDeltaTime;

        private static double frameUpdateTime = 0.001f;

        private static double frameDrawTime = 0.001f;

        /// <summary>
        /// Gets how much time passed since the start of the game in seconds, is affected by <see cref="TimeScale"/>.
        /// </summary>
        public static float GameTime => (float)gameTime;

        /// <summary>
        /// Gets how much time passed in the last frame in seconds, is affected by <see cref="TimeScale"/>.
        /// </summary>
        public static float DeltaTime => deltaTime;

        /// <summary>
        /// Gets or sets a time scale to let the game run faster (&gt;1), slower (&lt;1) or at normal speed (=1), cannot be negative.
        /// </summary>
        public static float TimeScale
        {
            get => timeScale;
            set
            {
                if (float.IsNaN(value))
                {
                    throw new ArgumentException("The time scale cannot be NaN.");
                }

                if (float.IsInfinity(value))
                {
                    throw new ArgumentException("The time scale cannot be infinite.");
                }

                timeScale = MathF.Max(0f, value);
            }
        }

        /// <summary>
        /// Gets how much time passed since the start of the game in seconds, not affected by <see cref="TimeScale"/>.
        /// </summary>
        public static float UnscaledGameTime => (float)unscaledGameTime;

        /// <summary>
        /// Gets how much time passed in the last frame in seconds, not affected by <see cref="TimeScale"/>.
        /// </summary>
        public static float UnscaledDeltaTime => unscaledDeltaTime;

        /// <summary>
        /// Gets the average time that was spent per frame for the Update loop.
        /// </summary>
        public static float AvgFrameUpdateTime => (float)frameUpdateTime;

        /// <summary>
        /// Gets the average time that was spent per frame for the Draw loop
        /// </summary>
        public static float AvgFrameDrawTime => (float)frameDrawTime;

        internal static void StartNextFrame(double newDeltaTime)
        {
            var scaled = newDeltaTime * timeScale;
            deltaTime = (float)scaled;
            gameTime += scaled;

            unscaledDeltaTime = (float)newDeltaTime;
            unscaledGameTime += newDeltaTime;
        }

        internal static void Init()
        {
            deltaTime = (1 / 60f) * timeScale;
            gameTime = 0f;

            unscaledDeltaTime = 1 / 60f;
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
