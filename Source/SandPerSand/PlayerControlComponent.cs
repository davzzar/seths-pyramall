using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    internal class PlayerControlComponent : Behaviour
    {
        public InputHandler InputHandler { get; set; }

        private RigidBody rigidBody;
        private GroundCheckComponent groundChecker;
        private GuiTextRenderer textRenderer;

        // Input
        private const Buttons JumpButton = Buttons.A;
        private bool JumpButtonPressed => InputHandler.getButtonState(JumpButton) == ButtonState.Pressed;
        private bool JumpButtonUp => InputHandler.getButtonState(JumpButton) == ButtonState.Up;
        private float HorizontalDirection => InputHandler.getLeftThumbstickDirX(magnitudeThreshold: 0.1f);

        // State
        private bool isGrounded, wasGrounded;

        // Horizontal movement
        private float horizontalSpeed;
        private float currentAcceleration;
        private const float MaxAcceleration = 110f;
        private const float MaxArialAcceleration = 30f;
        private const float MaxDeceleration = 60f;
        private const float MaxHorizontalSpeed = 13f;

        // Vertical movement
        private float verticalSpeed;
        private const float JumpHeight = 5f; // explicit jump height       
        private const float MaxFallingSpeed = -20f;
        
        // Player Gravity and Variable Jump Height
        private const float JumpVelocityFalloff = 3f;
        private const float Gravity = -9.8f; // a la Unity
        private const float ClimbGravityMultiplier = 1.7f;
        private const float FallGravityMultiplier = 6f;
        private const float DefaultGravityMultiplier = 1f;
        private float gravityScale = DefaultGravityMultiplier;
        private bool jumpEnded = true;

        //Coyote Time
        private const float CoyoteTimeBuffer = 0.1f;
        private bool coyoteEnabled;
        private float timeOfLeavingGround; // the last time we were on the ground
        // we can use a coyote jump when its enables, we are midair, and within the coyote time buffer.
        private bool CanUseCoyote => coyoteEnabled && !isGrounded && timeOfLeavingGround + CoyoteTimeBuffer > Time.GameTime;

        // Jump press buffer
        private const float JumpPressBuffer = 0.25f;
        private float timeOfLastAirborneJumpPress;
        // we jump immediately once we are grounded and have a buffered jump
        private bool BufferedJump => timeOfLastAirborneJumpPress + JumpPressBuffer > Time.GameTime;
        private bool HasLaunched => wasGrounded && !isGrounded;
        private bool HasLanded => !wasGrounded && isGrounded;

        // We can only jump when we are on the ground and we either just pressed jump or have it buffered,
        // OR if we pressed jump during coyote time.
        private bool WillJump =>
            isGrounded && JumpButtonPressed || HasLanded && BufferedJump || !isGrounded && JumpButtonPressed && CanUseCoyote;


        public PlayerControlComponent()
        {
            /*Empty component constructor*/
        }

        protected override void OnAwake()
        {

            // add needed components
            if (InputHandler == null)
            {
                throw new MemberAccessException("No InputHandler attached to this player.");  
            }
            
            rigidBody = this.Owner.GetOrAddComponent<RigidBody>();
            rigidBody.IgnoreGravity = true;
            groundChecker = this.Owner.GetOrAddComponent<GroundCheckComponent>();

            textRenderer = this.Owner.GetOrAddComponent<GuiTextRenderer>();

            this.Owner.Layer = 1;
        }

        protected override void Update()
        {
            // Update State

            // get current velocity, and update local mirrors
            var (x, y) = rigidBody.LinearVelocity;
            horizontalSpeed = x;
            verticalSpeed = y;

            // update grounded state for seeing if we landed or not
            wasGrounded = isGrounded;
            isGrounded = groundChecker.IsGrounded();
            
            // update the time of leaving ground if we left ground this frame
            // if we just landed, we re-enable coyote time
            if (HasLaunched) timeOfLeavingGround = Time.GameTime;
            else if (HasLanded) coyoteEnabled = true;

            if (JumpButtonPressed)
            {
                if (!isGrounded) { timeOfLastAirborneJumpPress = Time.GameTime; }
            }

            // Compute velocities
            ComputeGravityScale();
            ComputeHorizontalSpeed();
            ComputeVerticalSpeed();

            if (WillJump)
            {
                coyoteEnabled = false;
                jumpEnded = false;
                timeOfLeavingGround = float.MinValue; // disable jump buffer, so that we don't get infinite jumps...

                PerformJump();
            }

            // if jump is released while mid-air and we're moving up
            // (since now we only move up while jumping, will need to explicitly model a jump later)
            if (JumpButtonUp && !isGrounded && !jumpEnded && verticalSpeed > 0) jumpEnded = true;

            // Apply computed velocity
            rigidBody.LinearVelocity = new Vector2(horizontalSpeed, verticalSpeed);

            // Update the input handler's state
            InputHandler.UpdateState();
            
            ShowDebug();
        }

        /// <summary>
        /// Compute the gravity strength to apply to player in the current portion of its movement.
        /// </summary>
        private void ComputeGravityScale()
        {
            if (isGrounded) gravityScale = DefaultGravityMultiplier;

            else if (verticalSpeed < JumpVelocityFalloff) gravityScale = FallGravityMultiplier;
            
            else if (verticalSpeed > 0)
            {
                gravityScale = jumpEnded ? FallGravityMultiplier : ClimbGravityMultiplier;
            }
            // when we are at rest gravity still acts on us.
            // We may not want this due to friction or whatever, but I'll leave it for now.
            else if (verticalSpeed == 0) gravityScale = DefaultGravityMultiplier;
        }

        private void ComputeHorizontalSpeed()
        {
            // NOTE: Check assumes there is a dead zone on the stick input.
            if (HorizontalDirection != 0)
            {
                // use the appropriate acceleration
                currentAcceleration = isGrounded ? MaxAcceleration : MaxArialAcceleration;

                // Set horizontal move speed
                horizontalSpeed += HorizontalDirection * currentAcceleration * Time.DeltaTime;

                // clamped by max frame movement
                horizontalSpeed = MathHelper.Clamp(horizontalSpeed, -MaxHorizontalSpeed, MaxHorizontalSpeed);
            }
            else
            {
                // Decelerate the player
                horizontalSpeed = MathUtils.MoveTowards(horizontalSpeed, 0, MaxDeceleration * Time.DeltaTime);
            }
        }

        /// <summary>
        /// Compute the current vertical speed taking (potential) jumping and custom gravity into account.
        /// </summary>
        private void ComputeVerticalSpeed()
        {
            verticalSpeed +=  Gravity * gravityScale * Time.DeltaTime;

            // clamp to terminal velocity
            if (verticalSpeed < MaxFallingSpeed) verticalSpeed = MaxFallingSpeed; 
        }


        /// <summary>
        /// Trigger on jump input. Compute and add jump speed to current vertical velocity.
        /// </summary>
        private void PerformJump()
        {
            // reset vertical speed so we don't get shitty "half-jumps" or velocity negated jumps
            verticalSpeed = 0;

            // get the jumping speed via sqrt(-2gh) from high school
            var jumpSpeed = MathF.Sqrt(-2f * Gravity * JumpHeight); 
            
            if (verticalSpeed > 0f) // if we're moving up
            {
                // CAREFUL what are we assuming about verticalSpeed? Is it always +ve?
                jumpSpeed = MathF.Max(jumpSpeed - verticalSpeed, 0f);
            }

            verticalSpeed += jumpSpeed;
        }


        /// <summary>
        /// Show debug information for development.
        /// </summary>
        private void ShowDebug()
        {
            DrawInputControls();

            if (textRenderer != null)
            {
                RenderDebugText(textRenderer);
            }
        }

        /// <summary>
        /// Render debug test
        /// </summary>
        /// <param name="tr"></param>
        private void RenderDebugText(in GuiTextRenderer tr)
        {
            tr.Text = $"H. Vel.: {horizontalSpeed:F3}, V. Vel.: {verticalSpeed:F3}\n" +
                      $"H. Accel: {currentAcceleration}, Gravity Scale: {gravityScale}\n" +
                      $"isGrounded: {isGrounded}\n" +
                      $"jumpEnded: {jumpEnded}\n" +
                      $"CanUseCoyote: {CanUseCoyote}\n" +
                      $"BufferedJump: {BufferedJump}\n\n" +
                      $"Pos: {this.Transform.Position}";
        }

        /// <summary>
        /// Draws the current right analogue stick and jump button inputs next to player
        /// for debugging purposes.
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
