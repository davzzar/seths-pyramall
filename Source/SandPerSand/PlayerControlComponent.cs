using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace SandPerSand
{
    internal class PlayerControlComponent : Behaviour
    {
        [Browsable(false)]
        public InputHandler InputHandler { get; set; }

        public RigidBody rigidBody { get; private set; }
        private GroundCheckComponent groundChecker;
        private GuiTextRenderer textRenderer;
        private TimerBar timerBar;
        private OnScreenControlController onScreenControls;

        // Input
        private const Buttons JumpButton = Buttons.A;
        private const Buttons ActionButton = Buttons.A;
        private bool JumpButtonPressed => !this.IgnorePlayerInput && InputHandler.getButtonState(JumpButton) == ButtonState.Pressed;
        private bool JumpButtonUp => !this.IgnorePlayerInput && InputHandler.getButtonState(JumpButton) == ButtonState.Up;

        private float HorizontalDirection => !this.IgnorePlayerInput
            ? InputHandler.getLeftThumbstickDirX(magnitudeThreshold: 0.1f) *
              this.Owner.GetComponentInChildren<PlayerStates>().GetInvertedMovement()
            : 0f;

        public bool IgnorePlayerInput { get; set; }

        // Hard Jump
        public bool WillHardJump => CanHardJump&& JumpButtonPressed;
        private bool couldHardJump = false;
        private Vector2 HardJumpPosition; 
        private bool CanHardJump
        {
            get
            {
                var xVelo = rigidBody.LinearVelocity.X;
                var haloGo = Owner.GetComponentInChildren<EffectAnimatorController>().Owner;
                if (GameStateManager.Instance.CurrentState != GameState.InRound
                    || sandSimulation == null || IsGrounded || xVelo==0)
                {
                    // TODO disable HaloGo.
                    haloGo.IsEnabled = false;
                    couldHardJump = false;
                    return couldHardJump;
                }
                float leftBound = 0.7f;
                float rightBound = 1.5f;
                float sandGridStep = 0.1f;
                if (xVelo < 0)
                {
                    leftBound = -rightBound;
                    rightBound = -leftBound;
                }

                for (float x = Transform.Position.X + leftBound;
                    x < Transform.Position.X + rightBound; x += sandGridStep)
                {
                    var detectPosition = new Vector2(x, Transform.Position.Y);
                    SandSim.Int2 index = sandSimulation.SandData.PointToIndex(detectPosition);
                    SandSim.Int2 index2 = new SandSim.Int2(index.X, index.Y-1);
                    var sandGrid = sandSimulation.SandData[index];
                    var sandGrid2 = sandSimulation.SandData[index2];
                    if (sandGrid.HasSand && !sandGrid.IsSandStable
                        && sandGrid2.HasSand && !sandGrid2.IsSandStable)
                    {
                        // TODO create HaloGo at THAT place.
                        // activate Halo Animation
                        if (!couldHardJump)
                        {
                            haloGo.IsEnabled = true;
                        }
                        couldHardJump = true;
                        HardJumpPosition = detectPosition;
                        return couldHardJump;
                    }
                }
                // TODO disable HaloGo.
                haloGo.IsEnabled = false;
                couldHardJump = false;
                return couldHardJump;
            }
        }
        // Sand Stream
        private bool SandStreamDetected
        {
            get
            {
                if (GameStateManager.Instance.CurrentState != GameState.InRound  &&
                    GameStateManager.Instance.CurrentState != GameState.CountDown
                    || sandSimulation == null)
                {
                    return false;
                }
                // set up sand detector vector
                var sandDetector = Conf.SandDetectVec;
                if (!wasFacingRight)
                {
                    sandDetector = -sandDetector;
                }

                // detect sand
                var detectPosition = Transform.Position + sandDetector;
                Gizmos.DrawLine(detectPosition,
                    detectPosition + new Vector2(0, 0.1f),Color.LimeGreen,2);

                var index = sandSimulation.SandData.PointToIndex(detectPosition);
                var sandGrid = sandSimulation.SandData[index];
                if(sandGrid.HasSand && !sandGrid.IsSandStable)
                {
                    // delete sands
                    var deleteSandPosition = Transform.Position - sandDetector;
                    Circle circle = new Circle(deleteSandPosition,
                        Conf.SandDetectRemoveRadius);
                    sandSimulation.RemoveSand(circle);
                    return true;
                }
                return false;
            }
        }
        
        // State
        public bool IsGrounded { get; private set; }
        public bool WasGrounded { get; private set; }
        public bool HasLaunched => WasGrounded && !IsGrounded;
        public bool HasLanded => !WasGrounded && IsGrounded;
        public bool IsOnSlope => groundChecker.IsOnSlope;
        public bool HasLandedInSand = false;


        // Horizontal movement
        public float HorizontalSpeed { get; private set; }

        public float CurrentAcceleration { get; private set; }

        public float MaxAcceleration => 110f * this.Owner.GetComponentInChildren<PlayerStates>().GetAccellerationFactor(); //change these vals for changing vertical speed

        public const float MaxArialAcceleration = 50f;

        public const float MaxDeceleration = 60f;

        public const float MaxHorizontalSpeed = 7f;

        // Movement on Slopes
        public float SlopeClimbVerticalSpeed { get; private set; }
        public float SlopeAngle => groundChecker.SlopeAngleDown;
        public bool IsAscendingSlope => IsOnSlope && HorizontalSpeed != 0 && !(MathF.Sign(HorizontalSpeed) == 1 ^ MathF.Sign(SlopeAngle) == 1);

        // Vertical movement
        public float VerticalSpeed { get; private set; }

        public float JumpHeight => 7f * this.Owner.GetComponentInChildren<PlayerStates>().GetJumpFactor(); // explicit jump height
        //increase jump hight here

        public const float MaxFallingSpeed = -20f;

        // Player Gravity and Variable Jump Height
        public const float JumpVelocityFalloff = 1f;

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
        public bool WillJump =>
            IsGrounded && JumpButtonPressed || HasLanded && BufferedJump || !IsGrounded && JumpButtonPressed && CanUseCoyote;

        // Sand Interaction
        private SandSimulation sandSimulation;
        public bool HasSandReached
        {
            get
            {
                if (this.sandSimulation == null || !this.sandSimulation.IsAlive)
                {
                    this.sandSimulation = GameObject.FindComponent<SandSimulation>();

                }
                return GameStateManager.Instance.CurrentState != GameState.Shop && this.sandSimulation != null && this.sandSimulation.RaisingSandHeight >= this.Owner.Transform.Position.Y - this.Transform.Scale.Y / 2;
            }
        }

        private bool HasSandReachedBefore;
        private const float SandResistancePush = 16f;
        private bool isSandEscapeJump;
        private bool wasFacingRight = true;
        public bool DieFromDrown => timerBar.FillLevel <= TimerBar.EmptyLevel + 1e-05f;

        protected override void OnEnable()
        {
            // add needed components
            rigidBody = Owner.GetOrAddComponent<RigidBody>();
            rigidBody.IgnoreGravity = true;
            groundChecker = Owner.GetOrAddComponent<GroundCheckComponent>();

            textRenderer = Owner.GetComponent<GuiTextRenderer>();

            timerBar = Owner.GetComponent<TimerBar>();
            sandSimulation = GameObject.FindComponent<SandSimulation>();
            Owner.Layer = 1;

            onScreenControls = Owner.GetComponentInChildren<OnScreenControlController>();

            this.Owner.GetComponent<PlayerComponent>()!.IsPlayerAlive = true;

            this.HasSandReachedBefore = false;
        }

        protected override void OnDisable()
        {
            // unsubscribe from events
        }

        protected override void Update()
        {
            ControlUpdate();
        }



        private void DetectHardJump()
        {

            if (CanHardJump)
            {

            }
            if (WillHardJump)
            {
                PerformHardJump();
            }
        }

        private void ControlUpdate()
        {
            //Debug.WriteLine($"Position of game object {Owner.Name}: {Owner.Transform.Position}");
            
            #if DEBUG
            ShowDebug();
            #endif

            if (!Owner.GetComponent<PlayerComponent>()?.IsPlayerAlive ?? false)
            {
                return;
            }

            HasLandedInSand = false;

            // Sand Interaction
            DetectHardJump();

            if (HasSandReached && !HasSandReachedBefore)
            {
                // enable mashing bar
                timerBar.IsActive = true;
                timerBar.SetDepletingAt(0.3f);
                HasLandedInSand = true;

                // show spam controls
                onScreenControls.Owner.IsEnabled = true;

                // reset velocities
                rigidBody.LinearVelocity = Vector2.Zero;
                HorizontalSpeed = VerticalSpeed = 0.0f;

                // TODO Use current vertical speed to set player depth in sand accordingly
                // TODO (use ray casts with max depth to avoid clipping)
                if (this.Transform.Position.Y - this.Transform.Scale.Y / 2 < sandSimulation.RaisingSandHeight)
                {
                    // snap us to be just below the sand level
                    var pos = this.Transform.Position;
                    // Note, adding the -0.1f to this prevents continuous jumping on sand when its not rising
                    pos.Y = sandSimulation.RaisingSandHeight + this.Transform.Scale.Y / 2 - 0.1f;
                    this.Transform.Position = pos;
                }

                HasSandReachedBefore = true;
            }
            
            //HasSandReachedBefore = HasSandReached;

            if (HasSandReachedBefore)
            {
                if (!HasSandReached)
                {
                    // Exit trapped state and perform jump
                    // Switch to recharging
                    timerBar.SetRechargingAt(0.15f);
                    HasSandReachedBefore = false;
                    onScreenControls.Owner.IsEnabled = false;

                    // Do jump
                    CoyoteEnabled = false;
                    jumpEnded = false;
                    timeOfLeavingGround = float.MinValue;
                    isSandEscapeJump = true;

                    PerformJump();
                    ApplyVelocity();
                } 
                else if (timerBar.FillLevel <= TimerBar.EmptyLevel + 1e-05f)
                {
                    // (Exit trapped state) and die
                    Debug.WriteLine($"Player {InputHandler.PlayerIndex} has died!");
                    
                    rigidBody.LinearVelocity = Vector2.Zero;

                    var player = this.Owner.GetComponent<PlayerComponent>();
                    
                    if (player != null)
                    {
                        player.IsPlayerAlive = false;
                        // remove on screen controls as well
                        onScreenControls.Owner.IsEnabled = false;
                        Debug.WriteLine("Player IsAlive set to false.");
                    } else
                    {
                        Debug.WriteLine("Could not find PlayerComponent.");
                    }


                    var playerStates = this.Owner.GetComponent<PlayerStates>();

                    if (playerStates != null) {
                        // TODO:: This is wrong. Couldn't find a way to fucking set it properly....
                        // Currently is properly reset at CheckAllDead where I know the proper number of players
                        playerStates.RoundRank = -1;
                        Debug.WriteLine("PlayerStates Roundrank of dead player set.");
                    } else
                    {
                        Debug.WriteLine("Could not find PlayerStates.");
                    } 

                    timerBar.IsActive = false;
                    this.IsActive = false;
                }
                else
                {
                    // TODO make this speed not dependent on RaisingSandSpeed
                    // Note If we don't check for sand rising,
                    // we will get stuck in loop of entering, snapping, and exit jumping
                    var pushStrength = SandResistancePush / (60 * Time.DeltaTime);

                    var restMultiplier = 0.5f;
                    var sandVelocity = Vector2.Zero;
                    if (JumpButtonPressed)
                    {
                        sandVelocity.X = MathF.Sign(HorizontalDirection) * pushStrength;
                        sandVelocity.Y = sandSimulation.RaisingSandSpeed * pushStrength;
                    }
                    else
                    {
                        sandVelocity.X = 0.0f;
                        sandVelocity.Y =  sandSimulation.RaisingSandSpeed * restMultiplier;
                    }

                    if (!sandSimulation.IsSandRising)
                    {
                        sandVelocity.Y = 0.0f;
                    }

                    rigidBody.LinearVelocity = sandVelocity;
                }
                return;
            }


            // Update DummyState
            (HorizontalSpeed, VerticalSpeed) = rigidBody.LinearVelocity;

            WasGrounded = IsGrounded;
            IsGrounded = groundChecker.IsGrounded;
            
            // update the time of leaving ground if we left ground this frame
            // if we just landed, we re-enable coyote time
            if (HasLaunched) timeOfLeavingGround = Time.GameTime;
            else if (HasLanded)
            {
                CoyoteEnabled = true;
                if (isSandEscapeJump) isSandEscapeJump = false;
            }

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
            if (!isSandEscapeJump && JumpButtonUp && !IsGrounded && !jumpEnded && VerticalSpeed > 0) jumpEnded = true;

            if (HorizontalDirection > 0 && !wasFacingRight || HorizontalDirection < 0 && wasFacingRight)
            {
                wasFacingRight = HorizontalDirection > 0;
                Owner.GetComponent<SpriteRenderer>()!.FlipHorizontal();
            }

            // Apply computed velocity
            ComputeSandStreamInfluence();
            ApplyVelocity();
            
            #if DEBUG
            DrawVelocityVector(rigidBody.LinearVelocity, Color.Magenta);
            #endif
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
            if (!this.Owner.GetComponentInChildren<PlayerStates>().gravityOn()) GravityScale = 0;
            else if (IsGrounded) GravityScale = DefaultGravityMultiplier;

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
            //if (sandDetected)
            //{
            //    ComputeSandStreamInfluence();
            //    return;
            //}
            // NOTE: Check assumes there is a dead zone on the stick input.
            if (HorizontalDirection != 0) //
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
            if (VerticalSpeed < MaxFallingSpeed)
            {
                VerticalSpeed = MaxFallingSpeed;
            }
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
        /// Trigger on jump input near sand stream
        /// Hand over control to HardJumpController
        /// </summary>
        private void PerformHardJump()
        {
            var hardJumpComp = Owner.GetOrAddComponent<HardJumpController>();
            hardJumpComp.StartPosition = HardJumpPosition;
            hardJumpComp.IsActive = true;
        }

        private void ComputeSandStreamInfluence()
        {
            if (SandStreamDetected)
            {
                HorizontalSpeed = MathUtils.MoveTowards(HorizontalSpeed, 0, MaxDeceleration * 1.4f * Time.DeltaTime);
                if (!IsGrounded) VerticalSpeed += Gravity * 3f * Time.DeltaTime;
            }
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
                      $"HasLandedInSand: {HasLandedInSand}\n" +
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
