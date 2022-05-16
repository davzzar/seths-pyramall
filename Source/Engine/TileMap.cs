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
    public sealed class TileMap<T> : Component where T : TileLayer,new()
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

        public void LoadFromContent(string contentName)
        {
            if (string.IsNullOrWhiteSpace(contentName))
            {
                throw new ArgumentNullException(nameof(contentName));
            }

            this.loadFromContentMapPath = $"Content/Tiled/Tiledmap/{contentName}.tmx";
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
            var tiledMap = new TiledMap(loadFromContentMapPath);
            this.sourceMap = tiledMap;

            // Load tsx file
            var tiledsetsByFirstGridId = LoadTilesets(tiledMap.Tilesets);

            // Parse objects (colliders) for each Grid in each Tilesets
            var outlinesByGridId = ParseTilesetsObjects(tiledsetsByFirstGridId);

            // Init layers with properties
            this.Layers = InitLayers(tiledMap.Layers);

            // Load each Tile on each Layer
            foreach(TileLayer layer in this.Layers)
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
                var tiledS = new TiledTileset(Path.Join(
                    Path.GetDirectoryName(loadFromContentMapPath), tiledMS.source));
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
    }




}
