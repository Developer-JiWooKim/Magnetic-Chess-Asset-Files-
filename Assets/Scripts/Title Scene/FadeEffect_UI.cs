using System;
using System.Collections;
using TMPro;
using UnityEngine;

public static class FadeEffect_UI
{
    public static float fadeTime = 0.5f;
    public static IEnumerator FadeIn_CanvasGroup(CanvasGroup canvasGroup, float _FadeInTime = 0.5f, Action action = null)
    {
        Debug.Log(canvasGroup != null);
        float currentTime = 0.0f;
        float percent = 0.0f;

        canvasGroup.alpha = 0.0f;
        canvasGroup.blocksRaycasts = false;

        Debug.Log("1-3");
        while (percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / _FadeInTime;
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
    public static IEnumerator FadeIn_TextMeshPro(TextMeshProUGUI text, float _FadeInTime = 0.5f, Action action = null)
    {
        float currentTime = 0.0f;
        float percent = 0.0f;

        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);

        while (percent < 1f)
        {
        
            currentTime += Time.deltaTime;
            percent = currentTime / _FadeInTime;
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
    public static IEnumerator FadeOut_CanvasGroup(CanvasGroup canvasGroup, float _FadeOutTime = 0.5f, Action action = null)
    {
        float currentTime = 0.0f;
        float percent = 0.0f;

        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = false;

        while (percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / _FadeOutTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, percent);

            yield return null;
        }
        if (action != null)
        {
            action();
        }

        canvasGroup.blocksRaycasts = true;
    }
    public static IEnumerator FadeOut_TextMeshPro(TextMeshProUGUI text, float _FadeOutTime = 0.5f, Action action = null)
    {
        float currentTime = 0.0f;
        float percent = 0.0f;

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);

        while (percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / _FadeOutTime;
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