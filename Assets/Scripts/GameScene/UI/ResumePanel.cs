using System;
using UnityEngine;

public class ResumePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject resumePanel;

    public event Action OnReplay; // 게임 씬 - 리플레이 버튼눌렀을 때 실행될 기능을 담을 이벤트 액션(현재 진행중인 게임 종료,게임 세팅 UI 띄우기)
    public event Action OnSelect; // 게임 씬 - 모드 선택 버튼 눌렀을 때 실행될 기능을 담을 이벤트 액션(현재 진행중인 게임 종료, 게임 모드 선택 UI 띄우기)
    
    public void PlaySound_Button_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
    }
    /// <summary>
    /// Resume 버튼이 클릭되면 resume패널을 활성화
    /// </summary>
    public void OnClickResumeButton()
    {   
        resumePanel.SetActive(true);
    }
    /// <summary>
    /// Continue 버튼이 클릭되면 resume패널을 비활성화
    /// </summary>
    public void OnClickContinueButton()
    {
        resumePanel.SetActive(false);    
    }
    /// <summary>
    /// 게임을 초기화 시키고 GameSetting 화면으로 전환 및 resume 패널 비활성화
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
    /// 게임을 초기화 시키고 SelectMode 화면으로 
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
