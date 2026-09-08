using System;
using UnityEngine;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class StartPanel : PanelBase
    {
        [SerializeField] private Animator _animatorCamera;

        private CanvasGroup _canvasGroup;

        private void Awake() => Setup();

        private void Setup()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            panelName = UIPanelName.Start;
        }

        public void OnClickStartButton() => Hide();

        public override void Hide()
        {
            // MenuManager.ChangePanel은 이미 숨겨진 패널에도 Hide를 부른다.
            // 비활성 오브젝트에서는 코루틴을 시작할 수 없어 에러 로그만 남으므로 먼저 걸러낸다.
            // (ModeSelectPanel · GameSettingPanel도 같은 가드를 갖고 있다.)
            if (gameObject.activeSelf == false)
            {
                return;
            }

            Action action = () =>
            {
                _animatorCamera.SetTrigger("MoveStart");
                gameObject.SetActive(false);
            };
            StartCoroutine(FadeEffectUI.FadeOutCanvasGroup(_canvasGroup, FadeEffectUI.fadeTime, action));
        }
    }
}
