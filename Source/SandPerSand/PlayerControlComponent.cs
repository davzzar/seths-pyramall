using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{
    internal class PlayerControlComponent : Behaviour
    {
        [Browsable(false)]
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
        public bool IsGrounded { get; private set; }
        public bool WasGrounded { get; private set; }
        public bool HasLaunched => WasGrounded && !IsGrounded;
        public bool HasLanded => !WasGrounded && IsGrounded;
        public bool IsOnSlope => groundChecker.IsOnSlope;


        // Horizontal movement
        public float HorizontalSpeed { get; private set; }

        public float CurrentAcceleration { get; private set; }

        public const float MaxAcceleration = 110f;

        public const float MaxArialAcceleration = 30f;

        public const float MaxDeceleration = 60f;

        public const float MaxHorizontalSpeed = 13f;

        // Movement on Slopes
        public float SlopeClimbVerticalSpeed { get; private set; }
        public float SlopeAngle => groundChecker.SlopeAngleDown;
        public bool IsAscendingSlope => IsOnSlope && HorizontalSpeed != 0 && !(MathF.Sign(HorizontalSpeed) == 1 ^ MathF.Sign(SlopeAngle) == 1);

        // Vertical movement
        public float VerticalSpeed { get; private set; }

        public const float JumpHeight = 8f; // explicit jump height

        public const float MaxFallingSpeed = -20f;

        // Player Gravity and Variable Jump Height
        public const float JumpVelocityFalloff = 3f;

        public const float Gravity = -9.8f; // a la Unity

        public const float ClimbGravityMultiplier = 1.7f;

        public const float FallGravityMultiplier = 6f;

        private const float DefaultGravityMultiplier = 1f;

        public float GravityScale = DefaultGravityMultiplier;

        private bool jumpEnded = true;

        //Coyote Time
        public bool CoyoteEnabled;
        
        public const float CoyoteTimeBuffer = 0.1f;

        private float timeOfLeavingGround; // the last time we were on the ground
        
        // we can use a coyote jump when its enables, we are midair, and within the coyote time buffer.
        public bool CanUseCoyote => CoyoteEnabled && !IsGrounded && timeOfLeavingGround + CoyoteTimeBuffer > Time.GameTime;

        // Jump press buffer
        public const float JumpPressBuffer = 0.25f;

        private float timeOfLastAirborneJumpPress;

        // we jump immediately once we are grounded and have a buffered jump
        public bool BufferedJump => timeOfLastAirborneJumpPress + JumpPressBuffer > Time.GameTime;

        // We can only jump when we are on the ground and we either just pressed jump or have it buffered,
        // OR if we pressed jump during coyote time.
        private bool WillJump =>
            IsGrounded && JumpButtonPressed || HasLanded && BufferedJump || !IsGrounded && JumpButtonPressed && CanUseCoyote;


        protected override void OnAwake()
        {

            // add needed components
            rigidBody = Owner.GetOrAddComponent<RigidBody>();
            rigidBody.IgnoreGravity = true;
            groundChecker = Owner.GetOrAddComponent<GroundCheckComponent>();

            textRenderer = Owner.GetComponent<GuiTextRenderer>();

            Owner.Layer = 1;
        }

        protected override void Update()
        {
#if DEBUG
            ShowDebug();
#endif

            // Update DummyState
            (HorizontalSpeed, VerticalSpeed) = rigidBody.LinearVelocity;

            WasGrounded = IsGrounded;
            IsGrounded = groundChecker.IsGrounded;
            
            // update the time of leaving ground if we left ground this frame
            // if we just landed, we re-enable coyote time
            if (HasLaunched) timeOfLeavingGround = Time.GameTime;
            else if (HasLanded) CoyoteEnabled = true;

            if (JumpButtonPressed)
            {
                if (!IsGrounded) { timeOfLastAirborneJumpPress = Time.GameTime; }
            }

            // Compute velocities
            ComputeGravityScale();
            ComputeHorizontalSpeed();
            ComputeVerticalSpeed();

            if (WillJump)
            {
                CoyoteEnabled = false;
                jumpEnded = false;
                timeOfLeavingGround = float.MinValue; // disable jump buffer, so that we don't get infinite jumps...

                PerformJump();
            }

            // if jump is released while mid-air and we're moving up
            // (since now we only move up while jumping, will need to explicitly model a jump later)
            if (JumpButtonUp && !IsGrounded && !jumpEnded && VerticalSpeed > 0) jumpEnded = true;

            // Apply computed velocity
            ApplyVelocity();
#if DEBUG
            DrawVelocityVector(rigidBody.LinearVelocity, Color.Magenta);
#endif

            // Update the input handler's state
            InputHandler.UpdateState();
        }

        /// <summary>
        /// Apply computed velocity taking slopes and state into account
        /// </summary>
        private void ApplyVelocity()
        {
            Vector2 velocity = default;
            switch (IsGrounded)
            {
                case true when !IsOnSlope:
                    SlopeClimbVerticalSpeed = 0;
                    velocity = new Vector2(HorizontalSpeed, VerticalSpeed);
                    break;
                case true when IsOnSlope:
                {
                    // FIXME, we slow down on slopes due to decomposing a pure 13-horizontal speed into 
                    // its components and then doing so again idempotently
                    SlopeClimbVerticalSpeed = MathF.Sin(SlopeAngle) * HorizontalSpeed;

                    if (WillJump)
                    {
                        // If we jump we should set normal velocity
                        // If we are moving up a slope, we should reset horizontal movement to zero
                        // so we don't have a jump-spam-to-speed-up exploit.
                        Debug.WriteLine("Jumping on Slope!");
                        velocity = new Vector2(IsAscendingSlope ? 0.0f : HorizontalSpeed, VerticalSpeed);
                    }
                    else
                    {
                        velocity.Y = SlopeClimbVerticalSpeed;
                        velocity.X = MathF.Cos(SlopeAngle) * HorizontalSpeed;
                    }

                    Debug.WriteLine(velocity.Length());
                    break;
                }
                case false:
                    SlopeClimbVerticalSpeed = 0;
                    velocity = new Vector2(HorizontalSpeed, VerticalSpeed);
                    break;
            }

            rigidBody.LinearVelocity = velocity;
        }

        /// <summary>
        /// Compute the gravity strength to apply to player in the current portion of its movement.
        /// </summary>
        private void ComputeGravityScale()
        {
            if (IsGrounded) GravityScale = DefaultGravityMultiplier;

            else if (VerticalSpeed < JumpVelocityFalloff) GravityScale = FallGravityMultiplier;
            
            else if (VerticalSpeed > 0)
            {
                GravityScale = jumpEnded ? FallGravityMultiplier : ClimbGravityMultiplier;
            }
            // when we are at rest gravity still acts on us.
            // We may not want this due to friction or whatever, but I'll leave it for now.
            else if (VerticalSpeed == 0) GravityScale = DefaultGravityMultiplier;
        }

        private void ComputeHorizontalSpeed()
        {
            // NOTE: Check assumes there is a dead zone on the stick input.
            if (HorizontalDirection != 0)
            {
                // use the appropriate acceleration
                CurrentAcceleration = IsGrounded ? MaxAcceleration : MaxArialAcceleration;

                // Set horizontal move speed
                HorizontalSpeed += HorizontalDirection * CurrentAcceleration * Time.DeltaTime;

                // clamped by max frame movement
                HorizontalSpeed = MathHelper.Clamp(HorizontalSpeed, -MaxHorizontalSpeed, MaxHorizontalSpeed);
            }
            else
            {
                // Decelerate the player
                HorizontalSpeed = MathUtils.MoveTowards(HorizontalSpeed, 0, MaxDeceleration * Time.DeltaTime);
            }
        }

        /// <summary>
        /// Compute the current vertical speed due to gravity without taking (potential) jumping into account.
        /// </summary>
        private void ComputeVerticalSpeed()
        {
            // Note, not applying gravity when on ground mitigates slope sliding
            if (!IsGrounded) VerticalSpeed +=  Gravity * GravityScale * Time.DeltaTime;

            // clamp to terminal velocity
            if (VerticalSpeed < MaxFallingSpeed) VerticalSpeed = MaxFallingSpeed; 
        }


        /// <summary>
        /// Trigger on jump input. Compute and add jump speed to current vertical velocity.
        /// </summary>
        private void PerformJump()
        {
            // reset vertical speed so we don't get shitty "half-jumps" or velocity negated jumps
            VerticalSpeed = 0;

            // get the jumping speed via sqrt(-2gh) from high school
            var jumpSpeed = MathF.Sqrt(-2f * Gravity * JumpHeight); 
            
            if (VerticalSpeed > 0f) // if we're moving up
            {
                // CAREFUL what are we assuming about verticalSpeed? Is it always +ve?
                jumpSpeed = MathF.Max(jumpSpeed - VerticalSpeed, 0f);
            }

            VerticalSpeed += jumpSpeed;
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
            tr.Text = $"H. Vel.: {HorizontalSpeed:F3}, \n" +
                      $"V. Vel.: {VerticalSpeed:F3}\n" +
                      $"S. Vel.: {SlopeClimbVerticalSpeed:F3}\n\n" +
                      $"H. Accel: {CurrentAcceleration}, Gravity Scale: {GravityScale}\n" +
                      $"isGrounded: {IsGrounded}\n" +
                      $"jumpEnded: {jumpEnded}\n" +
                      $"CanUseCoyote: {CanUseCoyote}\n" +
                      $"BufferedJump: {BufferedJump}\n\n" +
                      $"Pos: {this.Transform.Position}\n\n" +
                      $"Slope Angle: {groundChecker.SlopeAngleDown}\n" +
                      $"IsOnSlope: {IsOnSlope}\n" +
                      $"IsAscendingSlope: {IsAscendingSlope}\n" +
                      $"Vel Mag: {new Vector2(HorizontalSpeed, VerticalSpeed).Length()}";
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

        private void DrawVelocityVector(in Vector2 vel, in Color color)
        {
            var pos = this.Transform.Position;
            Gizmos.DrawLine(pos, pos + vel / vel.Length(), color, thickness: 3f);
        }
    }
}
