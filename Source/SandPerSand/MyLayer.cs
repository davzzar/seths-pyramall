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
    
    public class MyLayer : TileLayer
    {
        public float Depth { get; set; }

        public MyLayer() 
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
                else if(p.name == "SimulationSpeed")
                {
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    sandSim.SimulationStepTime = float.Parse(p.value) * 2 * 1f / 80;
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

            var outline = tile.ColliderOutline;
            // Add different behavior compounents according to tile.type
            switch (tiledT.type)
            {
                case "Platform":
                    // Add Renderer
                    var tileRenderer = newTileGo.AddComponent<SpriteRenderer>();
                    tileRenderer.LoadFromContent(textureAssetName);
                    tileRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth, tile.PixelHeight);
                    tileRenderer.Depth = this.Depth;

                    // Add collider and other compounents for the Tile GameObject
                    if (outline != null)
                    {
                        var tileCollider = newTileGo.AddComponent<PolygonCollider>();
                        tileCollider.Outline = outline;
                    }
                    break;
                case "Entry":
                    var entryScript = newTileGo.AddComponent<EntryScript>();
                    break;
                case "Exit":
                    var exitScript = newTileGo.AddComponent<ExitScript>();
                    var exitRenderer = newTileGo.AddComponent<SpriteRenderer>();
                    exitRenderer.LoadFromContent(textureAssetName);
                    exitRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth, tile.PixelHeight);
                    exitRenderer.Depth = this.Depth;
                    break;
                case "Coin":
                    // Add Renderer
                    var itemRenderer = newTileGo.AddComponent<SpriteRenderer>();
                    var itemCollectable = newTileGo.AddComponent<Collectable>();
                    itemCollectable.init(CollectableType.coin, "Coin", outline);
                    itemRenderer.LoadFromContent(textureAssetName);
                    itemRenderer.SetSourceRectangle(tile.ID, tile.PixelWidth, tile.PixelHeight);
                    itemRenderer.Depth = this.Depth;
                    break;
                case "Item":
                    // Add Renderer
                    var coinRenderer = newTileGo.AddComponent<SpriteRenderer>();
                    var coinCollectable = newTileGo.AddComponent<Collectable>();
                    string itemId = "";
                    foreach (TiledProperty property in tiledT.properties)
                    {
                        Debug.Print(property.ToString());
                        if (property.name == "item_id")
                        {
                            itemId = property.value;
                        }
                    }
                    coinCollectable.init(CollectableType.item, itemId, outline);
                    coinRenderer.LoadFromContent(textureAssetName);
                    coinRenderer.SetSourceRectangle(tileId, tile.PixelWidth, tile.PixelHeight);
                    coinRenderer.Depth = this.Depth;
                    break;
                case "SandSource":
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    int size = 0;
                    foreach (TiledProperty property in tiledT.properties)
                    {
                        Debug.Print(property.ToString());
                        if (property.name == "size")
                        {
                            size = int.Parse(property.value);
                        }
                    }
                    Vector2 pos = newTileGo.Transform.Position;
                    Debug.Print("test");
                    Debug.Print((pos.X + 0.5f).ToString());
                    Debug.Print((pos.Y + 0.5f).ToString());

                    sandSim.AddSandSource(new Aabb(pos.X, pos.Y, size / 64f, size / 64f));
                    //sandSim.AddSandSource(new Aabb(29f, 48.5f, 0.2f, 0.2f));

                    break;
            }
        }
    }
}
