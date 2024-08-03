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
    /// Show Panel�� �� ���� �Ŵ����� ���Ӹ�尡 AI�� �ش� ������Ʈ Ȱ��ȭ �ƴϸ� ��Ȱ��ȭ�ϴ� �Լ�
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

        // ���� ���� UI Ȱ��ȭ
        gameObject.SetActive(true);

        // AI���� �ش� UI Ȱ��ȭ �ƴϸ� ��Ȱ��ȭ
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
    /// �� ��ȯ ����
    /// �ε� ȭ�� ���̰�, ���� ������ �񵿱� ��ȯ
    /// </summary>
    public void OnClickGamePlayButton()
    {
        if (animator_Camera != null)
        {
            // ���� �÷��� �ִϸ��̼� �۵�
            animator_Camera.SetTrigger("PlayStart");
        }
      
        StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(canvasGroup, 0.1f));
        fadeWindow.SetActive(true);
        StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(fadeWindow.GetComponent<CanvasGroup>(), 1.5f, AsyncLoadScene));
    }
    /// <summary>
    /// ���� �ʱ�ȭ �Լ�(���� ������ ���Ǵ� �Լ�)
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
