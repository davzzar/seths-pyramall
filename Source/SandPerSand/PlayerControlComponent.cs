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
        private bool desiredJump = false;
        private const Buttons jumpButton = Buttons.A;

        // Horizontal movement
        private float horizontalSpeed;
        private float currentAcceleration;
        private const float maxAcceleration = 110f;
        private const float maxArialAcceleration = 30f;
        private const float maxDeceleration = 60f;
        private const float maxHorizontalSpeed = 13f;

        // Vertical movement
        private float verticalSpeed;
        private const float jumpHeight = 10f; // explicit jump height       
        private const float maxFallingSpeed = -20f;
        private const float gravity = -9.8f; // a la Unity
        private const float climbGravityMultiplier = 1.7f;
        private const float fallGravityMultiplier = 6f;
        private const float defaultGravityMultiplier = 1f;
        private float gravityScale = defaultGravityMultiplier;

        //Coyote Time
        private float timeOfLeavingGround; // the last time we were on the ground
        private float coyoteTimeBuffer = 0.1f;
        private bool coyoteEnabled;
        // we can use a coyote jump when its enables, we are midair, and within the coyote time buffer.
        private bool canUseCoyote => coyoteEnabled && !isGrounded && timeOfLeavingGround + coyoteTimeBuffer > Time.GameTime;


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

            // get current velocity, and update local mirrors
            velocity = rigidBody.LinearVelocity;
            horizontalSpeed = velocity.X;
            verticalSpeed = velocity.Y;

            // update grounded state for seeing if we landed or not
            wasGrounded = isGrounded;
            isGrounded = groundChecker.IsGrounded();
            
            // update the time of leaving ground if we left ground this frame
            // if we just landed, we re-enable coyote time
            if (wasGrounded && !isGrounded) timeOfLeavingGround = Time.GameTime;
            else if (!wasGrounded && isGrounded) coyoteEnabled = true;
            


            // gather this frame's inputs
            horizontalDirection = InputHandler.getLeftThumbstickDirX(magnitudeThreshold:0.1f);
            // TODO abstract concrete button and state away.
            /// We |= the desired jump to flip it one once the jump button is pressed.
            /// We will manually reset this to false once a jump was executed.
            desiredJump |= InputHandler.getButtonState(jumpButton) == ButtonState.Pressed;

            // compute velocities
            ComputeGravityScale();
            ComputeHorizontalSpeed();
            ComputeVerticalSpeed();

            // do jump if we pressed jump button within the frame
            // TODO add jump buffer / jump delay of c. 0.25 seconds
            if (desiredJump)
            {
                desiredJump = false;
                
                PerformJump();
            }

            // Apply computed velocity
            rigidBody.LinearVelocity = new Vector2(horizontalSpeed, verticalSpeed);

            // Update the input handler's state
            InputHandler.UpdateState();
        }

        private void ComputeGravityScale()
        {
            if (isGrounded) gravityScale = defaultGravityMultiplier;

            else if (verticalSpeed < 0) gravityScale = fallGravityMultiplier;
            
            else if (verticalSpeed > 0) gravityScale = climbGravityMultiplier;
            
            // when we are at rest gravity still acts on us.
            // We may not want this due to friction or whatever, but I'll leave it for now.
            else if (verticalSpeed == 0) gravityScale = defaultGravityMultiplier;
            
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
            tr.Text = $"H. Vel.: {horizontalSpeed:F3}, V. Vel.: {verticalSpeed:F3}\n" +
                      $"H. Accel: {currentAcceleration}, Gravity Scale: {gravityScale}\n" +
                      $"isGrounded: {isGrounded}, CanUseCoyote: {canUseCoyote}\n\n" +
                      $"Pos: {this.Transform.Position}";
        }

        private void ComputeHorizontalSpeed()
        {
            // NOTE: Check assumes there is a dead zone on the stick input.
            if (horizontalDirection != 0)
            {
                // use the appropriate acceleration
                currentAcceleration = isGrounded ? maxAcceleration : maxArialAcceleration;

                // Set horizontal move speed
                horizontalSpeed += horizontalDirection * currentAcceleration * Time.DeltaTime;

                // clamped by max frame movement
                float absHorizontalDirection = MathF.Abs(horizontalDirection);
                horizontalSpeed = MathHelper.Clamp(horizontalSpeed, -maxHorizontalSpeed, maxHorizontalSpeed);

                //TODO Add jump apex bonus speed
                //System.Diagnostics.Debug.WriteLine($"Accelerating: {horizontalSpeed}");
            }
            else
            {
                //float decelerationMultiplier = isGrounded ? 1f : 1f;
                
                // Decelerate the player
                horizontalSpeed = MathUtils.MoveTowards(horizontalSpeed, 0, maxDeceleration * Time.DeltaTime);
               
            }

            // horizontal collisions with rigid body should set horizontal velocity to zero automatically.
        }

        /// <summary>
        /// Compute the current vertical speed taking (potential) jumping and custom gravity into account.
        /// </summary>
        private void ComputeVerticalSpeed()
        {
            // compute acceleration due to gravity if we are not on ground
            //if (isGrounded)
            //{
            //    if (verticalSpeed < 0) verticalSpeed = 0;
            //}
            //else
            //if(!isGrounded)
            {   
                verticalSpeed +=  gravity * gravityScale * Time.DeltaTime;
            }

            // clamp to terminal velocity
            if (verticalSpeed < maxFallingSpeed) verticalSpeed = maxFallingSpeed; 
        }

        /// <summary>
        /// Trigger on jump input. Compute and add jump speed to current vertical velocity.
        /// </summary>
        private void PerformJump()
        {
            if (isGrounded || canUseCoyote) // would add multi-jump check here
            {
                // disable coyote (until next time we land, see above)
                coyoteEnabled = false;

                // reset vertical speed so we don't get shitty "half-jumps" or velocity negated jumps
                verticalSpeed = 0;

                // get the jumping speed via sqrt(-2gh) from high school
                float jumpSpeed = MathF.Sqrt(-2f * gravity * jumpHeight); 
                
                if (verticalSpeed > 0f) // if we're moving up
                {
                    // CAREFUL what are we assuming about verticalSpeed? Is it always +ve?
                    jumpSpeed = MathF.Max(jumpSpeed - verticalSpeed, 0f);
                }

                verticalSpeed += jumpSpeed;
            }
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
