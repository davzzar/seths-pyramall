using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SandPerSand
{

    /// <summary>
    /// A very naive component to switch between a camera which follows Parent game object and a static camera.
    /// </summary>
    public class CameraSwitcherComponent : Behaviour

    {
        public InputHandler InputHandler { get; set; }
        public GameObject Parent { get; set; }
        public Vector2 GlobalPosition { get; set; }
        public float GlobalHeight { get; set; }
        public float LocalHeight { get; set; }

        private bool isGlobal = false;

        protected override void OnAwake()
        {
            InputHandler = new InputHandler(PlayerIndex.One);
            GlobalPosition = Vector2.Zero;
            GlobalHeight = 50f;
            LocalHeight = 10f;
        }

        protected override void Update()
        {
            if (InputHandler.getButtonState(Buttons.RightStick) == ButtonState.Pressed)
            {
                if (isGlobal)
                {
                    SwitchToParent();
                    System.Diagnostics.Debug.WriteLine("Switching to Parent Camera...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Switching to Global Camera...");
                    SwitchToGlobal();
                }

                isGlobal = !isGlobal;

            }

            InputHandler.UpdateState();
        }

        private void SwitchToParent()
        {
            this.Owner.Transform.Parent = Parent.Transform;
            this.Owner.Transform.LocalPosition = Vector2.Zero;
            this.Owner.GetComponent<Camera>().Height = LocalHeight;
        }

        private void SwitchToGlobal()
        {
            this.Owner.Transform.Transform.Parent = null;
            this.Owner.Transform.Transform.Position = GlobalPosition;
            this.Owner.GetComponent<Camera>().Height = GlobalHeight;
        }
    }
}
