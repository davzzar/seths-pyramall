using Engine;
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    class SimpleInputComonent : Behaviour
    {
        public PlayerIndex myPlayerIndex;
        private GamePadState prevState;
        public SimpleInputComonent()
        {
            // We don't do anything here
        }
        
        protected override void OnAwake() {
            GamePadState state = GamePad.GetState(myPlayerIndex);

            if (state.IsConnected == false)
            {
                throw new Exception("[TODD] Someone forgot to connect their controller ;)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[TODD] Controller connected.");
            }

            prevState = state;
        }

        protected override void Update() {
            GamePadState state = GamePad.GetState(myPlayerIndex);

            /* if (state.IsButtonDown(Buttons.A))
            {
                this.Transform.LossyScale *= 1.1f;
                this.Transform.Position *= 0.8f;
            }
            else if (state.IsButtonDown(Buttons.B))
            {
                this.Transform.LossyScale *= 0.8f;
                this.Transform.Position *= 1.1f;
            } */

            var newPosition = this.Transform.Position;

            if (state.ThumbSticks.Left.X < -0.5f)
                newPosition = new Vector2(newPosition.X - 0.2f, newPosition.Y);
            if (state.ThumbSticks.Left.X > 0.5f)
                newPosition = new Vector2(newPosition.X + 0.2f, newPosition.Y);
            if (state.ThumbSticks.Left.Y < -0.5f)
                newPosition = new Vector2(newPosition.X, newPosition.Y - 0.2f);
            if (state.ThumbSticks.Left.Y > 0.5f)
                newPosition = new Vector2(newPosition.X, newPosition.Y + 0.2f);

            this.Transform.Position = newPosition;
        }
    }
}
