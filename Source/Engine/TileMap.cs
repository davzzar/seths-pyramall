using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TiledCS;

namespace Engine
{
    public sealed class TileMap : Component
    {
        private string loadFromContentMapPath;

        private string loadFromContentTileSetPath;

        private Tile[,] tiles;

        public void LoadFromContent(string mapPath, string tileSetPath)
        {
            if (string.IsNullOrWhiteSpace(mapPath))
            {
                throw new ArgumentNullException(nameof(mapPath));
            }

            if (string.IsNullOrWhiteSpace(tileSetPath))
            {
                throw new ArgumentNullException(nameof(mapPath));
            }

            this.loadFromContentMapPath = mapPath;
            this.loadFromContentTileSetPath = tileSetPath;
            this.LoadFromContentPath();
        }

        /// <inheritdoc />
        protected override void OnAwake()
        {
            this.LoadFromContentPath();
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentMapPath == null || this.loadFromContentTileSetPath == null || !SceneManager.IsReady)
            {
                return;
            }

            var map = new TiledMap($"Content/{this.loadFromContentMapPath}.tmx");        
            var tileSet = new TiledTileset($"Content/tiles/{this.loadFromContentTileSetPath}.tsx");
            var baseLayer = map.Layers[0];

            this.tiles = new Tile[baseLayer.width, baseLayer.height];

            // TODO: Load the tile map from the loaded xml representation
            
            this.loadFromContentMapPath = null;
        }

        private readonly struct Tile
        {
            public readonly Texture2D Texture;
        }
    }
}
