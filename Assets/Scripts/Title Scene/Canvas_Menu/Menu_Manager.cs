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
    private Tablet_Logic Tablet_UI;             // �ΰ��� ������ ���� world UI

    private List<Panel_Base> panelList;         // �� �гε��� Show, Hide�� �����ϱ� ���� ����Ʈ

    public E_UI_Panel_Name currentName;         // ���� �г�(��ư�� ������ �� ������ UI)

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
    /// ���� ������ �г��� �̸��� �Ű������� �޾� �ش� �гθ� ���̰� ������ �г��� �� ���̰� �ϴ� �Լ�
    /// </summary>
    private void ChangePanel(E_UI_Panel_Name currName)
    {
        panelList.ForEach((panel) => panel.HidePanel());
        panelList.Find((panel) => panel.panel_Name == currName).ShowPanel();
    }
    /// <summary>
    /// ���� ���� �ٲ�, ����ȭ�� ���̵� �ƿ�, �ִϸ��̼� ���۱����� �ϴ� �Լ�
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
            // �׺� UI�� ���� ���� UI�� ����
            Tablet_UI.Switching_Select_to_Setting();
        }
    }
    public void Change_GameSetting_to_ModeSelect()
    {
        ChangePanelName(E_UI_Panel_Name.ModeSelect);
        ChangePanel(currentName);
        if (Tablet_UI != null)
        {
            // �׺� UI�� ���� ��� ���� UI�� ����
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
    /// ��ư Ŭ�� �� ���� �Ŵ����� ���� ȿ���� ���
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
