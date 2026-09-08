using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.MainMenu
{
    /// <summary>모드 버튼들을 한데 묶어, 어느 것이 눌렸든 하나의 이벤트로 내보낸다.</summary>
    public sealed class ModeButtons : MonoBehaviour
    {
        private ModeBase[] _buttons;

        public event Action<GameMode> ModeSelected;

        private void Awake() => Setup();

        private void Setup()
        {
            _buttons = GetComponentsInChildren<ModeBase>(true);

            if (_buttons.Length == 0)
            {
                Debug.LogError(nameof(ModeButtons) + ": 자식에서 모드 버튼을 찾지 못했다.", this);
                return;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Setup();
                _buttons[i].PreparingMode();
                _buttons[i].Selected += OnModeSelected;
            }
        }

        private void OnDestroy()
        {
            if (_buttons != null)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    if (_buttons[i] != null)
                    {
                        _buttons[i].Selected -= OnModeSelected;
                    }
                }
            }

            ModeSelected = null;
        }

        private void OnModeSelected(GameMode mode) => ModeSelected?.Invoke(mode);
    }
}
