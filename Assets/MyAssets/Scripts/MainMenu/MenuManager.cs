using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public enum UIPanelName
    {
        Start,
        ModeSelect,
        GameSetting,
    }

    public sealed class MenuManager : MonoBehaviour
    {
        [SerializeField] private TabletLogic _tabletUI;

        private List<PanelBase> _panelList;

        public UIPanelName currentName;

        private void Start() => Setup();
        private void Setup()
        {
            currentName = DontDestroyMenu.Instance.CurrentScene == DontDestroyMenu.SceneName.Title ? UIPanelName.Start : UIPanelName.ModeSelect;
            _panelList = GetComponentsInChildren<PanelBase>(true).ToList();
        }

        private void ChangePanel(UIPanelName currName)
        {
            _panelList.ForEach((panel) => panel.Hide());
            _panelList.Find((panel) => panel.panelName == currName).Show();
        }

        public void ChangeStartToModeSelect()
        {
            ChangePanelName(UIPanelName.ModeSelect);
            _panelList.Find((panel) => panel.panelName == UIPanelName.Start).
                gameObject.GetComponent<StartPanel>().OnClickStartButton();
        }
        public void ChangeModeSelectToGameSetting()
        {
            ChangePanelName(UIPanelName.GameSetting);
            ChangePanel(currentName);
            if (_tabletUI != null)
            {
                _tabletUI.SwitchingSelectToSetting();
            }
        }
        public void ChangeGameSettingToModeSelect()
        {
            ChangePanelName(UIPanelName.ModeSelect);
            ChangePanel(currentName);
            if (_tabletUI != null)
            {
                _tabletUI.SwitchingSettingToSelect();
            }
        }
        public void AllHidePanel()
        {
            if (_panelList == null)
            {
                Debug.Log("MenuManager.cs - AllHidePanel() : _panelList is Null!!");
                return;
            }
            _panelList.ForEach((panel) => panel.Hide());
        }
        public void ChangePanelName(UIPanelName newState)
        {
            if (currentName != newState)
            {
                currentName = newState;
            }
        }

        public void PlaySoundButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.ButtonPress);
        }
        public void PlaySoundDropDownPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.DropdownPress);
        }
    }
}
