using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SandPerSand
{
    public class CircleTriggerComp : Behaviour
    {
        public float Radius { get; set; } = 1f;
        public Dictionary<PlayerIndex, bool> WasOnCollision { get; private set; }
        public event EventHandler<GameObject> CollisionEnter;
        public event EventHandler<GameObject> CollisionExit;

        protected override void OnEnable()
        {
            base.OnEnable();
            WasOnCollision = new Dictionary<PlayerIndex, bool>();
            foreach (var key in PlayersManager.Instance.Players.Keys)
            {
                WasOnCollision.Add(key, false);
            }
        }

        protected bool CheckPlayerCollision(GameObject playerGo)
        {
            //PlayersManager.Instance.Players
            Vector2 distance = this.Transform.Position - playerGo.Transform.Position;
            Collider collider = playerGo.GetComponentInChildren<Collider>();
            if (distance.Length() <= Radius && collider!=null && collider.IsActive)
            {
                return true;
            }
            return false;
        }


        protected override void Update()
        {
            foreach (var item in PlayersManager.Instance.Players)
            {
                var playerIndex = item.Key;
                var playerGo = item.Value;
                var isOnCollision = CheckPlayerCollision(playerGo);

                if (!WasOnCollision.ContainsKey(playerIndex))
                {
                    WasOnCollision.Add(playerIndex, false);
                }
                if (isOnCollision)
                {
                    if (!WasOnCollision[playerIndex])
                    {
                        Debug.Print("oncollision enter - "+ playerIndex);
                        OnCollisionEnter(playerGo);
                        WasOnCollision[playerIndex] = true;
                    }
                }
                else
                {
                    if (WasOnCollision[playerIndex])
                    {
                        Debug.Print("oncollision exit - " + playerIndex);
                        OnCollisionExit(playerGo);
                        WasOnCollision[playerIndex] = false;
                    }
                }

            }
        }

        private void OnCollisionEnter(GameObject playerGo)
        {
            this.CollisionEnter?.Invoke(this, playerGo);
        }

        private void OnCollisionExit(GameObject playerGo)
        {
            this.CollisionExit?.Invoke(this, playerGo);
        }

    }
}
