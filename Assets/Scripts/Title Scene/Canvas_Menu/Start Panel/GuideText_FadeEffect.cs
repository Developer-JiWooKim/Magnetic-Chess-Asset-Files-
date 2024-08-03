using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuideText_FadeEffect : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI guideText;

    private float fadeTime = 1f;

    private void Start()
    {
        StartCoroutine(FadeEffect());
    }

    private IEnumerator FadeEffect()
    {
        while(gameObject.activeSelf)
        {
            yield return StartCoroutine(FadeEffect_UI.FadeOut_TextMeshPro(guideText, fadeTime));
            yield return StartCoroutine(FadeEffect_UI.FadeIn_TextMeshPro(guideText, fadeTime));
        }
    }

    
}
