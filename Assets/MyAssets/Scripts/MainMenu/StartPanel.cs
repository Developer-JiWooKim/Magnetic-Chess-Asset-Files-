using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class StartPanel : UIPanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _startButton;

        /// <summary>시작 버튼이 눌렸다. 다음에 무엇을 할지는 TitleUIController가 정한다.</summary>
        public event Action StartRequested;

        protected override void Bind()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            UIBinder.Bind(_startButton, OnClickStartButton, this, nameof(_startButton));
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_startButton);
            StartRequested = null;
        }

        public override void Show()
        {
            EnsureBound();
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public override void Hide() => HideAnimated(null);

        /// <summary>
        /// 서서히 사라진 뒤 onComplete를 부른다. 카메라를 언제 움직일지는 컨트롤러가 정하므로
        /// 이 패널은 "다 사라졌다"만 알려준다.
        /// </summary>
        public void HideAnimated(Action onComplete)
        {
            EnsureBound();

            // 비활성 오브젝트에서는 코루틴을 시작할 수 없어 에러 로그만 남으므로 먼저 걸러낸다.
            if (gameObject.activeSelf == false)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(FadeEffectUI.FadeOutCanvasGroup(_canvasGroup, FadeEffectUI.fadeTime,
                () =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                }));
        }

        /// <summary>페이드 없이 즉시 감춘다. 대전에서 모드 선택으로 돌아왔을 때 쓴다.</summary>
        public void HideImmediate()
        {
            EnsureBound();
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnClickStartButton() => StartRequested?.Invoke();
    }
}
