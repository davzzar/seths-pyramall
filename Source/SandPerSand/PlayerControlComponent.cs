using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    internal class PlayerControlComponent : Behaviour
    {
        /// <summary>
        /// This component is responsible for player control.
        /// </summary>
        private InputHandler inputHandler;
        private RigidBody rigidBody;
        private GroundCheckComponent groundChecker;

        private Vector2 velocity;

        private float horizontalDirection;
        private float currentHorizontalSpeed;

        private const float acceleration = 110f;
        private const float deceleration = 60f;
        private const float maxHorizontalSpeed = 13f;

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
            rigidBody = this.Owner.GetComponent<RigidBody>();
            groundChecker = this.Owner.GetComponent<GroundCheckComponent>();

            this.Owner.Layer = 1;
        }

        protected override void Update()
        {
            DrawInputControls();

            // get current velocity
            velocity = rigidBody.LinearVelocity;

            horizontalDirection = inputHandler.getLeftThumbstickDirX(magnitudeThreshold:0.1f);
            
            computeHorrizontalSpeed();
            applyVelocity();

            // Update the input handler's state
            inputHandler.UpdateState();
        }

        protected void computeHorrizontalSpeed()
        {
            currentHorizontalSpeed = velocity.X;

            // NOTE: Check assumes there is a dead zone on the stick input.
            if (horizontalDirection != 0)
            {
                // Set horizontal move speed
                currentHorizontalSpeed += horizontalDirection * acceleration * Time.DeltaTime;

                // clamped by max frame movement
                currentHorizontalSpeed = MathHelper.Clamp(currentHorizontalSpeed, -maxHorizontalSpeed, maxHorizontalSpeed);

                //TODO Add jump apex bonus speed
                System.Diagnostics.Debug.WriteLine($"Accelerating: {currentHorizontalSpeed}");
            }
            else
            {
                // Decelerate the player
                currentHorizontalSpeed = MathUtils.MoveTowards(currentHorizontalSpeed, 0, deceleration * Time.DeltaTime);
                System.Diagnostics.Debug.WriteLine($"Decelerating: {currentHorizontalSpeed}");
            }

            // horizontal collisions with rigid body should set horizontal velocity to zero automatically.
        }

        /// <summary>
        /// Apply updated velocities to the player.
        /// </summary>
        private void applyVelocity()
        {
            var currentVelocity = rigidBody.LinearVelocity;
            currentVelocity.X = currentHorizontalSpeed;
            
            rigidBody.LinearVelocity = currentVelocity;
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
