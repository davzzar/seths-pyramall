using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;
using TiledCS;

namespace SandPerSand.Items
{
    internal class ItemManager : Behaviour
    {
        public InputHandler inputHandler { get; set; }

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void Update()
        {
            base.Update();

            //check if Items are triggered and add them to playerstate if needed

            string itemId = "";
            var players = PlayersManager.Instance.InGamePlayerGo();

            var playerIndex = inputHandler.PlayerIndex;

            bool rightPressed = (inputHandler.getButtonState(Buttons.RightShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.RightTrigger) == ButtonState.Pressed);
            bool leftPressed = (inputHandler.getButtonState(Buttons.LeftShoulder) == ButtonState.Pressed || inputHandler.getButtonState(Buttons.LeftTrigger) == ButtonState.Pressed);

            bool rightState = GameObject.FindComponent<RealGameStateManager>().CurrentGameState == GameState.InRound || GameObject.FindComponent<RealGameStateManager>().CurrentGameState == GameState.CountDown;

            bool playerInGame = players.Contains(this.Owner);

            if (rightPressed && rightState && playerInGame)
            {
                itemId = PlayersManager.Instance.UseItem(playerIndex, true);
            }

            if (leftPressed && rightState && playerInGame)
            {
                itemId = PlayersManager.Instance.UseItem(playerIndex, false);
            }

            switch (itemId)
            {
                case "portable_sand_source":
                    SandSimulation sandSim = GameObject.FindComponent<SandSimulation>();
                    Vector2 pos = this.Transform.Position;
                    sandSim.AddSandSource(new Aabb(pos.X, pos.Y, 32 / 64f, 32 / 64f));
                    break;
                case "position_swap":
                    float y = 0;
                    int i = 0;
                    GameObject firstPlayer = null;
                    foreach (GameObject p in players)
                    {
                        if (p.Transform.Position.Y > y && p != this.Owner)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer = p;
                        }
                        i++;
                    }
                    if(firstPlayer!= null)
                    {
                        float dist = (firstPlayer.Transform.Position - this.Owner.Transform.Position).Length();
                        Vector2 velthis = this.Owner.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity;
                        Vector2 velfirst = firstPlayer.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity;
                        this.Owner.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity = Vector2.Zero;
                        firstPlayer.GetComponent<PlayerControlComponent>().rigidBody.LinearVelocity = Vector2.Zero;
                        firstPlayer.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.PositionSwapItem(ItemId.position_swap, this.Transform.Position, 0f, 0f, true, velfirst, firstPlayer.Transform.Position, dist));
                        this.Owner.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.PositionSwapItem(ItemId.position_swap, firstPlayer.Transform.Position, 0f, 0f, true, velthis, this.Transform.Position, dist));
                    }
                    break;
                case "lightning":
                    foreach (var p in players)
                    {
                        if (p != Owner && p.Transform.Position.Y > Transform.Position.Y)
                        {
                            var dist = p.Transform.Position.Y - this.Transform.Position.Y;
                            var duration = Math.Min(10, Math.Max(3, (int)(dist / 3)));
                            p.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.lightning, this.Transform.Position, duration, duration, true));
                        }
                    }
                    break;
                case "magnet":
                    break;
                case "ice_block":
                    float y2 = 0;
                    int i2 = 0;
                    GameObject firstPlayer2 = null;
                    foreach (GameObject p in players)
                    {
                        if (p.Transform.Position.Y > y2 && p != this.Owner)
                        {
                            y = p.Transform.Position.Y;
                            firstPlayer2 = p;
                        }
                        i2++;
                    }
                    if(firstPlayer2 != null)
                    {
                        firstPlayer2.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.ice_block, this.Transform.Position, 3f, 3f, true));
                    }
                    break;
                case "sunglasses":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.sunglasses, Vector2.Zero, float.NaN, float.NaN, false));
                    break;
                case "wings":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.wings, Vector2.Zero, 5f, 5f, false));
                    break;
                case "speedup":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.speedup, Vector2.Zero, 10f, 10f, false));
                    break;
                case "dizzy_eyes":
                    foreach (GameObject p in players)
                    {
                        if (p != this.Owner)
                        {
                            p.GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.dizzy_eyes, this.Transform.Position, 5f, 5f, true));
                        }
                    }
                    break;
                case "shield":
                    PlayersManager.Instance.Players[playerIndex].GetComponentInChildren<PlayerStates>().AddActiveItem(new Items.Item(ItemId.shield, Vector2.Zero, 10f, 10f, false));
                    break;
            }

        }
    }
}
