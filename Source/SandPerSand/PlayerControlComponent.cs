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
        private const Buttons jumpButton = Buttons.A;
        private bool PressedJump => InputHandler.getButtonState(jumpButton) == ButtonState.Pressed;
        private float HorizontalDirection => InputHandler.getLeftThumbstickDirX(magnitudeThreshold: 0.1f);

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

        // We can only jump whem we are on the ground and we either just pressed jump or have it buffered,
        // OR if we pressed jump during coyote time.
        private bool CanJump =>
            isGrounded && (PressedJump || BufferedJump) || !isGrounded && PressedJump && CanUseCoyote;

        //Coyote Time
        private float timeOfLeavingGround; // the last time we were on the ground
        private float coyoteTimeBuffer = 0.1f;
        private bool coyoteEnabled;
        // we can use a coyote jump when its enables, we are midair, and within the coyote time buffer.
        private bool CanUseCoyote => coyoteEnabled && !isGrounded && timeOfLeavingGround + coyoteTimeBuffer > Time.GameTime;

        // Jump press buffer
        private float timeOfLastJumpPress;
        private float jumpPressBuffer = 0.25f;
        // we jump immediately once we are grounded and have a buffered jump
        private bool BufferedJump => timeOfLastJumpPress + jumpPressBuffer > Time.GameTime;
        


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

            if (PressedJump)
            {
                timeOfLastJumpPress = Time.GameTime;
            }

            // compute velocities
            ComputeGravityScale();
            ComputeHorizontalSpeed();
            ComputeVerticalSpeed();

            // do jump if conditions are met
            
            if (CanJump)
            {
                coyoteEnabled = false;
                timeOfLeavingGround = float.MinValue; // disable jump buffer, so that we don't get infinite jumps...


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
                      $"isGrounded: {isGrounded}\n" +
                      $"pressedJump: {PressedJump}\n" +
                      $"CanUseCoyote: {CanUseCoyote}\n" +
                      $"BufferedJump: {BufferedJump}\n\n" +
                      $"Pos: {this.Transform.Position}";
        }

        private void ComputeHorizontalSpeed()
        {
            // NOTE: Check assumes there is a dead zone on the stick input.
            if (HorizontalDirection != 0)
            {
                // use the appropriate acceleration
                currentAcceleration = isGrounded ? maxAcceleration : maxArialAcceleration;

                // Set horizontal move speed
                horizontalSpeed += HorizontalDirection * currentAcceleration * Time.DeltaTime;

                // clamped by max frame movement
                float absHorizontalDirection = MathF.Abs(HorizontalDirection);
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


        /// <summary>
        /// Draws the current right analogue stick and jump button inputs next to player
        /// for debuging purposes.
        /// </summary>
        private void DrawInputControls()
        {
            var pos = this.Transform.Position;
            var stickLineOrigin = pos + (-Vector2.UnitX + Vector2.UnitY);

            var stickDir = new Vector2(HorizontalDirection, InputHandler.getLeftThumbstickDirY(magnitudeThreshold: 0f));
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
