using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace SandPerSand
{
    public class ShopItemComp : Behaviour
    {

        // player referrences
        private PlayerIndex playerIndex;
        private InputHandler playerInput;

        private bool wasOnCollision;
        private GameObject infoGo;
        private int stock;
        private int price;
        private string itemId = null;
        public string ItemId
        {
            get => itemId;
            set
            {
                itemId = value;
                // set stock, price, description accordingly, probably aquire from a ItemWiki class

                InitShopItem();
            }
        }

        private bool BuyButtonPressed
        {
            get
            {
                if (playerInput == null) return false;
                return playerInput.getButtonState(Buttons.X) == ButtonState.Pressed;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            wasOnCollision = false;
            playerInput = null;
            if (ItemId != null)
            {
                InitShopItem();
            }
            var trigger = Owner.GetOrAddComponent<CircleTriggerComp>();
            trigger.Radius = 1f;
            trigger.CollisionEnter += this.OnCollisionEnter;
            trigger.CollisionExit += this.OnCollisionExit;
        }

        private void InitShopItem()
        {
            if (ItemId == null)
            {
                //ItemId = "lightning";
                throw new InvalidOperationException("ShopItemScript's itemId must be set before InitShopItem");
            }
            var item = ItemWiki.ItemNametoItem[itemId];
            // TODO better random num
            stock = new Random().Next(1, 4);
            //create item info go
            infoGo = new GameObject();
            infoGo.Transform.Parent = this.Transform;
            var infoComp = infoGo.AddComponent<ShopItemInfoComp>();
            price = item.Price;
            infoComp.FillInForm(ItemId, item.Price, stock, item.ImageName, item.Description);
            infoGo.IsEnabled = false;
        }

        protected override void Update()
        {
            if (BuyButtonPressed)
            {
                Buy();
            }
        }

        private void OnCollisionEnter(object sender, GameObject playerGo)
        {
            Debug.Print("oncollision enter");
            playerInput = playerGo.GetComponent<PlayerStates>().InputHandler;
            infoGo.IsEnabled = true;
        }

        private void OnCollisionExit(object sender, GameObject playerGo)
        {
            Debug.Print("oncollision exit");
            playerInput = null;
            infoGo.IsEnabled = false;
        }

        private void Buy()
        {
            if (stock > 0)
            {
                // TODO deduct players' coins check enough amount
                if(PlayersManager.Instance.spendCoins(this.playerIndex, this.price))
                {
                    stock--;
                    // TODO add item to player
                    // FIXME major boolean is hard coded
                    PlayersManager.Instance.addItemToInventory(this.playerIndex, this.ItemId, true);
                    Debug.Print("Shop item "+ItemId+"("+price+" coins): player" + playerIndex + " bought me");
                }
                else
                {
                    Debug.Print("Shop item " + ItemId + "(" + price + " coins): player" + playerIndex + " bought me but failed");
                }
                UpdateInfo();
            }
            else
            {
                Debug.Print("Shop item: out of stock");
                return;
            }
            //play animation
        }

        private void UpdateInfo()
        {
            if (infoGo != null)
            {
                infoGo.GetComponent<ShopItemInfoComp>().UpdateStock(stock);
            }
        }
    }
}
