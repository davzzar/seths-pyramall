using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledCS;
using System.Diagnostics;

namespace Engine
{
    public sealed class TileMap<T> : Component where T : Layer,new()
    {
        private string loadFromContentMapPath;

        private TiledMap sourceMap;

        public Vector2 Size {
            get {
                return new Vector2(sourceMap.Width, sourceMap.Height);
            }
        }

        public bool Infinite { get { return sourceMap.Infinite; } }

        public T[] Layers
        {
            get;
            set;
        }

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
            if (this.loadFromContentMapPath == null || !this.Owner.Scene.IsLoaded)
            {
                return;
            }
            // Load tmx file
            var tiledMap = new TiledMap($"Content/{this.loadFromContentMapPath}.tmx");
            this.sourceMap = tiledMap;

            // Load tsx file
            var tiledsetsByFirstGridId = LoadTilesets(tiledMap.Tilesets);

            // Parse objects (colliders) for each Grid in each Tilesets
            var outlinesByGridId = ParseTilesetsObjects(tiledsetsByFirstGridId);

            // Init layers with properties
            this.Layers = InitLayers(tiledMap.Layers);

            // Load each Tile on each Layer
            foreach(Layer layer in this.Layers)
            {
                layer.ParseLayerProperties();
                layer.ParseTiles(tiledMap,tiledsetsByFirstGridId,outlinesByGridId);
            }

            this.loadFromContentMapPath = null;
        }

        private Dictionary<int, TiledTileset> LoadTilesets(TiledMapTileset[] sourceTilesets)
        {
            // Load all Tileset
            // Get paths of all TiledTileSet, then load them and store into a dic
            var tiledsetsByFirstGridId = new Dictionary<int, TiledTileset>();
            foreach (TiledMapTileset tiledMS in sourceTilesets)
            {
                string tiledsetPath = Path.GetFileNameWithoutExtension(tiledMS.source);
                var tiledS = new TiledTileset($"Content/tiles/{tiledsetPath}.tsx");
                if (tiledS == null)
                {
                    throw new NullReferenceException("Load tiledS Failed");
                }
                if (tiledS.Tiles.Length != tiledS.TileCount)
                {
                    throw new InvalidDataException("Invalid TiledSet:' " + tiledsetPath +
                        "'! Each Tile Must have a entry in .tsx file! Please " +
                        "ensure this by assign each Tile a Type in Tiled Editor!");
                }

                // Associate each Tiledset with its firstgid
                tiledsetsByFirstGridId.Add(tiledMS.firstgid, tiledS);
            }
            return tiledsetsByFirstGridId;
        }

        private Dictionary<int, Vector2[]> ParseTilesetsObjects(Dictionary<int, TiledTileset> tiledsetsByFirstGridId)
        {

            var outlinesByGridId = new Dictionary<int, Vector2[]>();

            foreach (var item in tiledsetsByFirstGridId)
            {
                int firstGridId = item.Key;
                TiledTileset tiledS = item.Value;
                Vector2 translatePoint(Vector2 vector)
                {
                    Vector2 result = new Vector2();
                    result.X = vector.X / tiledS.TileWidth - 0.5f;
                    result.Y = 0.5f - vector.Y / tiledS.TileHeight;
                    return result;
                }
                foreach (TiledTile tiledT in tiledS.Tiles)
                {
                    int gridId = tiledT.id + firstGridId;
                    foreach (TiledObject obj in tiledT.objects)
                    {

                        if (obj.polygon != null)//polygon collider
                        {
                            int pointsNum = obj.polygon.points.Length / 2;
                            var outline = new Vector2[pointsNum];
                            for (int i = 0; i < pointsNum; i += 1)
                            {
                                outline[i] = translatePoint(
                                    new Vector2(
                                        obj.x + obj.polygon.points[2 * i],
                                        obj.y + obj.polygon.points[2 * i + 1])
                                    );
                            }
                            outlinesByGridId.Add(gridId, outline);
                        }
                        else //rectangle collider
                        {
                            var outline = new Vector2[] {
                            translatePoint(new Vector2(obj.x, obj.y)),
                            translatePoint(new Vector2(obj.x+ obj.width, obj.y)),
                            translatePoint(new Vector2(obj.x + obj.width, obj.y + obj.height)),
                            translatePoint(new Vector2(obj.x, obj.y + obj.height))
                            };
                            outlinesByGridId.Add(gridId, outline);
                        }

                    }
                }
            }
            return outlinesByGridId;
        }

        private T[] InitLayers(TiledLayer[] tiledLayers)
        {
            var layers = new T[tiledLayers.Length];
            for (var i = 0; i < tiledLayers.Length; ++i)
            {
                layers[i] = new T();
                layers[i].TiledLayer = tiledLayers[i];
            }
            return layers;
        }

        public Dictionary<string, (int,string)> getItemTilesIdsAndPaths()
        {
            Dictionary<int, TiledTileset> tiledsetsByFirstGridId = LoadTilesets(sourceMap.Tilesets);

            var GridIdByItemId = new Dictionary<string, (int, string)>();

            foreach (var item in tiledsetsByFirstGridId)
            {
                int firstGridId = item.Key;
                TiledTileset tiledS = item.Value;
                string path = tiledS.Image.source;

                foreach (TiledTile tiledT in tiledS.Tiles)
                {
                    int gridId = tiledT.id + firstGridId;
                    if(tiledT.type == "Item" && tiledT.properties != null)
                    {
                        string item_id = null;
                        foreach(TiledProperty property in tiledT.properties)
                        {
                            if(property.name == "item_id")
                            {
                                item_id = property.value;
                            }
                        }
                        if(item_id != null)
                        {
                            GridIdByItemId.Add(item_id, (gridId, path));
                        }

                    }
                }
            }
            return GridIdByItemId;
        }
    }

    public class Layer
    {
        public TiledLayer TiledLayer { get; set; }
        public Tile[] Tiles { get; set; }
        public GameObject[] TileGos { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        private List<GameObject> tileGoList;

        public Layer()
        {
            TiledLayer = null;
            TileGos = null;
            TileWidth = 32;
            TileHeight = 32;
            this.tileGoList = new List<GameObject>();
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
            this.tileGoList = new List<GameObject>();
            foreach(Tile tile in Tiles){
                BuildTile(tile);
            }
            this.TileGos = tileGoList.ToArray();
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
            tile.TextureName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
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
            string textureAssetName = tile.TextureName;
            var tiledT = tile.TiledTile;
            var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
            tileRenderer.LoadFromContent(textureAssetName);
            tileRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth,tile.PixelHeight);

            // Add collider and other compounents for the Tile GameObject
            if (tile.ColliderOutline != null)
            {
                var tileCollider = newTileGo.AddComponent<PolygonCollider>();
                tileCollider.Outline = tile.ColliderOutline;
            }
            tileGoList.Add(newTileGo);
        }

    }

    public struct Tile
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

    }


}
