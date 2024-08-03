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
    /// Ÿ��Ʋ ȭ���� ��ġ�ϸ� �ش� ������Ʈ�� ���̵� �ƿ� ȿ��, ī�޶� ������ �����ϴ� �Լ�
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
