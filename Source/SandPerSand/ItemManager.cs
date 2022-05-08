using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using SandPerSand.SandSim;

namespace SandPerSand
{
    internal class ItemManager : Behaviour
    {
        public PlayerIndex playerIndex { get; set; }
        public InputHandler inputHandler { get; set; }

        protected override void Update()
        {
            base.Update();

            string itemId = "";

            if(inputHandler.getButtonState(Microsoft.Xna.Framework.Input.Buttons.X) == ButtonState.Pressed)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, false);
            }

            if (inputHandler.getButtonState(Microsoft.Xna.Framework.Input.Buttons.Y) == ButtonState.Pressed)
            {
                itemId = PlayersManager.Instance.useItem(playerIndex, true);
            }

            switch (itemId)
            {
                case "portable_sand_source":
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    Vector2 pos = this.Transform.Position;
                    sandSim.AddSandSource(new Aabb(pos.X, pos.Y, 32 / 64f, 32 / 64f));
                    break;
                case "position_swap":
                    PlayerComponent[] players = GameObject.FindComponents<PlayerComponent>();
                    float y = 0;
                    int i = 0;
                    int firstPlayer = 0;
                    foreach (PlayerComponent p in players)
                    {
                        if(p.Transform.Position.Y > y)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer = i;
                        }
                        i++;
                    }
                    Vector2 postmp = players[firstPlayer].Transform.Position;
                    players[firstPlayer].Transform.Position = this.Transform.Position;
                    this.Transform.Position = postmp;
                    break;
                case "lightning":
                    break;
                case "magnet":
                    break;
                case "ice_block":
                    PlayerComponent[] players2 = GameObject.FindComponents<PlayerComponent>();
                    float y2 = 0;
                    int i2 = 0;
                    PlayerIndex firstPlayer2 = 0;
                    foreach (PlayerComponent p in players2)
                    {
                        if (p.Transform.Position.Y > y2)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer2 = p.PlayerIndex;
                        }
                        i2++;
                    }
                    PlayersManager.Instance.Players[firstPlayer2].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 3));
                    break;
                case "sunglasses":
                    break;
                case "wings":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10));
                    break;
                case "speedup":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10));
                    break;
                case "dizzy_eyes":
                    break;
                case "shield":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().activeItems.Add((itemId, 10));
                    break;
            }
        }
    }
}
