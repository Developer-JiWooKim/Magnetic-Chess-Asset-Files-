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
        public SceneName CurrentScene
        {
            get
            {
                return currentScene;
            }
            private set
            {
                currentScene = value;
            }
        }

        /// <summary>
        /// currentScene -> Game
        /// </summary>
        public void ChangeGameScene() => currentScene = SceneName.Game;

        /// <summary>
        /// currentScene -> Title
        /// </summary>
        public void ChangeTitleScene() => currentScene = SceneName.Title;
    }
}
