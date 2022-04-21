using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledCS;
using Engine;

namespace SandPerSand
{
    
    public class MyLayer : Layer
    {
        public float Depth { get; set; }

        public MyLayer() : base()
        {
            Depth = 0f;
        }

        public override void ParseProperties()
        {
            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse properties" +
                    "before TiledLayer is ready");
            }
            this.Depth = 0f;
            foreach (TiledProperty p in TiledLayer.properties)
            {
                // TODO hard code
                if (p.name == "depth")
                {
                    this.Depth = float.Parse(p.value);
                    break;
                }
            }

        }


        public override void ParseTiles(TiledMap sourceTiledMap,
            Dictionary<int, TiledTileset> tiledsetsByFirstGridId,
            Dictionary<int, Vector2[]> outlinesByGridId)
        {
            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse Tiles" +
                    "before TiledLayer is ready");
            }
            // Parse layer properties TODO better structure
            var sourceLayer = this.TiledLayer;
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

                // Get (X,Y)OnLayer <- (Layer, LayerTile)
                int x = layerTileItr % sourceLayer.width;
                int y = -layerTileItr / sourceLayer.height + sourceLayer.height - 1;

                // Get Tile Properties
                var tile = tiledS.Tiles[tileId];

                // Create Tile GameObject
                var newTileGo = new GameObject();
                newTileGo.Transform.Position = new Vector2(x, y);

                // Add different behavior compounents according to tile.type
                switch (tile.type)
                {
                    case "Platform":
                        // Add Renderer
                        var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
                        tileRenderer.LoadFromContent(textureAssetName);
                        tileRenderer.SetSourceRectangle(tileId, tiledS.TileWidth, tiledS.TileHeight);
                        tileRenderer.Depth = this.Depth;

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
                        break;
                    case "Entry":
                        var entryScript = newTileGo.AddComponent<EntryScript>();
                        break;
                    case "Exit":
                        break;

                }
                //TODO group some tiles to a parent object then add script on the parent
                //TODO to simplify, only certain layers contains tiles that need to be combined?
                //TODO get parameters e.g moving routes from Tiled map objects

                // Register Tile GameObject
                tileGoList.Add(newTileGo);
            }
            // Register Tile GameObjects
            this.TileGos = tileGoList.ToArray();
        }

    }


}
