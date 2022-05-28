using Engine;

namespace SandPerSand
{
    public class MainMenu : Behaviour
    {
        protected override void OnAwake()
        {
            Template.InitializeMenu();
            Template.ShowMainMenu();
            
            UI.IsMouseVisible = true;
        }
    }
}
