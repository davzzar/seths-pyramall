using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SandPerSand
{
    /// <summary>
    /// ButtonState enum with more states than the enum provided by Monogame.
    /// </summary>
    enum  ButtonState
    {
        Up = 0,
        Pressed = 1,
        Held = 2,
        Released = 3
    }
    
    internal class InputHandler
    {
        // Point of this class is to implement GENERAL gamepad input

        private PlayerIndex playerIndex;
        private GamePadState currState;
        private GamePadState prevState;

        public InputHandler(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
            // assign current state as the previous state.

            /// FIXME: This is commented as no gamepad is detected before engine.Run is called...
            // Check gamepad connection
            GamePadCapabilities capabilities = GamePad.GetCapabilities(this.playerIndex);
            if (!capabilities.IsConnected)
            {
                // TODO: Find better exception type.
                // TODO: Develop an in-game connection menu to avoid this situation.
                // throw new InvalidOperationException($"Gamepad for player index {this.playerIndex} is not connected.");
                return;
            }

            // check gamepad capabilities
            if (capabilities.GamePadType != GamePadType.GamePad)
            {
                throw new InvalidOperationException(
                    $"Connected gamepad for player index {this.playerIndex} is of wrong type.");
            }

            prevState = currState = GamePad.GetState(this.playerIndex, GamePadDeadZone.Circular);
        }

        // Button State Machine
        // TODO make private/protected and expose button API in descendents.
        public ButtonState getButtonState(Buttons b)
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
        public bool LeftTrigger(float threshold = 0.4f, float maxValue = 1.0f)
        {
            return (currState.Triggers.Left >= threshold && currState.Triggers.Left <= maxValue);
        }

        public bool RightTrigger(float threshold = 0.4f, float maxValue = 1.0f)
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
        public Vector2 getLeftThumbstickDir(float magnitudeThreshold = 0.5f)
        {
            return getThumbstickDir(false, magnitudeThreshold);
        }

        public float getLeftThumbstickDirX(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirX(false, magnitudeThreshold);
        }

        public float getLeftThumbstickDirY(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirY(false, magnitudeThreshold);
        }

        /// <summary>
        /// Get the direction vector of Right analogue stick.
        /// If the direction has a normalized magnitude of less than the given threshold, the zero vector is returned.
        /// </summary>
        /// <param name="magnitudeThreshold">The minimum magnitude of the direction vector to return.</param>
        /// <returns>The normalized direction vector of the Right analogue stick, or the zero vector.</returns>
        public Vector2 getRightThumbstickDir(float magnitudeThreshold = 0.5f)
        {
            return getThumbstickDir(true, magnitudeThreshold);
        }

        public float getRightThumbstickDirX(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirX(true, magnitudeThreshold);
        }

        public float getRightThumbstickDirY(float magnitudeThreshold = 0.5f)
        {
            return getThumstickDirY(true, magnitudeThreshold);
        }

        /// <summary>
        /// Updates the input handler's internal state tracker to enable OnPress and OnRelease inputs.
        /// </summary
        public void UpdateState()
        {
            prevState = currState;
            currState = GamePad.GetState(playerIndex);
        }
    }
}
