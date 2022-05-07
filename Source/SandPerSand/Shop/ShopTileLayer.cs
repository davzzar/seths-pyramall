using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledCS;
using Engine;
using System.Diagnostics;
using SandPerSand.SandSim;

namespace SandPerSand
{
    public class ShopTileLayer : TileLayer
    {
        public float Depth { get; set; }

        public ShopTileLayer() : base()
        {
            Depth = 0f;
        }

        public override void ParseLayerProperties()
        {
            if (TiledLayer == null)
            {
                throw new InvalidOperationException("Can't parse properties" +
                    "before TiledLayer is ready");
            }
            this.Depth = 0f;
            foreach (TiledProperty p in TiledLayer.properties)
            {
                if (p.name == "depth")
                {
                    this.Depth = float.Parse(p.value);
                    break;
                }
            }

        }

        public override void BuildTile(Tile tile)
        {
            string textureAssetName = tile.TextureName;
            var tiledT = tile.TiledTile;
            var tileId = tile.ID;
            var newTileGo = new GameObject();
            TileGos[tile.X, tile.Y] = newTileGo;
            newTileGo.Transform.Parent = LayerGo.Transform;
            newTileGo.Transform.LocalPosition = new Vector2(tile.X, tile.Y);
            newTileGo.Transform.LossyScale = new Vector2(tile.GridWidth,tile.GridHeight);


            // Add different behavior compounents according to tile.type
            switch (tiledT.type)
            {
                case "Platform":
                    // Add Renderer
                    _ = AddTileRenderer(newTileGo, tile);
                    // Add collider and other compounents for the Tile GameObject
                    _ = AddPolygonCollider(newTileGo, tile);
                    break;
                case "Entry":
                    _ = newTileGo.AddComponent<EntryScript>();
                    //_ = newTileGo.AddComponent<ShopEntryScript>();
                    break;
                case "Exit":
                    //_ = newTileGo.AddComponent<ShopExitScript>();
                    _ = AddTileRenderer(newTileGo, tile);
                    break;
                case "Item":
                    // Add Renderer
                    var shopItemScript = newTileGo.AddComponent<ShopItemScript>();
                    shopItemScript.ItemId = tile.GetTileProperty("item_id");
                    _ = AddTileRenderer(newTileGo, tile);
                    break;
            }
        }

        private SpriteRenderer AddTileRenderer(GameObject Go, Tile tile)
        {
            var tileRenderer = Go.AddComponent<SpriteRenderer>();
            tileRenderer.LoadFromContent(tile.TextureName);
            tileRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth, tile.PixelHeight);
            tileRenderer.Depth = this.Depth;
            return tileRenderer;
        }

        private PolygonCollider AddPolygonCollider(GameObject Go, Tile tile)
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
}
