using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;

namespace SandPerSand
{
    internal class ItemManager : Behaviour
    {
        public PlayerIndex playerIndex { get; set; }
        public InputHandler inputHandler { get; set; }

        protected override void Update()
        {
            base.Update();

            string itemId;

            if(inputHandler.getButtonState(Microsoft.Xna.Framework.Input.Buttons.X) == ButtonState.Pressed)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, false);
            }

            if (inputHandler.getButtonState(Microsoft.Xna.Framework.Input.Buttons.Y) == ButtonState.Pressed)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, true);
            }
        }
    }
}
