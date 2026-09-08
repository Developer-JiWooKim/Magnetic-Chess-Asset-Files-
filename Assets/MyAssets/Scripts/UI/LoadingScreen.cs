using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 씬을 떠날 때 화면을 덮고 진행도를 보여준다.
    ///
    /// 예전에는 이 창이 DontDestroyOnLoad 캔버스에 있어서 다음 씬까지 따라간 뒤 거기서 걷혔다.
    /// 지금은 씬과 함께 파괴되므로, 걷어내는 쪽은 도착하는 씬의 SceneFadeIn이 맡는다.
    /// 덮는 씬과 걷는 씬이 다르다는 것만 지키면 된다.
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _percent;
        [SerializeField] private TextMeshProUGUI _percentText;

        /// <summary>화면을 덮는 데 걸리는 시간.</summary>
        [SerializeField] private float _fadeInTime = 1.5f;

        private bool _isSubscribed;

        private void Awake()
        {
            _root.SetActive(false);
        }

        private void OnDestroy() => Unsubscribe();

        /// <summary>
        /// 화면을 덮고, 다 덮이면 onCovered를 부른다. 씬 로드는 그때 시작한다.
        /// (먼저 로드를 걸면 프레임이 튀어 페이드가 끊긴다.)
        /// </summary>
        public void Cover(Action onCovered)
        {
            _root.SetActive(true);
            _percent.SetActive(true);
            SetPercent(0f);

            Subscribe();

            StartCoroutine(FadeEffectUI.FadeInCanvasGroup(_canvasGroup, _fadeInTime, onCovered));
        }

        private void Subscribe()
        {
            if (_isSubscribed)
            {
                return;
            }

            GameManager.Instance.LoadProgressChanged += SetPercent;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            // 이 오브젝트가 파괴될 때는 씬이 넘어가는 중이라 GameManager는 아직 살아 있다.
            // 그래도 종료 순서에 기대지 않도록 확인한다.
            GameManager manager = GameManager.Instance;
            if (manager != null)
            {
                manager.LoadProgressChanged -= SetPercent;
            }

            _isSubscribed = false;
        }

        private void SetPercent(float progress)
        {
            if (_percentText == null)
            {
                return;
            }

            _percentText.text = "Loading...\n" + Mathf.RoundToInt(progress * 100f) + "%";
        }
    }
}
