using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.MainMenu
{
    /// <summary>
    /// 모드 선택 버튼의 공통 동작. 파생 클래스는 "어느 모드인가"와
    /// "아직 준비 중인가" 두 가지만 말하면 된다.
    ///
    /// 예전에는 눌렸을 때 상위에서 MenuManager를 찾아 화면 전환까지 직접 시켰다.
    /// 지금은 "이 모드가 골렸다"만 알리고, 그 다음은 TitleUIController가 정한다.
    /// </summary>
    public abstract class ModeBase : MonoBehaviour
    {
        /// <summary>
        /// 준비 중 안내 표시. 파생 세 클래스가 각자 들고 있던 필드를 여기로 올렸다.
        /// 이름을 그대로 둬야 인스펙터에 연결해 둔 값이 보존된다.
        /// </summary>
        [SerializeField] private GameObject _preparing;

        protected bool isPreparing;
        public bool IsPreparing => isPreparing;

        private Button _button;

        /// <summary>이 버튼이 골라진 모드를 알린다. ModeButtons가 구독한다.</summary>
        public event Action<GameMode> Selected;

        /// <summary>이 버튼이 고르는 대전 모드.</summary>
        protected abstract GameMode Mode { get; }

        /// <summary>아직 만들지 않은 모드인가.</summary>
        protected abstract bool PreparingByDefault { get; }

        private void Awake()
        {
            // 버튼은 이 오브젝트에 있다. 타입으로 찾으므로 인스펙터에 연결할 것이 없고,
            // 따라서 빠뜨릴 것도 없다.
            _button = GetComponent<Button>();

            UIBinder.Bind(_button, OnClickModeButton, this, nameof(_button));
        }

        private void OnDestroy()
        {
            UIBinder.Unbind(_button);
            Selected = null;
        }

        /// <summary>ModeButtons가 시작할 때 호출한다.</summary>
        public void Setup()
        {
            isPreparing = PreparingByDefault;
        }

        /// <summary>준비 중인 모드는 안내를 띄우고 버튼을 잠근다.</summary>
        public void PreparingMode()
        {
            if (_preparing != null)
            {
                _preparing.SetActive(isPreparing);
            }

            if (_button != null)
            {
                _button.interactable = isPreparing == false;
            }
        }

        private void OnClickModeButton() => Selected?.Invoke(Mode);
    }
}
