using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class ResumeButton : UIPanel
    {
        public override void Show()
        {
            gameObject.SetActive(DontDestroyMenu.Instance.CurrentScene == DontDestroyMenu.SceneName.Game);
        }
    }
}