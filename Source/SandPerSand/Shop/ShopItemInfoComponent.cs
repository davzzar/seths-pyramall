using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace SandPerSand
{
    public class ShopItemInfoComponent : Behaviour
    {
        private SpriteRenderer infoRenderer;
        private GameObject soldGo;

        private GameObject itemImgGo;
        private int price;
        private GameObject priceGo;
        private int stock;
        private GameObject stockGo;
        private GameObject buttonGo; 

        protected override void OnAwake()
        {
            base.OnAwake();

            AddInfo();
        }

        public void AddInfo()
        {
            // TODO hard code
            Transform.LocalPosition = new Vector2(3, 4);
            // TODO notice the length and width of the original image
            Transform.LossyScale = new Vector2(4.5f, 6f);
            infoRenderer = Owner.AddComponent<SpriteRenderer>();
            infoRenderer.LoadFromContent("shop/iteminfo");
            infoRenderer.Depth = .1f;

            //add image TODO hard code
            itemImgGo = new GameObject();
            itemImgGo.Transform.Parent = Transform;
            itemImgGo.Transform.LocalPosition = new Vector2(0, .25f);
            itemImgGo.Transform.LossyScale = new Vector2(.75f, .6f);
            var itemImgRenderer = itemImgGo.AddComponent<SpriteRenderer>();
            itemImgRenderer.LoadFromContent("shop/item_unknown");
            itemImgRenderer.Depth = .05f;

            // add price
            priceGo = new GameObject();
            priceGo.Transform.Parent = Transform;
            priceGo.Transform.LocalPosition = new Vector2(-.5f, 0);
            var priceTextR = priceGo.AddComponent<TextRenderer>();
            //priceTextR.Text = price + " coins";
            priceTextR.Color = Color.Yellow;
            priceTextR.FontSize = .1f;
            priceTextR.Depth = .05f;
            // add stock
            stockGo = new GameObject();
            stockGo.Transform.Parent = Transform;
            stockGo.Transform.LocalPosition = new Vector2(-.5f, -.1f);
            var stockTextR = stockGo.AddComponent<TextRenderer>();
            //stockTextR.Text = "Remaining: "+ stock;
            stockTextR.Color = Color.Yellow;
            stockTextR.FontSize = .1f;
            stockTextR.Depth = .05f;
            // add "Press X to Buy"
            buttonGo = new GameObject();
            buttonGo.Transform.Parent = Transform;
            buttonGo.Transform.LocalPosition = new Vector2(-.3f, -.35f);
            var btnTextR = buttonGo.AddComponent<TextRenderer>();
            btnTextR.Text = "Press 'X' to Buy";
            btnTextR.Color = Color.Yellow;
            btnTextR.FontSize = .1f;
            btnTextR.Depth = .05f;
        }

        public void UpdateInfo(int price,int stock)
        {
            // update stock
            this.price = price;
            this.stock = stock;
            // if sold
            if (stock <= 0)
            {
                MarkSold();
            }
            priceGo.GetComponent<TextRenderer>().Text = price + " coins";
            stockGo.GetComponent<TextRenderer>().Text = "Remaining: " + stock;
        }

        private void MarkSold()
        {
            //
            soldGo = new GameObject();
            soldGo.Transform.Parent = this.Transform;
            soldGo.Transform.LocalPosition = new Vector2(0, .25f);
            soldGo.Transform.LossyScale = new Vector2(.75f, .6f);
            var soldRen = soldGo.AddComponent<SpriteRenderer>();
            soldRen.LoadFromContent("shop/sold");
            soldRen.Depth = .04f;
        }

        public void DestoryChildren()
        {
            if (itemImgGo != null) itemImgGo.Destroy();
            if (priceGo != null) priceGo.Destroy();
            if (stockGo != null) stockGo.Destroy();
            if (buttonGo != null) buttonGo.Destroy();
            if(soldGo!=null) soldGo.Destroy();
        }
    }
}
