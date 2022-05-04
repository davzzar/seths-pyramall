using JetBrains.Annotations;
using SandPerSand.Scenes;

namespace SandPerSand
{
    public sealed class LevelLoader : ISceneLoader
    {
        public static readonly string Level0MapName = "test_level_1";

        public static readonly string DebugMapName = "debug_map";

        [NotNull]
        private string mapName;

        public LevelLoader([NotNull]string mapName)
        {
            this.mapName = mapName;
        }

        public void Load()
        {
            Template.MakeGameMap(this.mapName);
            Template.MakeGameCamera();
        }
    }
}