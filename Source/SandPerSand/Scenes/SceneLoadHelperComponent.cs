using System;
using System.Collections.Generic;
using System.Text;
using Engine;

namespace SandPerSand.Scenes
{
    public sealed class SceneLoadHelperComponent : Behaviour
    {
        public ISceneLoader SceneLoader { get; set; }

        protected override void Update()
        {
            if (this.SceneLoader != null)
            {
                this.SceneLoader.Load();
            }

            this.Destroy();
        }
    }
}
