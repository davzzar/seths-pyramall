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
        

        private GameObject itemImgGo;
        private GameObject priceGo;
        private GameObject stockGo;
        private GameObject soldGo;
        private GameObject buttonGo; 

        protected override void OnAwake()
        {
            base.OnAwake();

            InitForm();
            infoRenderer.LoadFromContent("shop/iteminfo");
            buttonGo.GetComponent<TextRenderer>().Text = "Press 'X' to Buy";

        }

        private void InitForm()
        {
            // TODO hard code
            Transform.LocalPosition = new Vector2(3, 4);
            // TODO notice the length and width of the original image
            Transform.LossyScale = new Vector2(4.5f, 6f);
            infoRenderer = Owner.AddComponent<SpriteRenderer>();
            infoRenderer.Depth = .1f;

            //add image TODO hard code
            itemImgGo = new GameObject();
            itemImgGo.Transform.Parent = Transform;
            itemImgGo.Transform.LocalPosition = new Vector2(0, .25f);
            itemImgGo.Transform.LossyScale = new Vector2(.75f, .6f);
            var itemImgRenderer = itemImgGo.AddComponent<SpriteRenderer>();
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
            btnTextR.Color = Color.Yellow;
            btnTextR.FontSize = .1f;
            btnTextR.Depth = .05f;
        }

        
        public void FillInForm(string itemId, int itemPrice,int itemStock,
            string itemImg,string itemDescription)
        {
            // variable
            priceGo.GetComponent<TextRenderer>().Text = itemPrice + " coins";
            stockGo.GetComponent<TextRenderer>().Text = "Remaining: " + itemStock;
            itemImgGo.GetComponent<SpriteRenderer>().LoadFromContent(itemImg);
            if (itemStock <= 0)
            {
                MarkSold();
            }
        }

        public void UpdateStock(int stock)
        {
            // update stock
            // if sold
            if (stock <= 0)
            {
                MarkSold();
            }
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
    }
}
