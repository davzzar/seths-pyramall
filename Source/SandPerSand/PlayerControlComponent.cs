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
        private GroundCheckComponent groundChecker;

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

        protected override void OnAwake()
        {
            inputHandler = new InputHandler(this.PlayerIndex);
            playerRB = this.Owner.GetComponent<RigidBody>();
            groundChecker = this.Owner.GetComponent<GroundCheckComponent>();
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
                    if (groundChecker.IsGrounded())
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
    }
}
