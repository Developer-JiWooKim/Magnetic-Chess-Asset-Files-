namespace Assets.MyAssets.Scripts.Core
{
    public sealed class DontDestroy_Menu : Singleton<DontDestroy_Menu>
    {
        public enum SceneName
        {
            Title,
            Game,
        }
        private SceneName currentScene = SceneName.Title;
        public SceneName CurrentScene => currentScene;

        public void ChangeGameScene() => currentScene = SceneName.Game;

        public void ChangeTitleScene() => currentScene = SceneName.Title;
    }
}
