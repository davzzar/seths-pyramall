using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Resources;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SandPerSand.SandSim;

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

        // Input
        private const Buttons JumpButton = Buttons.A;
        private const Buttons ActionButton = Buttons.A;
        private bool JumpButtonPressed => InputHandler.getButtonState(JumpButton) == ButtonState.Pressed;
        private bool JumpButtonUp => InputHandler.getButtonState(JumpButton) == ButtonState.Up;
        private float HorizontalDirection => InputHandler.getLeftThumbstickDirX(magnitudeThreshold: 0.1f) * this.Owner.GetComponentInChildren<PlayerStates>().getInvertedMovement();

        // Hard Jump
        private bool canHardJump = false;
        public bool WillHardJump => JumpButtonPressed && canHardJump;
        private bool blockSandDetect = false;
        private bool doSandDetect => (!blockSandDetect) && rigidBody.LinearVelocity.X != 0 && !IsGrounded;
        private bool blockHControl = false;
        
        // Hard Jump parameters
        private float maxHSpeedAfterSand = 0.6f* MaxHorizontalSpeed;
        private float decelerationForBlockHControl = 0.7f * MaxDeceleration;
        private float hardJumpSpan = 0.4f;
        private float blockFallSandSpan = 0.4f;
        private float blockHControlSpan = 0.4f;

        // State
        public bool IsGrounded { get; private set; }
        public bool WasGrounded { get; private set; }
        public bool HasLaunched => WasGrounded && !IsGrounded;
        public bool HasLanded => !WasGrounded && IsGrounded;
        public bool IsOnSlope => groundChecker.IsOnSlope;


        // Horizontal movement
        public float HorizontalSpeed { get; private set; }

        public float CurrentAcceleration { get; private set; }

        public float MaxAcceleration => 110f * this.Owner.GetComponentInChildren<PlayerStates>().getAccellerationFactor(); //change these vals for changing vertical speed

        public const float MaxArialAcceleration = 50f;

        public const float MaxDeceleration = 60f;

        public const float MaxHorizontalSpeed = 7f;

        // Movement on Slopes
        public float SlopeClimbVerticalSpeed { get; private set; }
        public float SlopeAngle => groundChecker.SlopeAngleDown;
        public bool IsAscendingSlope => IsOnSlope && HorizontalSpeed != 0 && !(MathF.Sign(HorizontalSpeed) == 1 ^ MathF.Sign(SlopeAngle) == 1);

        // Vertical movement
        public float VerticalSpeed { get; private set; }

        public float JumpHeight => 7f * this.Owner.GetComponentInChildren<PlayerStates>().getJumpFactor(); // explicit jump height
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
                return this.sandSimulation != null && this.sandSimulation.RaisingSandHeight >= this.Owner.Transform.Position.Y - this.Transform.Scale.Y / 2;
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
        }

        protected override void Update()
        {
            ControlUpdate();
            // Update the input handler's state after every control update
            InputHandler.UpdateState();
        }

        private void HardJumpThroughFallingSand()
        {
            if (GameStateManager.Instance.CurrentState != GameState.InRound
                || sandSimulation == null)
            {
                return;
            }
            // set up sand detector vector
            var xVelo = rigidBody.LinearVelocity.X;
            var yVelo = rigidBody.LinearVelocity.Y;
            var sandDetector = new Vector2(1f, 0);
            if (xVelo < 0)
            {
                sandDetector = -sandDetector;
            }
            // Detect sand
            bool sandDetected = false;
            try { 
                var index = sandSimulation.SandData.PointToIndex(Transform.Position + sandDetector);
                var sandGrid = sandSimulation.SandData[index];
                sandDetected = doSandDetect && sandGrid.HasSand && !sandGrid.IsSandStable;
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("SandData.PointToIndex failed for position :" + (Transform.Position + sandDetector));
                return;
            }
            // If detected sand
            var haloGo = Owner.GetComponentInChildren<EffectAnimatorController>().Owner;
            if (sandDetected)
            {
                // press A within 0.5s otherwise fall
                // set onjump delegate
                canHardJump = true;
                blockSandDetect = true;
                blockHControl = true;
                // calculate immediate influence
                xVelo = MathHelper.Clamp(xVelo, -maxHSpeedAfterSand, maxHSpeedAfterSand);
                yVelo = 0f;
                // apply immediate influence
                rigidBody.LinearVelocity = new Vector2(xVelo, yVelo);
                // TODO activate Halo Animation
                haloGo.IsEnabled = true;
                // set timers
                Owner.AddComponent<GoTimer>().Init(hardJumpSpan, ()=> {
                    canHardJump = false; // only able to hardjump within 0.5s
                    // TODO not exactly what I want
                    Owner.AddComponent<GoTimer>().Init(0.2f, () => {
                        haloGo.IsEnabled = false;
                    });
                });
                Owner.AddComponent<GoTimer>().Init(blockFallSandSpan, () => {
                    blockSandDetect = false;// stop detect falling sand for 1s
                });
                Owner.AddComponent<GoTimer>().Init(blockHControlSpan, () => {
                    blockHControl = false;// not allowed to accelerate horizontally for 2s 
                });
            }
            if (WillHardJump)
            {
                // TODO breakthroughsand effect
                var position = Owner.Transform.Position;
                var effectGo = new GameObject("BreakThroughSand Effect");

                
                // just copied these code to make the jump works ...
                CoyoteEnabled = false;
                jumpEnded = false;
                timeOfLeavingGround = float.MinValue;
                isSandEscapeJump = true;
                PerformJump();
                ApplyVelocity();
                // reset control timers in advance if hard jump is performed
                canHardJump = false;
                blockHControl = false;
            }
        }

        private void ControlUpdate()
        {
            #if DEBUG
            ShowDebug();
            #endif

            if (!Owner.GetComponent<PlayerComponent>()?.IsAlive ?? false)
            {
                return;
            }

            // Sand Interaction
            HardJumpThroughFallingSand();

            if (HasSandReached && !HasSandReachedBefore)
            {
                // enable mashing bar
                timerBar.IsActive = true;
                timerBar.FillLevel = 1f;

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
                    timerBar.IsActive = false;
                    HasSandReachedBefore = false;

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
                        player.IsAlive = false;
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
            if (blockHControl)
            {
                HorizontalSpeed = MathUtils.MoveTowards(HorizontalSpeed, 0, decelerationForBlockHControl * Time.DeltaTime);
                return;
            }
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
