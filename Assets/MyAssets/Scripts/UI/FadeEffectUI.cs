using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.Scripts.UI
{
    public static class FadeEffectUI
    {
        public static float fadeTime = 0.5f;
        public static IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float fadeInTime = 0.5f, Action action = null)
        {
            float currentTime = 0.0f;
            float percent = 0.0f;

            canvasGroup.alpha = 0.0f;
            canvasGroup.blocksRaycasts = false;

            while (percent < 1f)
            {
                currentTime += Time.deltaTime;
                percent = currentTime / fadeInTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, percent);
                yield return null;
            }
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            if (action != null)
            {
                action();
            }
        }
        public static IEnumerator FadeInTextMeshPro(TextMeshProUGUI text, float fadeInTime = 0.5f, Action action = null)
        {
            float currentTime = 0.0f;
            float percent = 0.0f;

            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);

            while (percent < 1f)
            {

                currentTime += Time.deltaTime;
                percent = currentTime / fadeInTime;
                Color color = text.color;
                color.a = Mathf.Lerp(0, 1, percent);
                text.color = color;
                yield return null;
            }
            if (action != null)
            {
                action();
            }
        }
        public static IEnumerator FadeOutCanvasGroup(CanvasGroup canvasGroup, float fadeOutTime = 0.5f, Action action = null)
        {
            float currentTime = 0.0f;
            float percent = 0.0f;

            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = false;

            while (percent < 1f)
            {
                currentTime += Time.deltaTime;
                percent = currentTime / fadeOutTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, percent);

                yield return null;
            }
            if (action != null)
            {
                action();
            }

            canvasGroup.blocksRaycasts = true;
        }
        public static IEnumerator FadeOutTextMeshPro(TextMeshProUGUI text, float fadeOutTime = 0.5f, Action action = null)
        {
            float currentTime = 0.0f;
            float percent = 0.0f;

            text.color = new Color(text.color.r, text.color.g, text.color.b, 1);

            while (percent < 1f)
            {
                currentTime += Time.deltaTime;
                percent = currentTime / fadeOutTime;
                Color color = text.color;
                color.a = Mathf.Lerp(1, 0, percent);
                text.color = color;
                yield return null;
            }
            if (action != null)
            {
                action();
            }
        }
    }
}
