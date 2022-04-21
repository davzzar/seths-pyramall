using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledCS;

namespace Engine
{
    public sealed class TileMap<T> : Component where T : Layer,new()
    {
        private string loadFromContentMapPath;

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
            var sourceTiledMap = new TiledMap($"Content/{this.loadFromContentMapPath}.tmx");

            // Load tsx file
            var tiledsetsByFirstGridId = LoadTilesets(sourceTiledMap.Tilesets);

            // Parse objects (colliders) for each Grid in each Tilesets
            var outlinesByGridId = ParseTilesetsObjects(tiledsetsByFirstGridId);

            // Init layers with properties
            this.Layers = InitLayers(sourceTiledMap.Layers);

            // Load each Tile on each Layer
            foreach(Layer layer in this.Layers)
            {
                layer.ParseLayerProperties();
                layer.ParseTiles(sourceTiledMap,tiledsetsByFirstGridId,outlinesByGridId);
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
                    throw new InvalidDataException("Invalid TiledSet! " +
                        "Each Tile Must have a entry in .tsx file! Please " +
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
                            translatePoint(new Vector2(obj.x + obj.width, obj.y + obj.width)),
                            translatePoint(new Vector2(obj.x, obj.y + obj.width))
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
    }

    public class Layer
    {
        public TiledLayer TiledLayer { get; set; }
        public GameObject[] TileGos { get; set; }

        public Layer()
        {
            TiledLayer = null;
            TileGos = null;
        }

        public virtual void ParseLayerProperties()
        {
            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse properties" +
                    "before TiledLayer is ready");
            }

        }


        public void ParseTiles(TiledMap sourceTiledMap,
            Dictionary<int, TiledTileset> tiledsetsByFirstGridId,
            Dictionary<int, Vector2[]> outlinesByGridId)
        {
            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse Tiles" +
                    "before TiledLayer is ready");
            }

            var tileGoList = new List<GameObject>();

            for (int layerTileItr = 0; layerTileItr < TiledLayer.data.Length; ++layerTileItr)
            {
                // Start with gridId and firstGridId
                int gridId = TiledLayer.data[layerTileItr];
                if (gridId <= 0)
                    continue;
                int firstGridId = sourceTiledMap.GetTiledMapTileset(gridId).firstgid;

                // Get (X,Y) OnLayer
                int x = layerTileItr % TiledLayer.width;
                int y = -layerTileItr / TiledLayer.height + TiledLayer.height - 1;

                // Create GameObject
                var newTileGo = new GameObject();
                newTileGo.Transform.Position = new Vector2(x, y);

                // Get tileId
                int tileId = gridId - firstGridId;

                // Get Tileset
                TiledTileset tiledS = tiledsetsByFirstGridId[firstGridId];

                // Get outline (if applicable)
                Vector2[] outline = null;
                if (outlinesByGridId.ContainsKey(gridId))
                {
                    outline = outlinesByGridId[gridId];
                }

                // Parse Tile
                ParseTile(newTileGo, tileId, tiledS, outline);

                // Register Tile GameObject
                tileGoList.Add(newTileGo);
            }
            // Register Tile GameObjects
            this.TileGos = tileGoList.ToArray();
        }

        public virtual void ParseTile(GameObject newTileGo, int tileId,
            TiledTileset tiledS, Vector2[] outline)
        {
            string textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
            var tiledT = tiledS.Tiles[tileId];

            var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
            tileRenderer.LoadFromContent(textureAssetName);
            tileRenderer.SetSourceRectangle(tileId, tiledS.TileWidth, tiledS.TileHeight);

            // Add collider and other compounents for the Tile GameObject
            if (outline!=null && outline.Length >= 3)
            {
                var tileCollider = newTileGo.AddComponent<PolygonCollider>();
                tileCollider.Outline = outline;
            }
        }

    }


}
