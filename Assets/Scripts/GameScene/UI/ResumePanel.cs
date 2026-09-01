using System;
using UnityEngine;

public class ResumePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject resumePanel;

    public event Action OnReplay;
    public event Action OnSelect;

    public void PlaySound_Button_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
    }

    public void OnClickResumeButton()
    {
        resumePanel.SetActive(true);
    }

    public void OnClickContinueButton()
    {
        resumePanel.SetActive(false);
    }

    public void OnClickReplayButton()
    {
        if (OnReplay == null)
        {
            Debug.Log("OnReplay is Null ");
            return;
        }
        OnReplay();
        OnClickContinueButton();
    }

    public void OnClickSelectModeButton()
    {
        if (OnSelect == null)
        {
            Debug.Log("OnSelect is Null ");
            return;
        }
        OnSelect();
        OnClickContinueButton();
    }
}
