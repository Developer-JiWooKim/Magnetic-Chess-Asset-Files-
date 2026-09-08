using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.Match;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class GameSettingPanel : PanelBase
    {
        [SerializeField] private GameObject _pieceCountAIOption;
        [SerializeField] private GameObject _fadeWindow;
        [SerializeField] private Animator _animatorCamera;
        [SerializeField] private CanvasGroup _canvasGroup;

        private Coroutine _runtimeCoroutine = null;

        private const float FADE_TIME = 0.2f;

        private void Start() => Setup();
        private void Setup()
        {
            panelName = UIPanelName.GameSetting;
        }

        private void ModeAISetting()
        {
            if (GameManager.Instance.CurrentSetting.gameMode == GameMode.AI)
            {
                _pieceCountAIOption.SetActive(true);
            }
            else
            {
                _pieceCountAIOption.SetActive(false);
            }
        }
        private void AsyncLoadScene()
        {
            GameManager.Instance.AsyncLoadGameScene();
        }
        public override void Show()
        {
            if (gameObject.activeSelf == true)
            {
                return;
            }
            if (_runtimeCoroutine != null)
            {
                StopCoroutine(_runtimeCoroutine);
            }

            gameObject.SetActive(true);

            ModeAISetting();
            _runtimeCoroutine = StartCoroutine(FadeEffectUI.FadeInCanvasGroup(_canvasGroup, FADE_TIME, () => _runtimeCoroutine = null));
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

            _pieceCountAIOption.SetActive(false);
            gameObject.SetActive(false);
        }

        public void OnClickGamePlayButton()
        {
            if (_animatorCamera != null)
            {
                _animatorCamera.SetTrigger("PlayStart");
            }

            StartCoroutine(FadeEffectUI.FadeOutCanvasGroup(_canvasGroup, 0.1f));
            _fadeWindow.SetActive(true);
            StartCoroutine(FadeEffectUI.FadeInCanvasGroup(_fadeWindow.GetComponent<CanvasGroup>(), 1.5f, AsyncLoadScene));
        }

        public void OnClickGamePlayButtonGameScene()
        {
            // 이 패널은 DontDestroyOnLoad 캔버스에 있어 TitleScene에서도 살아있으므로,
            // GameScene에서만 존재하는 GameDirector는 클릭 시점에 조회한다.
            GameDirector director = GameDirector.Instance;

            if (director != null)
            {
                director.Setup();
            }
            else
            {
                Debug.Log("GameDirector is null!");
            }
        }
    }
}
