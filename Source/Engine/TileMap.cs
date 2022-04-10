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

        public Layer[] Layers
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
            this.Layers = ParseLayersProperties(sourceTiledMap.Layers);

            // Load each Tile on each Layer
            foreach(Layer layer in this.Layers)
            {
                ParseTiles(layer,sourceTiledMap,tiledsetsByFirstGridId,outlinesByGridId);
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

        private Layer[] ParseLayersProperties(TiledLayer[] tiledLayers)
        {
            var layers = new Layer[tiledLayers.Length];
            for (var i = 0; i < tiledLayers.Length; ++i)
            {
                var tiledLayer = tiledLayers[i];
                var layerDepth = 0f;

                foreach (TiledProperty p in tiledLayer.properties)
                {
                    // TODO hard code
                    if (p.name == "depth")
                    {
                        layerDepth = float.Parse(p.value);
                        break;
                    }
                }
                layers[i] = new Layer(tiledLayer, layerDepth);
            }
            return layers;
        }

        private void ParseTiles(Layer layer,TiledMap sourceTiledMap,
            Dictionary<int, TiledTileset> tiledsetsByFirstGridId,
            Dictionary<int, Vector2[]> outlinesByGridId)
        {
            // Parse layer properties TODO better structure
            var sourceLayer = layer.TiledLayer;
            var tileGoList = new List<GameObject>();

            for (int layerTileItr = 0; layerTileItr < sourceLayer.data.Length; ++layerTileItr)
            {
                // Get gridId, firstGridId, tileId
                int gridId = sourceLayer.data[layerTileItr];
                if (gridId <= 0)
                    continue;
                int firstGridId = sourceTiledMap.GetTiledMapTileset(gridId).firstgid;
                int tileId = gridId - firstGridId;

                // Get Tileset <- (LayerTile)
                TiledTileset tiledS = tiledsetsByFirstGridId[firstGridId];
                // Get Texture <- (Tileset)
                // TODO handle no texture: tiledS.Image == null
                string textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
                Texture2D texture = GameEngine.Instance.Content.Load<Texture2D>(textureAssetName);
                // Get SourceRectangle <- (Tileset, Texture, TilesetTile)
                Rectangle sourceRectangle = new Rectangle(
                    tileId * tiledS.TileWidth % texture.Width,
                    tileId * tiledS.TileWidth / texture.Width * tiledS.TileHeight,
                    tiledS.TileWidth,
                    tiledS.TileHeight);

                // Get (X,Y)OnLayer <- (Layer, LayerTile)
                int x = layerTileItr % sourceLayer.width;
                int y = -layerTileItr / sourceLayer.width + sourceLayer.height - 1;

                // Create Tile GameObject
                var newTileGo = new GameObject();
                newTileGo.Transform.Position = new Vector2(x, y);
                var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
                tileRenderer.Texture = texture;
                tileRenderer.SourceRect = sourceRectangle;
                tileRenderer.Depth = layer.Depth;

                // Add collider and other compounents for the Tile GameObject
                if (outlinesByGridId.ContainsKey(gridId))
                {
                    var outline = outlinesByGridId[gridId];

                    var tileCollider = newTileGo.AddComponent<PolygonCollider>();

                    if (outline.Length >= 3)
                    {
                        tileCollider.Outline = outline;
                    }
                }

                //TODO add behavior compounents here
                //TODO group some tiles to a parent object then add script on the parent
                //TODO to simplify, only certain layers contains tiles that need to be combined?
                //TODO get parameters e.g moving routes from Tiled map objects

                // Register Tile GameObject
                tileGoList.Add(newTileGo);
            }
            // Register Tile GameObjects
            layer.TileGos = tileGoList.ToArray();
        }


        public class Layer
        {
            public readonly TiledLayer TiledLayer;
            public readonly float Depth;
            public GameObject[] TileGos;
            public Layer(TiledLayer tiledLayer, float depth)
            {
                TiledLayer = tiledLayer;
                Depth = depth;
                TileGos = null;
            }
        }
    }
}
