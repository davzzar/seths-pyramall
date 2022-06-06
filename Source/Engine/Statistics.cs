using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    /// <summary>
    /// Provides information about engine states and usage.
    /// </summary>
    public static class Statistics
    {
        public static int AliveGameObjectCount { get; internal set; }

        public static int ActiveGameObjectCount { get; internal set; }

        public static int ActiveRendererCount { get; internal set; }
    }
}
