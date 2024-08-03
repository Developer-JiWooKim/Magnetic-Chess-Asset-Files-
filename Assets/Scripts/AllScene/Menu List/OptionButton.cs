public class OptionButton : MenuButtonBase
{
    public override void Show()
    {
        gameObject.SetActive(true);
    }
    public override void Hide()
    {
        gameObject.SetActive(false);
    }
}
