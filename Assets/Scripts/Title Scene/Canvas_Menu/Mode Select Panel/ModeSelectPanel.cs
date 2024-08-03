using UnityEngine;

public class ModeSelectPanel : Panel_Base
{
    private CanvasGroup canvasGroup;
    private Coroutine runtimeCoroutine = null;

    private const float __FADE_TIME = 0.2f;

    private void Awake()
    {
        Setup();
    }
    private void Setup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        panel_Name = E_UI_Panel_Name.ModeSelect;
    }
    public override void ShowPanel()
    {
        if (gameObject.activeSelf)
        {
            return;
        }
        if (runtimeCoroutine != null)
        {
            StopCoroutine(runtimeCoroutine);
        }
        gameObject.SetActive(true);
        runtimeCoroutine = StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(canvasGroup, __FADE_TIME));
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
        gameObject.SetActive(false);
    }
}
