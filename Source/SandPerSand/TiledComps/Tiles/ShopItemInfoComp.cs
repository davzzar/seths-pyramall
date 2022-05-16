using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace SandPerSand
{
    public class ShopItemInfoComp : Behaviour
    {
        private SpriteRenderer infoRenderer;


        private GameObject bgGo;
        private GameObject itemImgGo;
        private GameObject nameGo;
        private GameObject descriptionGo;
        private GameObject flavourGo;
        private GameObject priceGo;
        private GameObject stockGo;
        private GameObject soldGo;
        private GameObject buttonGo;

        protected override void OnAwake()
        {
            base.OnAwake();

            InitForm();
            infoRenderer.LoadFromContent("shop/iteminfo");
            buttonGo.GetComponent<TextRenderer>().Text = "Press 'X' to Buy Item";

        }

        private void InitForm()
        {
            // TODO hard code
            Transform.LocalPosition = new Vector2(3, 4);
            // TODO notice the length and width of the original image
            Transform.LossyScale = new Vector2(4.5f, 6f);
            infoRenderer = Owner.AddComponent<SpriteRenderer>();
            infoRenderer.Depth = .1f;

            // TODO replace the above with this.
            /*bgGo = new GameObject();
            bgGo.Transform.LocalPosition = new Vector2(3, 4);
            bgGo.Transform.LossyScale = new Vector2(4.5f, 6f);
            infoRenderer = bgGo.AddComponent<SpriteRenderer>();
            infoRenderer.Depth = .1f;*/

            // usage icon
            itemImgGo = new GameObject();
            itemImgGo.Transform.Parent = Transform;
            itemImgGo.Transform.LocalPosition = new Vector2(-0.3f, .32f);
            itemImgGo.Transform.LossyScale = new Vector2(.35f, .3f);
            var itemImgRenderer = itemImgGo.AddComponent<SpriteRenderer>();
            itemImgRenderer.Depth = .05f;

            // name
            nameGo = new GameObject();
            nameGo.Transform.Parent = Transform;
            nameGo.Transform.LocalPosition = new Vector2(-0.075f, 0.45f);
            var nameRenderer = nameGo.AddComponent<TextRenderer>();
            nameRenderer.Color = Color.White;
            nameRenderer.FontSize = .11f;
            itemImgRenderer.Depth = .05f;

            // description
            descriptionGo = new GameObject();
            descriptionGo.Transform.Parent = Transform;
            descriptionGo.Transform.LocalPosition = new Vector2(-0.075f, 0.35f);
            var descRenderer = descriptionGo.AddComponent<TextRenderer>();
            descRenderer.Color = Color.White;
            descRenderer.FontSize = .075f;
            descRenderer.Depth = .05f;

            // flavour
            flavourGo = new GameObject();
            flavourGo.Transform.Parent = Transform;
            flavourGo.Transform.LocalPosition = new Vector2(-0.075f, -0.15f);
            var flavourRenderer = flavourGo.AddComponent<TextRenderer>();
            flavourRenderer.Color = Color.Cyan;
            flavourRenderer.FontSize = .075f;
            flavourRenderer.Depth = .05f;

            // add price
            priceGo = new GameObject();
            priceGo.Transform.Parent = Transform;
            priceGo.Transform.LocalPosition = new Vector2(-.5f, 0);
            var priceTextR = priceGo.AddComponent<TextRenderer>();
            //priceTextR.Text = price + " coins";
            priceTextR.Color = Color.Yellow;
            priceTextR.FontSize = .075f;
            priceTextR.Depth = .05f;

            // add stock
            stockGo = new GameObject();
            stockGo.Transform.Parent = Transform;
            stockGo.Transform.LocalPosition = new Vector2(-.5f, -.1f);
            var stockTextR = stockGo.AddComponent<TextRenderer>();
            //stockTextR.Text = "Remaining: "+ stock;
            stockTextR.Color = Color.Yellow;
            stockTextR.FontSize = .075f;
            stockTextR.Depth = .05f;
            
            // add "Press X to Buy"
            buttonGo = new GameObject();
            buttonGo.Transform.Parent = Transform;
            buttonGo.Transform.LocalPosition = new Vector2(-.34f, -.38f);
            var btnTextR = buttonGo.AddComponent<TextRenderer>();
            btnTextR.Color = Color.Yellow;
            btnTextR.FontSize = .075f;
            btnTextR.Depth = .05f;
        }

        
        public void FillInForm(Item item, int itemStock)
        {
            itemImgGo.GetComponent<SpriteRenderer>()!.LoadFromContent(item.UsageIconPath);
            nameGo.GetComponent<TextRenderer>()!.Text = item.DisplayName;
            descriptionGo.GetComponent<TextRenderer>()!.Text = item.Description;
            flavourGo.GetComponent<TextRenderer>()!.Text = item.FlavourText;

            // price
            priceGo.GetComponent<TextRenderer>()!.Text = item.Price + " coins";
            UpdateStock(itemStock);
        }

        public void UpdateStock(int stock)
        {
            // update stock
            // if sold
            if (stock <= 0)
            {
                MarkSold();
            }
            stockGo.GetComponent<TextRenderer>().Text = "(" + stock + " in stock)";
        }

        private void MarkSold()
        {
            //
            soldGo = new GameObject();
            soldGo.Transform.Parent = this.Transform;
            soldGo.Transform.LocalPosition = new Vector2(0, .25f);
            soldGo.Transform.LossyScale = new Vector2(.75f, .6f);
            var soldRen = soldGo.AddComponent<SpriteRenderer>();
            // FIXME hard code filepath
            soldRen.LoadFromContent("shop/sold");
            soldRen.Depth = .04f;
        }
    }
}
