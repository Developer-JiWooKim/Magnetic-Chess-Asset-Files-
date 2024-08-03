using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum E_UI_Panel_Name
{
    Start,
    ModeSelect,
    GameSetting,
}

public class Menu_Manager : MonoBehaviour
{
    [SerializeField]
    private Tablet_Logic Tablet_UI;             // 인게임 연출을 위한 world UI

    private List<Panel_Base> panelList;         // 각 패널들의 Show, Hide를 관리하기 위한 리스트

    public E_UI_Panel_Name currentName;         // 현재 패널(버튼을 눌렀을 때 보여질 UI)

    private void Start()
    {
        Setup();
    }
    private void Setup()
    {
        currentName = DontDestroy_Menu.Instance.CurrentScene == DontDestroy_Menu.SceneName.Title ? E_UI_Panel_Name.Start : E_UI_Panel_Name.ModeSelect;
        panelList = GetComponentsInChildren<Panel_Base>(true).ToList();
    }
    /// <summary>
    /// 현재 보여질 패널의 이름을 매개변수로 받아 해당 패널만 보이고 나머지 패널은 안 보이게 하는 함수
    /// </summary>
    private void ChangePanel(E_UI_Panel_Name currName)
    {
        panelList.ForEach((panel) => panel.HidePanel());
        panelList.Find((panel) => panel.panel_Name == currName).ShowPanel();
    }
    /// <summary>
    /// 현재 상태 바꿈, 시작화면 페이드 아웃, 애니메이션 시작까지만 하는 함수
    /// </summary>
    public void Change_Start_to_ModeSelect()
    {
        ChangePanelName(E_UI_Panel_Name.ModeSelect);
        panelList.Find((panel) => panel.panel_Name == E_UI_Panel_Name.Start).
            gameObject.GetComponent<StartPanel>().OnClickStartButton();
    }
    public void Change_ModeSelect_to_GameSetting()
    {
        ChangePanelName(E_UI_Panel_Name.GameSetting);
        ChangePanel(currentName);
        if (Tablet_UI != null)
        {
            // 테블릿 UI를 게임 세팅 UI로 변경
            Tablet_UI.Switching_Select_to_Setting();
        }
    }
    public void Change_GameSetting_to_ModeSelect()
    {
        ChangePanelName(E_UI_Panel_Name.ModeSelect);
        ChangePanel(currentName);
        if (Tablet_UI != null)
        {
            // 테블릿 UI를 게임 모드 선택 UI로 변경
            Tablet_UI.Switching_Setting_to_Select();
        }
    }
    public void All_HidePanel()
    {
        if (panelList == null)
        {
            Debug.Log("Menu_Manager.cs - All_HidePanel() : panelList is Null!!");
            return;
        }
        panelList.ForEach((panel) => panel.HidePanel());
    }
    public void ChangePanelName(E_UI_Panel_Name _newState)
    {
        if (currentName != _newState)
        {
            currentName = _newState;
        }
    }
    /// <summary>
    /// 버튼 클릭 시 사운드 매니저를 통해 효과음 재생
    /// </summary>
    public void PlaySound_Button_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
    }
    public void PlaySound_DropDown_Press()
    {
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.DROPDOWN_PRESS);
    }
}
