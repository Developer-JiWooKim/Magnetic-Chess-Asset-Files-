using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class ExitPanel : UIPanel
    {
        [SerializeField] private GameObject _exitPanel;

        public override void Show() => _exitPanel.SetActive(true);
        public override void Hide() => _exitPanel.SetActive(false);

        public void OnClickExitYesButton()
        {
            Application.Quit();
        }
        public void PlaySoundButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.ButtonPress);
        }
    }
}
