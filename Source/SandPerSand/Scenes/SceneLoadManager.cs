using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using JetBrains.Annotations;

namespace SandPerSand.Scenes
{
    public sealed class SceneLoadManager : Component
    {
        private static SceneLoadManager instance;

        public static SceneLoadManager Instance => instance;

        public void LoadScene([NotNull] ISceneLoader loader)
        {
            if (loader == null)
            {
                throw new ArgumentNullException(nameof(loader));
            }

            this.CloseOldScenes();

            var newScene = new Scene();
            var loaderGo = new GameObject("Load Scene Helper", newScene);
            var loaderComp = loaderGo.AddComponent<SceneLoadHelperComponent>();
            loaderComp.SceneLoader = loader;

            SceneManager.LoadSceneAdditive(newScene);
        }

        protected override void OnAwake()
        {
            if (instance != null && instance != this)
            {
                this.Destroy();
                throw new InvalidOperationException("SceneLoadManager is a singleton and cannot exist more than once.");
            }

            instance = this;
        }

        protected override void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void CloseOldScenes()
        {
            for (var i = SceneManager.OpenScenes.Count - 1; i >= 0; i++)
            {
                var scene = SceneManager.OpenScenes[i];

                if (scene == this.Owner.Scene)
                {
                    continue;
                }

                SceneManager.UnloadScene(scene);
            }
        }
    }
}
