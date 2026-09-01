using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.UI
{
    public sealed class ExitPanel : UIPanel
    {
        [SerializeField]
        private GameObject exitPanel;

        public override void Show()
        {
            exitPanel.SetActive(true);
        }
        public override void Hide()
        {
            exitPanel.SetActive(false);
        }
        public void OnClickExit_yes_Button()
        {
            Application.Quit();
        }
        public void PlaySound_Button_Press_()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
        }
    }
}
