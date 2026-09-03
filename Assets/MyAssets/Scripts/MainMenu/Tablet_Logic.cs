using System.Collections;
using UnityEngine;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class Tablet_Logic : MonoBehaviour
    {
        [SerializeField] private GameObject loading_UI;

        [SerializeField] private GameObject select_UI;

        [SerializeField] private GameObject setting_UI;

        [SerializeField] private ModeSelectPanel modeSelectPanel;

        private IEnumerator Switching_Select_after_loading()
        {
            yield return new WaitForSeconds(1.5f);
            loading_UI.SetActive(false);
            select_UI.SetActive(true);
            modeSelectPanel.Show();
        }

        public void Switching_Select_to_Setting()
        {
            select_UI.SetActive(false);
            setting_UI.SetActive(true);
        }

        public void Switching_Setting_to_Select()
        {
            setting_UI.SetActive(false);
            select_UI.SetActive(true);
        }

        public void Tablet_Logic_Start()
        {
            loading_UI.SetActive(true);
            StartCoroutine(Switching_Select_after_loading());
        }
    }
}
