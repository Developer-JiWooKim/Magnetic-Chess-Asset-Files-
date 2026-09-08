using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class ModeSelectPanel : UIPanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ModeButtons _modeButtons;

        private Coroutine _runtimeCoroutine = null;

        private const float FADE_TIME = 0.2f;

        /// <summary>어느 모드가 골라졌는지 알린다. 화면 전환은 TitleUIController가 한다.</summary>
        public event Action<GameMode> ModeSelected;

        protected override void Bind() => Setup();

        private void Setup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _modeButtons.ModeSelected += OnModeSelected;
        }

        protected override void Unbind()
        {
            if (_modeButtons != null)
            {
                _modeButtons.ModeSelected -= OnModeSelected;
            }

            ModeSelected = null;
        }

        public override void Show()
        {
            EnsureBound();

            if (gameObject.activeSelf)
            {
                return;
            }
            if (_runtimeCoroutine != null)
            {
                StopCoroutine(_runtimeCoroutine);
            }
            gameObject.SetActive(true);
            _runtimeCoroutine = StartCoroutine(FadeEffectUI.FadeInCanvasGroup(_canvasGroup, FADE_TIME,
                () => _runtimeCoroutine = null));
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
                _runtimeCoroutine = null;
            }
            gameObject.SetActive(false);
        }

        private void OnModeSelected(GameMode mode) => ModeSelected?.Invoke(mode);
    }
}
