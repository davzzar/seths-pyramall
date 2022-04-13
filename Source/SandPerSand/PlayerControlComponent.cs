using System;
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
        public PlayerIndex PlayerIndex
        {
            get;
            set;
        }

        public InputHandler InputHandler { get; set;  }

        private RigidBody rigidBody;
        private GroundCheckComponent groundChecker;
        private bool isGrounded, wasGrounded;

        private Vector2 velocity;

        // Input
        private float horizontalDirection;
        private bool desiredJump;

        // Horizontal movement
        private float currentHorizontalSpeed;
        private const float maxAcceleration = 110f;
        private const float maxDeceleration = 60f;
        private const float maxHorizontalSpeed = 13f;

        // Vertical movement
        private float currentVerticalSpeed;
        private const float jumpHeight = 3f; // explicit jump height
        private const float maxFallingSpeed = -20f;
        private const float maxArialAcceleration = 70f;
        private const float gravity = -9.8f; // a la Unity
        private const float climbGravityMultiplier = 1.7f;
        private const float fallGravityMultiplier = 3f;
        private const float defaultGravityMultiplier = 1f;
        private float gravityScale = defaultGravityMultiplier;

        public PlayerControlComponent()
        {
            /*Empty component constructor*/
        }

        protected override void OnAwake()
        {
            rigidBody = this.Owner.GetComponent<RigidBody>();
            rigidBody.IgnoreGravity = true;
            groundChecker = this.Owner.GetComponent<GroundCheckComponent>();

            this.Owner.Layer = 1;
        }

        protected override void Update()
        {
            ShowDebug();

            // update grounded state for seeing if we landed or not
            wasGrounded = isGrounded;
            isGrounded = groundChecker.IsGrounded();
                
            // get current velocity
            velocity = rigidBody.LinearVelocity;

            // gather this frame's inputs
            horizontalDirection = InputHandler.getLeftThumbstickDirX(magnitudeThreshold:0.1f);
            // TODO abstract concrete button and state away.
            /// We |= the desired jump to flip it one once the jump button is pressed.
            /// We will manually reset this to false once a jump was executed.
            desiredJump |= InputHandler.getButtonState(Buttons.A) == ButtonState.Pressed;

            // compute velocities
            ComputeGravityScale();
            ComputeHorizontalSpeed();
            ComputeVerticalSpeed();
            ApplyVelocity();

            // Update the input handler's state
            InputHandler.UpdateState();
        }

        private void ComputeGravityScale()
        {
            if (currentVerticalSpeed < 0)
            {
                gravityScale = fallGravityMultiplier;
            }

            if (currentHorizontalSpeed > 0)
            {
                gravityScale = climbGravityMultiplier;
            }

            // when we are at rest gravity still acts on us.
            // We may not want this due to friction or whatever, but I'll leave it for now.
            gravityScale = defaultGravityMultiplier;
        }

        private void ShowDebug()
        {
            DrawInputControls();

            var textRenderer = this.Owner.GetComponent<GuiTextRenderer>();
            if (textRenderer != null)
            {
                RenderDebugText(textRenderer);
            }
        }

        private void RenderDebugText(in GuiTextRenderer tr)
        {
            tr.Text = $"H. Vel.: {currentHorizontalSpeed:F3}, V. Vel.: {currentVerticalSpeed:F3}";
        }

        private void ComputeHorizontalSpeed()
        {
            currentHorizontalSpeed = velocity.X;


            // NOTE: Check assumes there is a dead zone on the stick input.
            if (horizontalDirection != 0)
            {
                // use the appropriate acceleration
                float acceleration = isGrounded ? maxAcceleration : maxArialAcceleration;

                // Set horizontal move speed
                currentHorizontalSpeed += horizontalDirection * acceleration * Time.DeltaTime;

                // clamped by max frame movement
                currentHorizontalSpeed = MathHelper.Clamp(currentHorizontalSpeed, -maxHorizontalSpeed, maxHorizontalSpeed);

                //TODO Add jump apex bonus speed
                //System.Diagnostics.Debug.WriteLine($"Accelerating: {currentHorizontalSpeed}");
            }
            else
            {
                // Decelerate the player
                currentHorizontalSpeed = MathUtils.MoveTowards(currentHorizontalSpeed, 0, maxDeceleration * Time.DeltaTime);
                //System.Diagnostics.Debug.WriteLine($"Decelerating: {currentHorizontalSpeed}");
            }

            // horizontal collisions with rigid body should set horizontal velocity to zero automatically.
        }

        /// <summary>
        /// Compute the current vertical speed taking (potential) jumping and custom gravity into account.
        /// </summary>
        private void ComputeVerticalSpeed()
        {
            
            // do jump if we pressed jump button within the frame
            // TODO add jump buffer / jump delay of c. 0.25 seconds
            if (desiredJump)
            {
                desiredJump = false;
                PerformJump();
            }

            // compute acceleration due to gravity

            currentVerticalSpeed +=  gravity * gravityScale * Time.DeltaTime;

            // clamp to terminal velocity
            if (currentVerticalSpeed < maxFallingSpeed)
            {
                currentVerticalSpeed = maxFallingSpeed;
            }
        }

        /// <summary>
        /// Trigger on jump input. Compute and add jump speed to current vertical velocity.
        /// </summary>
        private void PerformJump()
        {
            currentVerticalSpeed = velocity.Y;

            if (isGrounded) // would add multi-jump check here
            {
                float jumpSpeed = MathF.Sqrt(-2f * gravity * jumpHeight); // sqrt(-2gh) from high school 
                
                if (currentHorizontalSpeed > 0f) // if we're moving up
                {

                    // CAREFUL what are we assuming about currentVerticalSpeed? Is it always +ve?
                    jumpSpeed = MathF.Max(jumpSpeed - currentVerticalSpeed, 0f);
                }

                currentVerticalSpeed += jumpSpeed;
            }

            
        }

        /// <summary>
        /// Apply updated velocities to the player.
        /// </summary>
        private void ApplyVelocity()
        {
            var currentVelocity = rigidBody.LinearVelocity;
            currentVelocity.X = currentHorizontalSpeed;
            currentVelocity.Y = currentVerticalSpeed;
            
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

            var stickDir = new Vector2(horizontalDirection, InputHandler.getLeftThumbstickDirY(magnitudeThreshold: 0f));
            Gizmos.DrawRect(stickLineOrigin, 0.5f * Vector2.One, Color.Black);
            Gizmos.DrawLine(stickLineOrigin, stickLineOrigin + stickDir, Color.Black);

            
            var jumpButtonState = InputHandler.getButtonState(Buttons.A);
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
