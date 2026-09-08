using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class ResumePanel : UIPanel
    {
        [SerializeField] private GameObject _resumePanel;

        // events
        public event Action OnReplay;
        public event Action OnSelect;

        public void PlaySoundButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.ButtonPress);
        }

        public override void Show() => _resumePanel.SetActive(true);
        public override void Hide() => _resumePanel.SetActive(false);

        public void OnClickReplayButton()
        {
            OnReplay?.Invoke();
            Hide();
        }

        public void OnClickSelectModeButton()
        {
            OnSelect?.Invoke();
            Hide();
        }
    }
}
