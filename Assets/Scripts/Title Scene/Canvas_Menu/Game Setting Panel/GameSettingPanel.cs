using System;
using UnityEngine;


public class GameSettingPanel : Panel_Base
{
    [SerializeField]
    private GameObject PieceCount_AI_Option;
    [SerializeField]
    private GameObject fadeWindow;
    [SerializeField]
    private Animator animator_Camera;
    [SerializeField]
    private CanvasGroup canvasGroup;

    private GameDirector gameDirector;
    private Coroutine runtimeCoroutine = null;

    private const float __FADE_TIME = 0.2f;

    private void Start()
    {
        Setup();
    }
    private void Setup()
    {
        panel_Name = E_UI_Panel_Name.GameSetting;
        gameDirector = FindObjectOfType<GameDirector>();
        if ( gameDirector == null )
        {
            Debug.Log("gameDirector is null");
        }
    }
    /// <summary>
    /// Show Panel할 때 게임 매니저의 게임모드가 AI면 해당 오브젝트 활성화 아니면 비활성화하는 함수
    /// </summary>
    private void ModeAI_Setting()
    {
        if (GameManager.Instance.gameSetting.gameMode == GameMode.AI)
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
    public override void ShowPanel()
    {
        if (gameObject.activeSelf == true)
        {
            return;
        }
        if (runtimeCoroutine != null)
        {
            StopCoroutine(runtimeCoroutine);
        }

        // 게임 세팅 UI 활성화
        gameObject.SetActive(true);

        // AI모드면 해당 UI 활성화 아니면 비활성화
        ModeAI_Setting();
        runtimeCoroutine = StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(canvasGroup, __FADE_TIME, () => runtimeCoroutine = null));
    }
    public override void HidePanel()
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
    /// <summary>
    /// 씬 전환 연출
    /// 로딩 화면 보이게, 게임 씬으로 비동기 전환
    /// </summary>
    public void OnClickGamePlayButton()
    {
        if (animator_Camera != null)
        {
            // 게임 플레이 애니메이션 작동
            animator_Camera.SetTrigger("PlayStart");
        }
      
        StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(canvasGroup, 0.1f));
        fadeWindow.SetActive(true);
        StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(fadeWindow.GetComponent<CanvasGroup>(), 1.5f, AsyncLoadScene));
    }
    /// <summary>
    /// 게임 초기화 함수(게임 씬에서 사용되는 함수)
    /// </summary>
    public void OnClickGamePlayButton_GameScene()
    {
        if (gameDirector != null)
        {
            gameDirector.Setup();
        }
        else
        {
            Debug.Log("GameDirector is null!");
        }
    }
}
