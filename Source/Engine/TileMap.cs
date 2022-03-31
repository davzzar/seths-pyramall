using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledCS;

namespace Engine
{
    public sealed class TileMap : Component
    {
        private string loadFromContentMapPath;

        private TiledMap sourceTiledMap;

        private Dictionary<int, TiledTileset> tiledsetsByFirstGridId;

        private TiledLayer[] tiledLayers;
        private TiledLayer baseLayer;
        private Tile[,] baseTiles;

        public void LoadFromContent(string mapPath)
        {
            if (string.IsNullOrWhiteSpace(mapPath))
            {
                throw new ArgumentNullException(nameof(mapPath));
            }

            this.loadFromContentMapPath = mapPath;
            this.LoadFromContentPath();
        }

        /// <inheritdoc />
        protected override void OnAwake()
        {
            this.LoadFromContentPath();
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentMapPath == null || !SceneManager.IsReady)
            {
                return;
            }

            sourceTiledMap = new TiledMap($"Content/{this.loadFromContentMapPath}.tmx");
            // Load all Tileset
            // Get paths of all TiledTileSet, then load them and store into a dic
            tiledsetsByFirstGridId = new Dictionary<int, TiledTileset>();
            foreach (TiledMapTileset tiledMS in sourceTiledMap.Tilesets)
            {
                string tiledsetPath = Path.GetFileNameWithoutExtension(tiledMS.source);
                var tiledS = new TiledTileset($"Content/tiles/{tiledsetPath}.tsx");
                if (tiledS == null)
                {
                    throw new NullReferenceException("Load tiledS Failed");
                }
                tiledsetsByFirstGridId.Add(tiledMS.firstgid, tiledS);
            }
            this.tiledLayers = sourceTiledMap.Layers;
            this.baseLayer = sourceTiledMap.Layers[0];



            // Load each Tile
            this.baseTiles = new Tile[baseLayer.width, baseLayer.height];

            for (int gridItr = 0; gridItr < baseLayer.data.Length; ++gridItr)
            {
                Tile newTile = new Tile();

                int gridId = baseLayer.data[gridItr];
                //TODO Could this happen????
                if (gridId <= 0)
                    continue;

                // get: gridId, firstGridId, tileId
                TiledMapTileset tiledMS = sourceTiledMap.GetTiledMapTileset(gridId);
                int firstGridId = tiledMS.firstgid;
                int tileId = gridId - firstGridId;

                // tile n->1 Tileset
                TiledTileset tiledS = tiledsetsByFirstGridId[firstGridId];

                // tile n->1 Texture
                System.Console.WriteLine("tiledS.Image:" + tiledS.Image);
                // TODO handle no texture: tiledS.Image == null
                string textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
                Texture2D texture = GameEngine.Instance.Content.Load<Texture2D>(textureAssetName);

                // (tileset,tile) 1->1 SourceRectangle
                
                Rectangle sourceRectangle = new Rectangle(
                    tileId * tiledS.TileWidth % texture.Width,
                    tileId * tiledS.TileWidth / texture.Width * tiledS.TileHeight,
                    tiledS.TileWidth,
                    tiledS.TileHeight);

                // tile 1->1 CoordinatesOnLayer
                int x = gridItr % baseLayer.width;
                int y = gridItr / baseLayer.height;
                Console.WriteLine("new Tile(texture:" + texture);
                baseTiles[x, y] = new Tile(texture, tiledS,sourceRectangle,x,y,gridId,firstGridId);
            }

            this.loadFromContentMapPath = null;
        }

        public void DrawMap(Color color,float depth)
        {
            foreach(Tile tile in baseTiles)
            {
                var matrix = this.Transform.LocalToWorld * Matrix3x3.CreateTranslation(new Vector2(-0.5f, 0.5f));
                
                if (tile.Texture != null)
                {
                    Graphics.DrawTile(tile.Texture, tile.SourceRectangle, color, ref matrix, depth);
                }
                else
                {
                    Console.WriteLine("tile.Texture == null,tileGridId:" + tile.GridId);
                }
                
            }
        }

        private readonly struct Tile
        {
            public readonly Texture2D Texture;
            public readonly TiledTileset TiledS;
            public readonly Rectangle SourceRectangle;
            //CoordinatesOnLayer
            public readonly int X;
            public readonly int Y;
            public readonly int GridId, FirstGridId;

            public Tile(Texture2D texture, TiledTileset tiledS, Rectangle sourceRectangle,int layerX,int layerY, int gridId, int firstGridId)
            {
                Texture = texture;
                TiledS = tiledS;
                SourceRectangle = sourceRectangle;
                X = layerX;
                Y = layerY;
                GridId = gridId;
                FirstGridId = firstGridId;

            }

        }
    }
}
