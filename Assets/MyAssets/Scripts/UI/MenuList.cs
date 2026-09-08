using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 메뉴 막대 안에 늘어놓는 버튼들.
    ///
    /// 예전에는 자식에서 UIPanel을 긁어모아 Show/Hide를 불렀다. 그래서 "목록을 여는 버튼"만은
    /// 숨으면 안 된다는 예외를 ListButton이라는 빈 오버라이드 클래스로 표현해야 했고,
    /// 간격은 DontDestroyMenu.CurrentScene으로 분기했다.
    /// 지금은 무엇을 토글할지 이 컴포넌트가 목록으로 들고 있으므로 예외 클래스가 필요 없다.
    /// </summary>
    public sealed class MenuList : MonoBehaviour
    {
        /// <summary>펼칠 때 함께 나타날 항목들. 목록을 여는 버튼 자신은 넣지 않는다.</summary>
        [SerializeField] private List<GameObject> _items = new();

        /// <summary>항목 간격. 타이틀은 -80, 대전 씬은 20을 쓴다.</summary>
        [SerializeField] private float _spacing = -80f;

        private HorizontalLayoutGroup _horizontalLayoutGroup;

        /// <summary>이 씬에서 쓰지 않는 항목(예: 타이틀의 Resume). 펼쳐도 나타나지 않는다.</summary>
        private readonly HashSet<GameObject> _unavailable = new();

        private void Awake() => Setup();

        private void Setup()
        {
            _horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();

            if (_horizontalLayoutGroup != null)
            {
                _horizontalLayoutGroup.spacing = _spacing;
            }

            SetVisible(false);
        }

        /// <summary>이 씬에서 그 항목을 쓸 수 있는지 정한다. CommonMenu가 부른다.</summary>
        public void SetAvailable(GameObject item, bool available)
        {
            if (item == null)
            {
                return;
            }

            if (available)
            {
                _unavailable.Remove(item);
            }
            else
            {
                _unavailable.Add(item);
                item.SetActive(false);
            }
        }

        public void SetVisible(bool visible)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                GameObject item = _items[i];

                if (item == null)
                {
                    Debug.LogError(nameof(MenuList) + "._items[" + i + "] 가 비어 있다.", this);
                    continue;
                }

                item.SetActive(visible && _unavailable.Contains(item) == false);
            }
        }
    }
}
