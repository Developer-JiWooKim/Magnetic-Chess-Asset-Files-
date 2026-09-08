using UnityEngine;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 씬에 들어올 때 덮여 있던 화면을 걷어낸다. 떠나는 씬의 LoadingScreen과 짝이다.
    /// 씬을 다시 읽어도 같은 방식으로 걷히므로 재시작에도 그대로 쓰인다.
    /// </summary>
    public sealed class SceneFadeIn : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeOutTime = 1f;

        private void Awake()
        {
            // 씬이 보이기 전에 덮어 둔다. Start까지 기다리면 한 프레임이 새어 나온다.
            _root.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        private void Start()
        {
            StartCoroutine(FadeEffectUI.FadeOutCanvasGroup(_canvasGroup, _fadeOutTime,
                () => _root.SetActive(false)));
        }
    }
}
