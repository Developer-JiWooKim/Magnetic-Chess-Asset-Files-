using System;
using UnityEngine;

public class StartPanel : Panel_Base
{
    [SerializeField]
    private Animator animator_Camera;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Setup();
    }
    private void Setup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        panel_Name = E_UI_Panel_Name.Start;
    }
    /// <summary>
    /// 타이틀 화면을 터치하면 해당 오브젝트를 페이드 아웃 효과, 카메라 무빙을 시작하는 함수
    /// </summary>
    public void OnClickStartButton()
    {
        Action action = () => {
            animator_Camera.SetTrigger("MoveStart");
            gameObject.SetActive(false);
        };
        StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(canvasGroup, FadeEffect_UI.fadeTime, action));
    }
    public override void ShowPanel() { }
    public override void HidePanel() { }
}
