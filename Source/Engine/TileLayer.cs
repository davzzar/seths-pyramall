using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TiledCS;
using System.Diagnostics;

namespace Engine
{
    public class TileLayer
    {
        public TiledLayer TiledLayer { get; set; }
        public Tile[] Tiles { get; set; }
        public GameObject LayerGo { get; set; }
        public GameObject[,] TileGos { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public float Depth { get; set; }

        public TileLayer()
        {
            TiledLayer = null;
            LayerGo = new GameObject();
            TileGos = null;
            TileWidth = 32;
            TileHeight = 32;
        }

        public virtual void ParseLayerProperties()
        {
            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse properties" +
                    "before TiledLayer is ready");
            }

        }

        // Property 'public Tile[] Tiles' must be filled by this method
        public virtual void ParseTiles(TiledMap sourceTiledMap,
            Dictionary<int, TiledTileset> tiledsetsByFirstGridId,
            Dictionary<int, Vector2[]> outlinesByGridId)
        {
            this.TileWidth = sourceTiledMap.TileWidth;
            this.TileHeight = sourceTiledMap.TileHeight;
            TileGos = new GameObject[sourceTiledMap.Width,sourceTiledMap.Height];

            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse Tiles" +
                    "before TiledLayer is ready");
            }
            var tileList = new List<Tile>();
            for (int layerTileItr = 0; layerTileItr < TiledLayer.data.Length; ++layerTileItr)
            {
                // Start with gridId and firstGridId
                int gridId = TiledLayer.data[layerTileItr];
                if (gridId <= 0)
                    continue;
                int firstGridId = sourceTiledMap.GetTiledMapTileset(gridId).firstgid;
                // Get tileId
                int tileId = gridId - firstGridId;

                // Get outline (if applicable)
                Vector2[] outline = null;
                if (outlinesByGridId.ContainsKey(gridId))
                {
                    outline = outlinesByGridId[gridId];
                }

                var tile = ParseTileInfo(
                    layerTileItr,
                    gridId,
                    tileId,
                    tiledsetsByFirstGridId[firstGridId],
                    outline);
                tileList.Add(tile);

            }
            this.Tiles = tileList.ToArray();

            // Register Tile GameObjects
            foreach (Tile tile in Tiles)
            {
                BuildTile(tile);
            }
        }

        private Tile ParseTileInfo(int layerTileItr, int gridId, int tileId,
            TiledTileset tiledS, Vector2[] outline)
        {
            //Tile
            var tile = new Tile();
            // Get (X,Y) OnLayer
            tile.X = layerTileItr % TiledLayer.width;
            tile.Y = -layerTileItr / TiledLayer.width + TiledLayer.height - 1;
            // Get Tileset
            tile.PixelWidth = tiledS.TileWidth;
            tile.PixelHeight = tiledS.TileHeight;
            tile.GridWidth = tiledS.TileWidth / this.TileWidth;
            tile.GridHeight = tiledS.TileHeight / this.TileHeight;
            tile.TextureName = $"Tiled/TiledsetTexture/{Path.GetFileNameWithoutExtension(tiledS.Image.source)}";
            tile.TiledTile = tiledS.Tiles[tileId];
            //Debug.Print(tiledS.TileWidth + " / " + TiledLayer.width + "==" + tile.GridWidth);
            // Get outline (if applicable)
            tile.ColliderOutline = outline;
            return tile;
        }

        // Rewrite this method to parse customized tiles
        public virtual void BuildTile(Tile tile)
        {
            var newTileGo = new GameObject();
            TileGos[tile.X, tile.Y] = newTileGo;
            newTileGo.Transform.Parent = LayerGo.Transform;
            newTileGo.Transform.LocalPosition = new Vector2(tile.X, tile.Y);
            string textureAssetName = tile.TextureName;
            var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
            tileRenderer.LoadFromContent(textureAssetName);
            tileRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth, tile.PixelHeight);

            // Add collider and other compounents for the Tile GameObject
            if (tile.ColliderOutline != null)
            {
                var tileCollider = newTileGo.AddComponent<PolygonCollider>();
                tileCollider.Outline = tile.ColliderOutline;
            }

        }

        protected SpriteRenderer AddTileRenderer(GameObject Go, Tile tile)
        {
            var tileRenderer = Go.AddComponent<SpriteRenderer>();
            tileRenderer.LoadFromContent(tile.TextureName);
            tileRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth, tile.PixelHeight);
            tileRenderer.Depth = this.Depth;
            return tileRenderer;
        }

        protected PolygonCollider AddPolygonCollider(GameObject Go, Tile tile)
        {
            var outline = tile.ColliderOutline;
            if (outline != null)
            {
                var tileCollider = Go.AddComponent<PolygonCollider>();
                tileCollider.Outline = outline;
                return tileCollider;
            }
            return null;
        }

    }

    public class Tile
    {
        public int X;
        public int Y;
        public int ID { get => TiledTile.id; }
        public string Type { get => TiledTile.type; }

        public int PixelWidth;
        public int PixelHeight;
        public float GridWidth;
        public float GridHeight;

        public string TextureName;

        public TiledTile TiledTile;
        public Vector2[] ColliderOutline;

        public string GetTileProperty(string propertyName)
        {
            foreach (TiledProperty p in TiledTile.properties)
            {
                if (p.name == propertyName)
                {
                    return p.value;
                }
            }
            throw new Exception("Undefined property name!");
        }

    }
}
