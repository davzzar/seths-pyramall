using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ray = Engine.Ray;

namespace SandPerSand
{
    internal class PlayerControlComponent : Behaviour
    {
        /// <summary>
        /// This component is responsible for interpreting GamePad input and updating the player accordingly.
        /// </summary>
        private InputHandler inputHandler;

        private RigidBody playerRB;
        private Vector2[] dubugPlayerColliderOutline;

        private const float JumpForce = 10.0f;
        //private const float WalkForce = 20.0f;

        public PlayerIndex PlayerIndex
        {
            get;
            set;
        }


        public PlayerControlComponent()
        {
            /*Empty component constructor*/
        }

        protected override void OnEnable()
        {
            inputHandler = new InputHandler(this.PlayerIndex);
            playerRB = this.Owner.GetComponent<RigidBody>();
            dubugPlayerColliderOutline = this.Owner.GetComponent<PolygonCollider>().Outline;

            this.Owner.Layer = 1;
        }

        protected override void Update()
        {
            //impulse up if A is pressed
            switch (inputHandler.getButtonState(Buttons.A))
            {
                case ButtonState.Pressed:
                {
                    if (this.IsGrounded())
                    {
                        playerRB.ApplyLinearImpulse(Vector2.UnitY * JumpForce);
                    }

                    break;
                }
            }

            //force in the stick direction
            Vector2 stickDir = inputHandler.getLeftThumbstickDirX();
            //System.Diagnostics.Debug.Write($"Stick Dir X: {stickDir}\n");
            //playerRB.ApplyForce(stickDir * WalkForce * Time.DeltaTime);
            var velocity = this.playerRB.LinearVelocity;
            velocity.X = stickDir.X * 10f;
            this.playerRB.LinearVelocity = velocity;

            //var newPosition = this.Transform.Position + (stickDir * 0.2f);
            //this.Transform.Position = newPosition;

            // Update the input handler's state
            inputHandler.UpdateState();
        }

        private bool IsGrounded()
        {
            const int resolution = 8;

            var size = this.Transform.Scale;
            var pos0 = this.Transform.Position;
            pos0.X -= size.X / 2f;

            for (var i = 0; i < resolution; i++)
            {
                var origin = pos0 + Vector2.UnitX * (i / (float)(resolution - 1));
                var ray = new Ray(origin, -Vector2.UnitY);
                if (Physics.RayCast(ray, out var hit, size.Y / 2f + 0.1f, LayerMask.FromLayers(0)))
                {
                    Gizmos.DrawLine(origin, hit.Point, Color.Red);
                    return true;
                }
            }

            return false;
        }
    }
}
