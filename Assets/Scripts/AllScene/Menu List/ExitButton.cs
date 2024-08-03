public class ExitButton : MenuButtonBase
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
