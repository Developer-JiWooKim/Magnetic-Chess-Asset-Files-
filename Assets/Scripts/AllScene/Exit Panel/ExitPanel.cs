using UnityEngine;

public class ExitPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject exitPanel;

    public void OnClickExitButton()
    {
        exitPanel.SetActive(true);
    }
    public void OnClickExit_no_Button()
    {
        exitPanel.SetActive(false);
    }
    public void OnClickExit_yes_Button()
    {
        Application.Quit();
    }
    public void PlaySound_Button_Press_()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
    }
}
