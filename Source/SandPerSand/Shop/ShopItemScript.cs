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
        private int stock = 10;
        private int price = 100;
        private string description;
        private string itemImg;
        private string itemId = null;
        public string ItemId
        {
            get => itemId;
            set
            {
                itemId = value;
                // TODO set stock, price, description accordingly, probably aquire from a ItemWiki class
                stock = 10;
                price = 100;
                description = "this is a dummy description";
                itemImg = "shop/item_unknown";
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
            infoGo = null;
            playerInput = null;
            if (itemId == null)
            {
                // FIXME
                ItemId = "unknown";
                //throw new InvalidOperationException();
            }
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
            wasOnCollision = true;
            var playerGo = PlayersManager.Instance.GetPlayer(playerIndex);
            playerInput = playerGo.GetComponent<PlayerStates>().InputHandler;
            Debug.Print("oncollision enter");
            ToggleInfo();
        }

        private void OnCollisionExit()
        {
            wasOnCollision = false;
            Debug.Print("oncollision exit");
            ToggleInfo();
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

        private void ToggleInfo()
        {
            if (infoGo == null)
            {
                //create item info go
                infoGo = new GameObject();
                infoGo.Transform.Parent = this.Transform;
                var infoComp = infoGo.AddComponent<ShopItemInfoComponent>();
                infoComp.FillInForm(itemId, price, stock, itemImg, description);
                // TODO add item info animator?
            }
            else
            {
                //infoGo.GetComponent<ShopItemInfoComponent>().DestoryChildren();
                infoGo.Destroy();
                infoGo = null;
            }
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
