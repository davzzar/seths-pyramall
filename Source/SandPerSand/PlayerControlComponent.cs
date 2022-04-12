using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Ray = Engine.Ray;

namespace SandPerSand
{
    internal class PlayerControlComponent : Behaviour
    {
        /// <summary>
        /// This component is responsible for player control.
        /// </summary>
        private InputHandler inputHandler;
        private RigidBody playerRB;
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

            this.Owner.Layer = 1;
        }

        protected override void Update()
        {
            DrawInputControls();
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

        /// <summary>
        /// Draws the current right analogue stick and jump button inputs next to player
        /// for debuging purposes.
        /// </summary>
        private void DrawInputControls()
        {
            var pos = this.Transform.Position;
            var stickLineOrigin = pos + (-Vector2.UnitX + Vector2.UnitY);

            var stickDir = inputHandler.getLeftThumbstickDir(magnitudeThreshold: 0f);
            Gizmos.DrawRect(stickLineOrigin, 0.5f * Vector2.One, Color.Black);
            Gizmos.DrawLine(stickLineOrigin, stickLineOrigin + stickDir, Color.Black);

            
            var jumpButtonState = inputHandler.getButtonState(Buttons.A);
            Color jumpIndicatorColor;
            switch (jumpButtonState)
            {
                case ButtonState.Pressed:
                    jumpIndicatorColor = Color.Yellow;
                    break;
                case ButtonState.Held:
                    jumpIndicatorColor = Color.Red;
                    break;
                case ButtonState.Released:
                    jumpIndicatorColor = Color.Magenta;
                    break;
                default:
                    jumpIndicatorColor = Color.Black;
                    break;
            }
            var jumpButtonOrigin = pos + Vector2.UnitY;
            Gizmos.DrawRect(jumpButtonOrigin, 0.5f * Vector2.One, jumpIndicatorColor);

        }
    }
}
