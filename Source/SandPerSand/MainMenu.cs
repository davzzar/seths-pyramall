using Engine;

namespace SandPerSand
{
    public class MainMenu : Component
    {
        protected override void OnAwake()
        {
            Template.InitializeMenu();
            Template.ShowMainMenu();
            
            UI.IsMouseVisible = true;
        }
    }
}
