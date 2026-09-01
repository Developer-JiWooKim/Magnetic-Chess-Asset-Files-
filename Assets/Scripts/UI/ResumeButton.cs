using Assets.Scripts.Core;

namespace Assets.Scripts.UI
{
    public sealed class ResumeButton : UIPanel
    {
        public override void Show()
        {
            gameObject.SetActive(DontDestroy_Menu.Instance.CurrentScene == DontDestroy_Menu.SceneName.Game);
        }
    }
}
