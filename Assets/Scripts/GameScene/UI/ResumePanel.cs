using System;
using UnityEngine;

public class ResumePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject resumePanel;

    public event Action OnReplay; // ���� �� - ���÷��� ��ư������ �� ����� ����� ���� �̺�Ʈ �׼�(���� �������� ���� ����,���� ���� UI ����)
    public event Action OnSelect; // ���� �� - ��� ���� ��ư ������ �� ����� ����� ���� �̺�Ʈ �׼�(���� �������� ���� ����, ���� ��� ���� UI ����)
    
    public void PlaySound_Button_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
    }
    /// <summary>
    /// Resume ��ư�� Ŭ���Ǹ� resume�г��� Ȱ��ȭ
    /// </summary>
    public void OnClickResumeButton()
    {   
        resumePanel.SetActive(true);
    }
    /// <summary>
    /// Continue ��ư�� Ŭ���Ǹ� resume�г��� ��Ȱ��ȭ
    /// </summary>
    public void OnClickContinueButton()
    {
        resumePanel.SetActive(false);    
    }
    /// <summary>
    /// ������ �ʱ�ȭ ��Ű�� GameSetting ȭ������ ��ȯ �� resume �г� ��Ȱ��ȭ
    /// </summary>
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
    /// <summary>
    /// ������ �ʱ�ȭ ��Ű�� SelectMode ȭ������ 
    /// </summary>
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
