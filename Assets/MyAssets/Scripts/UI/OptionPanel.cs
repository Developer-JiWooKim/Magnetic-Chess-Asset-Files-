using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class OptionPanel : UIPanel
    {
        [Header("Slider")]
        [SerializeField] private Slider slider_BGM;
        [SerializeField] private Slider slider_SFX;

        private void Start() => Setup();
        private void Setup()
        {
            slider_BGM.value = DataManager.Instance.data.volume_value_BGM;
            slider_SFX.value = DataManager.Instance.data.volume_value_SFX;

            SetVolumeBGM();
            SetVolumeSFX();
        }

        public void SetOption()
        {
            DataManager.Instance.SaveGameOptionData();
        }

        public void SetVolumeBGM()
        {
            DataManager.Instance.data.volume_value_BGM = slider_BGM.value;
            SoundManager.Instance.SetVolume_BGM(slider_BGM.value);
        }

        public void SetVolumeSFX()
        {
            DataManager.Instance.data.volume_value_SFX = slider_SFX.value;
            SoundManager.Instance.SetVolume_SFX(slider_SFX.value);
        }

        public void PlaySoundDropDownPress()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.DROPDOWN_PRESS);
        }

        public void PlaySoundSlider()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.SLIDER);
        }

        public void PlaySoundSaveButtonPress()
        {
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.SAVE_BUTTON_PRESS);
        }
    }
}
