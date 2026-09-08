using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class OptionPanel : UIPanel
    {
        [Header("Slider")]
        [SerializeField] private Slider _sliderBGM;
        [SerializeField] private Slider _sliderSFX;

        private void Start() => Setup();
        private void Setup()
        {
            _sliderBGM.value = DataManager.Instance.data.volumeBgm;
            _sliderSFX.value = DataManager.Instance.data.volumeSfx;

            SetVolumeBGM();
            SetVolumeSFX();
        }

        public void SetOption()
        {
            DataManager.Instance.SaveGameOptionData();
        }

        public void SetVolumeBGM()
        {
            DataManager.Instance.data.volumeBgm = _sliderBGM.value;
            SoundManager.Instance.SetVolumeBGM(_sliderBGM.value);
        }

        public void SetVolumeSFX()
        {
            DataManager.Instance.data.volumeSfx = _sliderSFX.value;
            SoundManager.Instance.SetVolumeSFX(_sliderSFX.value);
        }

        public void PlaySoundDropDownPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.DropdownPress);
        }

        public void PlaySoundSlider()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.Slider);
        }

        public void PlaySoundSaveButtonPress()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SfxName.SaveButtonPress);
        }
    }
}
