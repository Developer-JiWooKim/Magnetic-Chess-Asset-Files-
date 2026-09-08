namespace Assets.MyAssets.Scripts.Core
{
    public sealed class DontDestroyMenu : Singleton<DontDestroyMenu>
    {
        public enum SceneName
        {
            Title,
            Game,
        }
        private SceneName _currentScene = SceneName.Title;
        public SceneName CurrentScene => _currentScene;

        public void ChangeGameScene() => _currentScene = SceneName.Game;

        public void ChangeTitleScene() => _currentScene = SceneName.Title;
    }
}
