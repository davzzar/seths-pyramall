using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TiledCS;

namespace SandPerSand
{
    public class ItemWiki : Component
    {
        public static Dictionary<string, Item> ItemNametoItem; //= new Dictionary<string, int>();
        public TiledTileset ItemTiledS; // new TiledTileset($"Content/tiles/TilesetItems.tsx");
        private string loadFromContentPath;

        public void LoadFromContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            this.loadFromContentPath = path;
            this.LoadFromContentPath();
        }

        private void ClearContent()
        {
            // clear existing content
            ItemNametoItem = new Dictionary<string, Item>();
            ItemTiledS = null;
        }

        private void LoadFromContentPath()
        {
            if (this.loadFromContentPath == null)//|| !this.Owner.Scene.IsLoaded
            {
                return;
            }
            ClearContent();
            ItemTiledS = new TiledTileset($"Content/Tiled/Tiledset/{loadFromContentPath}.tsx");
            if (ItemTiledS == null)
            {
                throw new NullReferenceException("Load tiledS Failed");
            }
            foreach (TiledTile tile in ItemTiledS.Tiles)
            {
                Item item = ParseTiledItem(tile, ItemTiledS);
                if (item.Name != null)
                    ItemNametoItem.Add(item.Name, item);
            }
        }

            private Item ParseTiledItem(TiledTile tile, TiledTileset tiledS)
        {
            Item item = new Item();
            foreach (TiledProperty p in tile.properties)
            {
                // TODO hard code
                switch (p.name)
                {
                    case "item_id":
                        item.Name = p.value;
                        break;
                    case "item_type":
                        item.Type = p.value;
                        break;
                    case "item_description":
                        item.Description = p.value;
                        break;
                    case "item_image_path":
                        // FIXME hard code
                        item.ImageName = $"shop/{Path.GetFileNameWithoutExtension(p.value)}";
                        break;
                    case "item_price":
                        item.Price = int.Parse(p.value);
                        break;
                }
            }
            return item;
        }
    }

    public class Item
    {
        public int TileID;
        public string Name;
        public string Type;
        public string Description;
        public string ImageName;
        public int Price;
    }
}