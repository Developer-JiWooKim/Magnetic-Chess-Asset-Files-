using System.Collections;
using UnityEngine;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class TabletLogic : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingUI;

        [SerializeField] private GameObject _selectUI;

        [SerializeField] private GameObject _settingUI;

        [SerializeField] private ModeSelectPanel _modeSelectPanel;

        private IEnumerator SwitchingSelectAfterLoading()
        {
            yield return new WaitForSeconds(1.5f);
            _loadingUI.SetActive(false);
            _selectUI.SetActive(true);
            _modeSelectPanel.Show();
        }

        public void SwitchingSelectToSetting()
        {
            _selectUI.SetActive(false);
            _settingUI.SetActive(true);
        }

        public void SwitchingSettingToSelect()
        {
            _settingUI.SetActive(false);
            _selectUI.SetActive(true);
        }

        public void TabletLogicStart()
        {
            _loadingUI.SetActive(true);
            StartCoroutine(SwitchingSelectAfterLoading());
        }
    }
}
