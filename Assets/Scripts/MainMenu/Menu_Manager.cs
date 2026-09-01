using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.MainMenu
{
    public enum E_UI_Panel_Name
    {
        Start,
        ModeSelect,
        GameSetting,
    }

    public sealed class Menu_Manager : MonoBehaviour
    {
        [SerializeField]
        private Tablet_Logic Tablet_UI;

        private List<Panel_Base> panelList;

        public E_UI_Panel_Name currentName;

        private void Start()
        {
            Setup();
        }
        private void Setup()
        {
            currentName = DontDestroy_Menu.Instance.CurrentScene == DontDestroy_Menu.SceneName.Title ? E_UI_Panel_Name.Start : E_UI_Panel_Name.ModeSelect;
            panelList = GetComponentsInChildren<Panel_Base>(true).ToList();
        }

        private void ChangePanel(E_UI_Panel_Name currName)
        {
            panelList.ForEach((panel) => panel.Hide());
            panelList.Find((panel) => panel.panel_Name == currName).Show();
        }

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
                Tablet_UI.Switching_Select_to_Setting();
            }
        }
        public void Change_GameSetting_to_ModeSelect()
        {
            ChangePanelName(E_UI_Panel_Name.ModeSelect);
            ChangePanel(currentName);
            if (Tablet_UI != null)
            {
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
            panelList.ForEach((panel) => panel.Hide());
        }
        public void ChangePanelName(E_UI_Panel_Name _newState)
        {
            if (currentName != _newState)
            {
                currentName = _newState;
            }
        }

        public void PlaySound_Button_Press()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
        }
        public void PlaySound_DropDown_Press()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.DROPDOWN_PRESS);
        }
    }
}
