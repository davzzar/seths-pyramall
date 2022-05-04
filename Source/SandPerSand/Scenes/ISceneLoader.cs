using System;
using System.Collections.Generic;
using System.Text;
using Engine;

namespace SandPerSand.Scenes
{
    public interface ISceneLoader
    {
        /// <summary>
        /// Loads the scene associated with this loader.
        /// </summary>
        void Load();
    }
}
