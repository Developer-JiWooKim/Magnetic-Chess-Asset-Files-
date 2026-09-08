using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    /// <summary>
    /// 판이 끝났을 때의 결과 화면.
    ///
    /// 예전에는 다시하기 · 모드선택 · 종료를 처리하려고 ResumePanel과 ExitPanel을
    /// FindObjectOfType으로 찾았다. 두 패널이 다른 씬(DontDestroyOnLoad 캔버스)에 있어
    /// 인스펙터로 연결할 수 없었기 때문이다.
    /// 지금은 무엇을 하려는지만 알리고, 누가 그것을 처리하는지는 MatchUIController가 안다.
    /// </summary>
    public sealed class ResultPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI _winPlayerText;
        [SerializeField] private TextMeshProUGUI _endTurnText;

        [Header("Buttons")]
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _selectModeButton;
        [SerializeField] private Button _quitButton;

        public event Action ReplayRequested;
        public event Action SelectModeRequested;
        public event Action QuitRequested;

        // 클릭 소리는 UIBinder가 붙여준다. 핸들러는 하는 일만 적는다.
        protected override void Bind()
        {
            UIBinder.Bind(_replayButton, OnClickReplayButton, this, nameof(_replayButton));
            UIBinder.Bind(_selectModeButton, OnClickSelectModeButton, this, nameof(_selectModeButton));
            UIBinder.Bind(_quitButton, OnClickQuitButton, this, nameof(_quitButton));
        }

        protected override void Unbind()
        {
            UIBinder.Unbind(_replayButton);
            UIBinder.Unbind(_selectModeButton);
            UIBinder.Unbind(_quitButton);

            ReplayRequested = null;
            SelectModeRequested = null;
            QuitRequested = null;
        }

        public void ResultInitialize(string winPlayer, int endTurn)
        {
            _winPlayerText.text = "WINNER - " + winPlayer.ToUpper();
            _endTurnText.text = "End Turn - " + endTurn.ToString();
        }

        private void OnClickReplayButton() => ReplayRequested?.Invoke();

        private void OnClickSelectModeButton() => SelectModeRequested?.Invoke();

        private void OnClickQuitButton() => QuitRequested?.Invoke();
    }
}
