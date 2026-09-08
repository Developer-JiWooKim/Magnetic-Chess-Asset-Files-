using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class HelpPanel : UIPanel
    {
        public void PlaySoundButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.ButtonPress);
        }
    }
}
