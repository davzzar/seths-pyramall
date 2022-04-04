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
        // TiledTiles from ALL TiledTilesets
        private Dictionary<int, Vector2[]> outlinesByGridId;

        private TiledLayer[] tiledLayers;
        private TiledLayer baseLayer;
        private GameObject[] tileGos;


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
            // Load tmx file
            sourceTiledMap = new TiledMap($"Content/{this.loadFromContentMapPath}.tmx");

            // Load tsx file
            LoadTilesets();

            // Parse objects (collider) for each Grid in each Tilesets
            ParseTilesetsObjects();

            // Load layers TODO
            this.tiledLayers = sourceTiledMap.Layers;
            this.baseLayer = sourceTiledMap.Layers[0];

            // Load each Tile on the Layer TODO multiple layers
            var tileGoList = new List<GameObject>();
            for (int layerTileItr = 0; layerTileItr < baseLayer.data.Length; ++layerTileItr)
            {
                int gridId = baseLayer.data[layerTileItr];
                if (gridId <= 0)
                    continue;

                // Get gridId, firstGridId, tileId
                int firstGridId = sourceTiledMap.GetTiledMapTileset(gridId).firstgid;
                int tileId = gridId - firstGridId;

                // Get Tileset <- (TileOnLayer)
                TiledTileset tiledS = tiledsetsByFirstGridId[firstGridId];
                // Get Texture <- (Tileset)
                // TODO handle no texture: tiledS.Image == null
                string textureAssetName = Path.GetFileNameWithoutExtension(tiledS.Image.source);
                Texture2D texture = GameEngine.Instance.Content.Load<Texture2D>(textureAssetName);
                // Get SourceRectangle <- (Tileset,Texture,TileOnLayer)
                Rectangle sourceRectangle = new Rectangle(
                    tileId * tiledS.TileWidth % texture.Width,
                    tileId * tiledS.TileWidth / texture.Width * tiledS.TileHeight,
                    tiledS.TileWidth,
                    tiledS.TileHeight);

                // Get (X,Y)OnLayer <- 
                int x = layerTileItr % baseLayer.width;
                int y = -layerTileItr / baseLayer.height + baseLayer.height - 1;

                // Create Tile GameObject
                var newTileGo = new GameObject();
                newTileGo.Transform.Position = new Vector2(x, y);
                var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
                // TODO Depth of TileRenderer
                tileRenderer.Texture = texture;
                tileRenderer.SourceRect = sourceRectangle;

                if (outlinesByGridId.ContainsKey(gridId))
                {
                    var tileCollider = newTileGo.AddComponent<ShapeExampleComponent>();
                    tileCollider.Outline = outlinesByGridId[gridId];

                    Console.WriteLine("gridId:" + gridId);
                    foreach (Vector2 p in tileCollider.Outline)
                    {
                        //var point = newTileGo.Transform.TransformPoint(p);
                        Console.WriteLine(p.X + "," + p.Y);
                    }

                }
                // Register Tile GameObject
                tileGoList.Add(newTileGo);
            }
            // Register Tile GameObjects
            tileGos = tileGoList.ToArray();
            this.loadFromContentMapPath = null;
        }

        private void LoadTilesets()
        {
            if (sourceTiledMap == null)
            {
                return;
            }
            // Load all Tileset
            // Get paths of all TiledTileSet, then load them and store into a dic
            this.tiledsetsByFirstGridId = new Dictionary<int, TiledTileset>();
            foreach (TiledMapTileset tiledMS in sourceTiledMap.Tilesets)
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
        }

        private void ParseTilesetsObjects()
        {

            outlinesByGridId = new Dictionary<int, Vector2[]>();

            foreach (var item in tiledsetsByFirstGridId)
            {
                int firstGridId = item.Key;
                TiledTileset tiledS = item.Value;
                Vector2 translatePoint(Vector2 vector)
                {   //TODO explain the 0.5f offset
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
                            Console.WriteLine("polygon");
                            int pointsNum = obj.polygon.points.Length / 2;
                            var outline = new Vector2[pointsNum];
                            Console.WriteLine("gridId:" + gridId);
                            for (int i = 0; i < pointsNum; i += 1)
                            {
                                outline[i] = translatePoint(
                                    new Vector2(
                                        obj.x + obj.polygon.points[2 * i],
                                        obj.y + obj.polygon.points[2 * i + 1])
                                    );
                                Console.WriteLine("point[" + i + "]:" + outline[i].X + "," + outline[i].Y);
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
        }
    }

    //FIXME WHY drawed shape doesn't correpond to actual collider?
    public class ShapeExampleComponent : Behaviour
    {
        public Vector2[] Outline { get; set; }

        public Color Color { get; set; } = Color.White;

        /// <inheritdoc />
        protected override void OnAwake()
        {

            var collider = this.Owner.AddComponent<PolygonCollider>();

            if (this.Outline != null && this.Outline.Length >= 2)
            {
                collider.Outline = this.Outline;
            }
        }

        protected override void Update()
        {
            if (this.Outline == null || this.Outline.Length < 2)
            {
                return;
            }

            var p0 = this.Transform.TransformPoint(this.Outline[0]);
            var pCur = p0;

            for (var i = 0; i < this.Outline.Length - 1; i++)
            {
                var pNext = this.Transform.TransformPoint(this.Outline[i + 1]);
                Gizmos.DrawLine(pCur, pNext, Color.Red);
                pCur = pNext;
            }

            Gizmos.DrawLine(pCur, p0, Color.Red);
        }
    }

}
