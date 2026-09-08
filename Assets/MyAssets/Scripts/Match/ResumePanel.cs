using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    /// <summary>대전 중 메뉴에서 여는 이어하기 대화상자.</summary>
    public sealed class ResumePanel : UIPanel
    {
        [SerializeField] private GameObject _resumePanel;

        [Header("Buttons")]
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _selectModeButton;
        [SerializeField] private Button _continueButton;

        // events
        public event Action OnReplay;
        public event Action OnSelect;

        // 클릭 소리는 UIBinder가 붙여준다. 핸들러는 하는 일만 적는다.
        protected override void Bind()
        {
            UIBinder.Bind(_replayButton, OnClickReplayButton, this, nameof(_replayButton));
            UIBinder.Bind(_selectModeButton, OnClickSelectModeButton, this, nameof(_selectModeButton));
            UIBinder.Bind(_continueButton, Hide, this, nameof(_continueButton));
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_replayButton);
            UIBinder.Unbind(_selectModeButton);
            UIBinder.Unbind(_continueButton);

            OnReplay = null;
            OnSelect = null;
        }

        public override void Show()
        {
            EnsureBound();
            _resumePanel.SetActive(true);
        }

        public override void Hide() => _resumePanel.SetActive(false);

        // 버튼 핸들러는 private으로 둔다. 코드에서만 연결하므로 공개할 이유가 없고,
        // private이면 인스펙터에 남아 있는 옛 바인딩이 이 메소드를 찾지 못해
        // (UnityEvent는 public 인스턴스 메소드만 호출한다) 이중 실행이 생기지 않는다.
        private void OnClickReplayButton() => RequestReplay();

        private void OnClickSelectModeButton() => RequestSelectMode();

        /// <summary>결과 패널의 다시하기 버튼도 같은 흐름을 탄다.</summary>
        public void RequestReplay()
        {
            OnReplay?.Invoke();
            Hide();
        }

        /// <summary>결과 패널의 모드선택 버튼도 같은 흐름을 탄다.</summary>
        public void RequestSelectMode()
        {
            OnSelect?.Invoke();
            Hide();
        }
    }
}
