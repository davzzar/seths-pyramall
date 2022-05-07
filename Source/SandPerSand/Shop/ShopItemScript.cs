using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace SandPerSand
{
    public class ShopItemScript : Behaviour
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
            var infoComp = infoGo.AddComponent<ShopItemInfoComponent>();
            price = item.Price;
            infoComp.FillInForm(ItemId, item.Price, stock, item.ImageName, item.Description);
            infoGo.IsEnabled = false;
        }

        protected override void Update()
        {
            var isOnCollision = CheckPlayerCollision();
            if (isOnCollision)
            {
                if (!wasOnCollision)
                {
                    OnCollisionEnter();
                }
                // check input handler
                if (BuyButtonPressed)
                {
                    Buy();
                }
            }
            else
            {
                if (wasOnCollision)
                {
                    OnCollisionExit();
                }
            }
        }

        private bool CheckPlayerCollision()
        {
            foreach (var item in PlayersManager.Instance.Players)
            {
                var playerGo = item.Value;
                Vector2 distance = this.Transform.Position - playerGo.Transform.Position;
                if (distance.Length() <= 1f)
                {
                    this.playerIndex = item.Key;
                    return true;
                }
            }
            return false;
        }

        private void OnCollisionEnter()
        {
            Debug.Print("oncollision enter");
            var playerGo = PlayersManager.Instance.GetPlayer(playerIndex);
            playerInput = playerGo.GetComponent<PlayerStates>().InputHandler;
            wasOnCollision = true;
            infoGo.IsEnabled = true;
        }

        private void OnCollisionExit()
        {
            Debug.Print("oncollision exit");
            wasOnCollision = false;
            infoGo.IsEnabled = false;
        }

        private void Buy()
        {
            if (stock > 0)
            {
                // TODO deduct players' coins check enough amount
                stock--;
                // TODO add item to player
                Debug.Print("Shop item: player" + playerIndex + " bought me");
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
                infoGo.GetComponent<ShopItemInfoComponent>().UpdateStock(stock);
            }
        }
    }
}
