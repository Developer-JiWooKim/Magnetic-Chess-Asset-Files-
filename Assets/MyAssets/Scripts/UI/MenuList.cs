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
        private const float TITLE_SPACING = -80f;
        private const float GAME_SPACING = 20f;

        [SerializeField] private bool _isShowButton = false;

        private List<UIPanel> _buttons;
        private HorizontalLayoutGroup _horizontalLayoutGroup;

        private void Start() => Setup();
        private void Setup()
        {
            _buttons = GetComponentsInChildren<UIPanel>(true).ToList();

            _horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            _horizontalLayoutGroup.spacing = TITLE_SPACING;

            GameManager.Instance.ChangeSceneAction += ChangeLayoutGroupSpacing;
        }

        private void ChangeLayoutGroupSpacing()
        {
            bool istitle = DontDestroyMenu.Instance.CurrentScene == DontDestroyMenu.SceneName.Title;
            _horizontalLayoutGroup.spacing = istitle ?
                    TITLE_SPACING : GAME_SPACING;
        }

        public void OnClickListButton()
        {
            if (_isShowButton)
            {
                foreach (UIPanel btn in _buttons)
                {
                    btn.Hide();
                }

                _isShowButton = false;
            }
            else
            {
                foreach (UIPanel btn in _buttons)
                {
                    btn.Show();
                }
                _isShowButton = true;
            }
        }
    }
}
