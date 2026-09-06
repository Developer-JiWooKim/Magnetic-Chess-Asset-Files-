using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    public sealed class MenuList : MonoBehaviour
    {
        // consts
        private const float TITLE_SPACING_HorizontalLayoutGroup = -80f;
        private const float GAME_SPACING_HorizontalLayoutGroup = 20f;

        [SerializeField] private bool isShowButton = false;

        private List<UIPanel> buttons;
        private HorizontalLayoutGroup horizontalLayoutGroup;

        private void Start() => Setup();
        private void Setup()
        {
            buttons = GetComponentsInChildren<UIPanel>(true).ToList();

            horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = TITLE_SPACING_HorizontalLayoutGroup;

            GameManager.Instance.ChangeSceneAction += ChangeLayoutGroupSpacing;
        }

        private void ChangeLayoutGroupSpacing()
        {
            bool istitle = DontDestroy_Menu.Instance.CurrentScene == DontDestroy_Menu.SceneName.Title;
            horizontalLayoutGroup.spacing = istitle ?
                    TITLE_SPACING_HorizontalLayoutGroup : GAME_SPACING_HorizontalLayoutGroup;
        }

        public void OnClickListButton()
        {
            if (isShowButton)
            {
                foreach (UIPanel btn in buttons)
                {
                    btn.Hide();
                }

                isShowButton = false;
            }
            else
            {
                foreach (UIPanel btn in buttons)
                {
                    btn.Show();
                }
                isShowButton = true;
            }
        }
    }
}
