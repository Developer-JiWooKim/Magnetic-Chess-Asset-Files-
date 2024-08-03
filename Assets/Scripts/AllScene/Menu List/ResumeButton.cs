public class ResumeButton : MenuButtonBase
{
    public override void Show()
    {
        gameObject.SetActive(DontDestroy_Menu.Instance.CurrentScene == DontDestroy_Menu.SceneName.Game);
    }
    public override void Hide()
    {
        gameObject.SetActive(false);
    }

}
