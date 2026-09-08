using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class OptionPanel : UIPanel
    {
        [SerializeField] private GameObject _root;

        [Header("Slider")]
        [SerializeField] private Slider _sliderBGM;
        [SerializeField] private Slider _sliderSFX;

        [Header("Button")]
        [SerializeField] private Button _saveButton;

        protected override void Bind()
        {
            UIBinder.Bind(_sliderBGM, SetVolumeBGM, this, nameof(_sliderBGM));
            UIBinder.Bind(_sliderSFX, SetVolumeSFX, this, nameof(_sliderSFX));
            UIBinder.Bind(_saveButton, OnClickSaveButton, this, nameof(_saveButton),
                SoundManager.SfxName.SaveButtonPress);
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_sliderBGM);
            UIBinder.Unbind(_sliderSFX);
            UIBinder.Unbind(_saveButton);
        }

        /// <summary>
        /// 슬라이더 손잡이를 저장된 값에 맞춘다.
        ///
        /// 실제 볼륨은 SoundManager가 시작할 때 이미 적용해 두므로 여기서 다시 걸 필요가 없다.
        /// SetValueWithoutNotify를 쓰는 이유: value에 대입하면 onValueChanged가 돌아
        /// 옵션 창을 여는 것만으로 슬라이더 효과음이 난다.
        /// </summary>
        private void SyncSliders()
        {
            _sliderBGM.SetValueWithoutNotify(DataManager.Instance.data.volumeBgm);
            _sliderSFX.SetValueWithoutNotify(DataManager.Instance.data.volumeSfx);
        }

        public override void Show()
        {
            EnsureBound();
            SyncSliders();

            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        public override void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void OnClickSaveButton()
        {
            DataManager.Instance.SaveGameOptionData();
            Hide();
        }

        private void SetVolumeBGM(float value)
        {
            DataManager.Instance.data.volumeBgm = value;
            SoundManager.Instance.SetVolumeBGM(value);
        }

        private void SetVolumeSFX(float value)
        {
            DataManager.Instance.data.volumeSfx = value;
            SoundManager.Instance.SetVolumeSFX(value);
        }
    }
}
