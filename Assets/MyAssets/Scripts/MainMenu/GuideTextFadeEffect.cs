using System.Collections;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class GuideTextFadeEffect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _guideText;

        private float _fadeTime = 1f;

        private void Start()
        {
            StartCoroutine(FadeEffect());
        }

        private IEnumerator FadeEffect()
        {
            while (gameObject.activeSelf)
            {
                yield return StartCoroutine(FadeEffectUI.FadeOutTextMeshPro(_guideText, _fadeTime));
                yield return StartCoroutine(FadeEffectUI.FadeInTextMeshPro(_guideText, _fadeTime));
            }
        }
    }
}
