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
    
    public class LevelTileLayer : TileLayer
    {
        public float Depth { get; set; }

        public LevelTileLayer() 
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
                    _ = AddTileRenderer(newTileGo, tile);
                    // Add collider and other compounents for the Tile GameObject
                    _ = AddPolygonCollider(newTileGo, tile);
                    break;
                case "Entry":
                    var entryScript = newTileGo.AddComponent<EntryScript>();
                    break;
                case "Exit":
                    var exitScript = newTileGo.AddComponent<ExitScript>();
                    _ = AddTileRenderer(newTileGo, tile);
                    break;
                case "Coin":
                    _ = AddTileRenderer(newTileGo, tile);
                    var itemCollectable = newTileGo.AddComponent<Collectable>();
                    itemCollectable.init(CollectableType.coin, "Coin", outline);
                    break;
                case "Stone":
                    _ = AddTileRenderer(newTileGo, tile);
                    var stonComp = newTileGo.AddComponent<TripStone>();
                    break;
                case "Item":
                    _ = AddTileRenderer(newTileGo, tile);
                    var coinCollectable = newTileGo.AddComponent<Collectable>();
                    string itemId = "";
                    foreach (TiledProperty property in tiledT.properties)
                    {
                        if (property.name == "item_id")
                        {
                            itemId = property.value;
                        }
                    }
                    coinCollectable.init(CollectableType.item, itemId, outline);
                    break;
                case "SandSource":
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    int size = 0;
                    foreach (TiledProperty property in tiledT.properties)
                    {
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
