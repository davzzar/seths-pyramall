using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Myra;
using Myra.Graphics2D.UI;

namespace Engine
{
    public static class UI
    {
        [CanBeNull]
        private static Desktop desktop;

        [CanBeNull]
        private static Widget rootWidget;

        public static bool IsMouseVisible
        {
            get => GameEngine.Instance.IsMouseVisible;
            set => GameEngine.Instance.IsMouseVisible = value;
        }

        [CanBeNull]
        public static Widget Root
        {
            get => desktop?.Root;
            set
            {
                rootWidget = value;

                if (desktop != null)
                {
                    desktop.Root = rootWidget;
                }
            }
        }

        internal static void Initialize()
        {
            MyraEnvironment.Game = GameEngine.Instance;
            desktop = new Desktop();

            if (rootWidget != null)
            {
                desktop.Root = rootWidget;
            }
        }

        internal static void Draw()
        {
            if (desktop == null)
            {
                return;
            }

            desktop.Render();
        }
    }
}
