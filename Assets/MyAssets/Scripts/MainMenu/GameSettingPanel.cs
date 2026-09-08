using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    /// <summary>
    /// 조각 수 · 대기 시간 · 최대 턴을 고르고 판을 시작하는 패널.
    ///
    /// 예전에는 이 패널이 씬 로드까지 직접 시켰고, DontDestroyOnLoad 캔버스에 있어
    /// 대전 씬에서도 살아남는 탓에 GameScene 전용 경로(OnClickGamePlayButtonGameScene)를
    /// 따로 들고 있었다. 지금은 씬마다 UI가 따로라 그 두 번째 경로가 필요 없다.
    /// </summary>
    public sealed class GameSettingPanel : UIPanel
    {
        [SerializeField] private GameObject _pieceCountAIOption;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _backButton;

        private Coroutine _runtimeCoroutine = null;

        private const float FADE_TIME = 0.2f;

        /// <summary>시작 버튼이 눌렸다.</summary>
        public event Action PlayRequested;

        /// <summary>뒤로 버튼이 눌렸다.</summary>
        public event Action BackRequested;

        protected override void Bind()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            UIBinder.Bind(_playButton, OnClickPlayButton, this, nameof(_playButton));
            UIBinder.Bind(_backButton, OnClickBackButton, this, nameof(_backButton));
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_playButton);
            UIBinder.Unbind(_backButton);

            PlayRequested = null;
            BackRequested = null;
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

            ModeAISetting();
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

            _pieceCountAIOption.SetActive(false);
            gameObject.SetActive(false);
        }

        /// <summary>AI 모드일 때만 AI 조각 수 항목을 보여준다.</summary>
        private void ModeAISetting()
        {
            _pieceCountAIOption.SetActive(GameManager.Instance.CurrentSetting.gameMode == GameMode.AI);
        }

        private void OnClickPlayButton() => PlayRequested?.Invoke();

        private void OnClickBackButton() => BackRequested?.Invoke();
    }
}
