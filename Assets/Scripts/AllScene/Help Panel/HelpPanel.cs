using UnityEngine;

public class HelpPanel : MonoBehaviour
{
    public void PlaySound_Button_Press_()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
    }
}