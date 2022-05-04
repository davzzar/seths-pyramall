using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        private string itemId;

        private bool BuyButtonPressed
        {
            get
            {
                if (playerInput == null) return false;
                return playerInput.getButtonState(Buttons.X) == ButtonState.Pressed;
            }
        }

        private SpriteRenderer infoRenderer;

        protected override void OnAwake()
        {
            base.OnAwake();
            wasOnCollision = false;
            infoGo = null;
            playerInput = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
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
                    //play animation

                    //add item to player
                    Debug.Print("Shop item: player"+ playerIndex + " bought me");
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

        private void ToggleInfo()
        {
            if (infoGo == null)
            {
                //create item info go
                infoGo = new GameObject();
                infoGo.Transform.Parent = this.Transform;
                // TODO hard code
                infoGo.Transform.LocalPosition = new Vector2(3, 4);
                infoGo.Transform.LossyScale = new Vector2(5, 8);
                infoRenderer = infoGo.AddComponent<SpriteRenderer>();
                // TODO hard code
                infoRenderer.LoadFromContent("shop/iteminfo");


                // TODO add item info animator
                // TODO add item sprite
                // TODO update stock
                // TODO add sold sprite
            }
            else
            {
                infoGo.Destroy();
                infoGo = null;
            }

        }
    }
}
