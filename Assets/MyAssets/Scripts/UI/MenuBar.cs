using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 메뉴 목록을 펼치고 접는 가로 막대.
    ///
    /// 예전에는 펼친 너비를 DontDestroyMenu.CurrentScene으로 분기해 정했다(타이틀 365 / 게임 470).
    /// 지금은 CommonMenu 프리팹이 씬마다 따로 배치되므로, 그 값은 인스펙터에 적으면 된다.
    /// </summary>
    public sealed class MenuBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private MenuList _menu;
        [SerializeField] private Button _listButton;

        [Header("Size")]
        [SerializeField] private float _sizeUpSpeed = 1000f;
        [SerializeField] private float _collapsedWidth = 130f;

        /// <summary>펼쳤을 때의 너비. 타이틀은 365, 대전 씬은 470을 쓴다.</summary>
        [SerializeField] private float _expandedWidth = 365f;

        private bool _isShowList;
        private Coroutine _currCor;

        public bool IsExpanded => _isShowList;

        private void Awake()
        {
            UIBinder.Bind(_listButton, Toggle, this, nameof(_listButton),
                SoundManager.SfxName.MenuButtonPress);
        }

        private void OnDestroy()
        {
            UIBinder.Unbind(_listButton);
        }

        private void Start() => Setup();

        private void Setup()
        {
            _isShowList = false;
            SetWidth(_collapsedWidth);
        }

        /// <summary>목록이 펼쳐져 있으면 접는다. 메뉴 항목을 고른 뒤에 부른다.</summary>
        public void Collapse()
        {
            if (_isShowList)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (_currCor != null)
            {
                StopCoroutine(_currCor);
            }

            _currCor = _isShowList
                ? StartCoroutine(DecreaseBar())
                : StartCoroutine(IncreaseBar());
        }

        private IEnumerator IncreaseBar()
        {
            SetWidth(_collapsedWidth);

            while (_background.sizeDelta.x <= _expandedWidth)
            {
                SetWidth(_background.sizeDelta.x + Time.deltaTime * _sizeUpSpeed);
                yield return null;
            }

            SetWidth(_expandedWidth);
            EndAnimation();
            _currCor = null;
        }

        private IEnumerator DecreaseBar()
        {
            SetWidth(_expandedWidth);
            EndAnimation();

            while (_background.sizeDelta.x >= _collapsedWidth)
            {
                SetWidth(_background.sizeDelta.x - Time.deltaTime * _sizeUpSpeed);
                yield return null;
            }

            SetWidth(_collapsedWidth);
            _currCor = null;
        }

        private void SetWidth(float width)
        {
            _background.sizeDelta = new Vector2(width, _background.sizeDelta.y);
        }

        private void EndAnimation()
        {
            _isShowList = !_isShowList;
            _menu.SetVisible(_isShowList);
        }
    }
}
