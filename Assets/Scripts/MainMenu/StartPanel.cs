using System;
using UnityEngine;
using Assets.Scripts.UI;

namespace Assets.Scripts.MainMenu
{
    public sealed class StartPanel : Panel_Base
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

        public void OnClickStartButton() => Hide();

        public override void Hide()
        {
            Action action = () =>
            {
                animator_Camera.SetTrigger("MoveStart");
                gameObject.SetActive(false);
            };
            StartCoroutine(FadeEffect_UI.FadeOut_CanvasGroup(canvasGroup, FadeEffect_UI.fadeTime, action));
        }
    }
}
