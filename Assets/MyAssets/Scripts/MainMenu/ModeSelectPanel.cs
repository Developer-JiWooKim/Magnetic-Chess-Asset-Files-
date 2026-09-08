using UnityEngine;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class ModeSelectPanel : PanelBase
    {
        private CanvasGroup _canvasGroup;
        private Coroutine _runtimeCoroutine = null;

        private const float FADE_TIME = 0.2f;

        private void Awake() => Setup();

        private void Setup()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            panelName = UIPanelName.ModeSelect;
        }

        public override void Show()
        {
            if (gameObject.activeSelf)
            {
                return;
            }
            if (_runtimeCoroutine != null)
            {
                StopCoroutine(_runtimeCoroutine);
            }
            gameObject.SetActive(true);
            _runtimeCoroutine = StartCoroutine(FadeEffectUI.FadeInCanvasGroup(_canvasGroup, FADE_TIME));
        }
        public override void Hide()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            if (_runtimeCoroutine != null)
            {
                StopCoroutine(_runtimeCoroutine);
            }
            gameObject.SetActive(false);
        }
    }
}