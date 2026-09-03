using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class ResumePanel : UIPanel
    {
        [SerializeField]
        private GameObject resumePanel;

        public event Action OnReplay;
        public event Action OnSelect;

        public void PlaySound_Button_Press()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
        }

        public override void Show()
        {
            resumePanel.SetActive(true);
        }

        public override void Hide()
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
            Hide();
        }

        public void OnClickSelectModeButton()
        {
            if (OnSelect == null)
            {
                Debug.Log("OnSelect is Null ");
                return;
            }
            OnSelect();
            Hide();
        }
    }
}
