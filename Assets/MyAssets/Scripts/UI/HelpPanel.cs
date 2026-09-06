using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class HelpPanel : UIPanel
    {
        public void PlaySound_Button_Press_()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.BUTTON_PRESS);
        }
    }
}
