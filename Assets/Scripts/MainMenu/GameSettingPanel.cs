using System;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Match;
using Assets.Scripts.UI;

namespace Assets.Scripts.MainMenu
{
    public sealed class GameSettingPanel : Panel_Base
    {
        [SerializeField]
        private GameObject PieceCount_AI_Option;
        [SerializeField]
        private GameObject fadeWindow;
        [SerializeField]
        private Animator animator_Camera;
        [SerializeField]
        private CanvasGroup canvasGroup;

        private Coroutine runtimeCoroutine = null;

        private const float __FADE_TIME = 0.2f;

        private void Start()
        {
            Setup();
        }
        private void Setup()
        {
            panel_Name = E_UI_Panel_Name.GameSetting;
        }

        private void ModeAI_Setting()
        {
            if (GameManager.Instance.CurrentSetting.gameMode == GameMode.AI)
            {
                PieceCount_AI_Option.SetActive(true);
            }
            else
            {
                PieceCount_AI_Option.SetActive(false);
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
            if (runtimeCoroutine != null)
            {
                StopCoroutine(runtimeCoroutine);
            }

            gameObject.SetActive(true);

            ModeAI_Setting();
            runtimeCoroutine = StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(canvasGroup, __FADE_TIME, () => runtimeCoroutine = null));
        }
        public override void Hide()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            if (runtimeCoroutine != null)
            {
                StopCoroutine(runtimeCoroutine);
            }

            PieceCount_AI_Option.SetActive(false);
            gameObject.SetActive(false);
        }

        public void OnClickGamePlayButton()
        {
            if (animator_Camera != null)
            {
                animator_Camera.SetTrigger("PlayStart");
            }

            StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(canvasGroup, 0.1f));
            fadeWindow.SetActive(true);
            StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(fadeWindow.GetComponent<CanvasGroup>(), 1.5f, AsyncLoadScene));
        }

        public void OnClickGamePlayButton_GameScene()
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
