using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SandPerSand
{
    /// <summary>
    /// ButtonState enum with more states than the enum provided by Monogame.
    /// </summary>
    public enum  ButtonState
    {
        Up = 0,
        Pressed = 1,
        Held = 2,
        Released = 3
    }

    
    public class InputHandler
    {
        // Point of this class is to implement GENERAL gamepad input

        private PlayerIndex playerIndex;
        public PlayerIndex PlayerIndex
        {
            get { return playerIndex; }
            set { playerIndex = value; }
        }
        private GamePadState currState;
        private GamePadState prevState;

        public InputHandler(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
            prevState = currState = GamePad.GetState(this.playerIndex, GamePadDeadZone.Circular);
        }

        // Button State Machine
        // TODO make private/protected and expose button API in descendents.
        public virtual ButtonState getButtonState(Buttons b)
        {
            if (currState.IsButtonUp(b))
            {
                if (prevState.IsButtonUp(b))
                {
                    return ButtonState.Up;
                }

                return ButtonState.Released;
            }

            // implicitly: currState is Down
            if (prevState.IsButtonUp(b))
            {
                return ButtonState.Pressed;
            }

            return ButtonState.Held;
            }
        
        // Triggers
        //TODO move the threshold constants to a settings section or file.
        public virtual  bool LeftTrigger(float threshold = 0.4f, float maxValue = 1.0f)
        {
            return (currState.Triggers.Left >= threshold && currState.Triggers.Left <= maxValue);
        }

        public virtual  bool RightTrigger(float threshold = 0.4f, float maxValue = 1.0f)
        {
            return (currState.Triggers.Right >= threshold && currState.Triggers.Right <= maxValue);
        }

        // Analogue Sticks
        // need directional and boolean state methods.
        // What we want. We want to get the directional state of the stick per axis and jointly.
        //TODO Implement 8-directional state query

        /// <summary>
        /// Get the direction vector of an analogue stick.
        /// If the direction has a normalized magnitude of less than the given threshold, the zero vector is returned.
        /// </summary>
        /// <param name="right">To query the right stick or not.</param>
        /// <param name="magnitudeThreshold">The minimum magnitude of the direction vector to return.</param>
        /// <returns>The normalized direction vector of the analogue stick, or the zero vector.</returns>
        // NOTE Maybe need to do separate axis magnitude tests.
        private Vector2 getThumbstickDir(bool right, float magnitudeThreshold = 0.5f)
        {
            Vector2 dir = right ? currState.ThumbSticks.Right : currState.ThumbSticks.Left;

            return (dir.Length() > magnitudeThreshold) ? dir : Vector2.Zero;
        }

        private float getThumstickDirX(bool right, float magnitudeThreshold = 0.5f)
        {
            return getThumbstickDir(right, magnitudeThreshold).X;
        }

        private float getThumstickDirY(bool right, float magnitudeThreshold = 0.5f)
        {
            return getThumbstickDir(right, magnitudeThreshold).Y;
        }

        /// <summary>
        /// Get the direction vector of Left analogue stick.
        /// If the direction has a normalized magnitude of less than the given threshold, the zero vector is returned.
        /// </summary>
        /// <param name="magnitudeThreshold">The minimum magnitude of the direction vector to return.</param>
        /// <returns>The normalized direction vector of the Left analogue stick, or the zero vector.</returns>
        public virtual Vector2 getLeftThumbstickDir(float magnitudeThreshold = 0.5f)
        {
            return getThumbstickDir(false, magnitudeThreshold);
        }

        public virtual float getLeftThumbstickDirX(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirX(false, magnitudeThreshold);
        }

        public virtual float getLeftThumbstickDirY(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirY(false, magnitudeThreshold);
        }

        /// <summary>
        /// Get the direction vector of Right analogue stick.
        /// If the direction has a normalized magnitude of less than the given threshold, the zero vector is returned.
        /// </summary>
        /// <param name="magnitudeThreshold">The minimum magnitude of the direction vector to return.</param>
        /// <returns>The normalized direction vector of the Right analogue stick, or the zero vector.</returns>
        public virtual Vector2 getRightThumbstickDir(float magnitudeThreshold = 0.5f)
        {
            return getThumbstickDir(true, magnitudeThreshold);
        }

        public virtual float getRightThumbstickDirX(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirX(true, magnitudeThreshold);
        }

        public virtual float getRightThumbstickDirY(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirY(true, magnitudeThreshold);
        }

        /// <summary>
        /// Updates the input handler's internal state tracker to enable OnPress and OnRelease inputs.
        /// </summary
        public void UpdateState()
        {
            prevState = currState;
            currState = GamePad.GetState(PlayerIndex);
        }
    }

    public class DummyInputHandler : InputHandler
    {
        public DummyInputHandler(PlayerIndex playerIndex) : base(playerIndex)
        {   
        }
        public override float getLeftThumbstickDirX(float magnitudeThreshold = 0.5f)
        {
            float dir = MathF.Sin(5f * Time.GameTime);
            System.Diagnostics.Debug.WriteLine($"Dummy: {dir}");
            return dir;
        }
    }
}
