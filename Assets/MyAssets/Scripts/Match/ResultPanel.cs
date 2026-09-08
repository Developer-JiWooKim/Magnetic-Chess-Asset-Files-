using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class ResultPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI _winPlayerText;
        [SerializeField] private TextMeshProUGUI _endTurnText;

        [Header("Buttons")]
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _selectModeButton;
        [SerializeField] private Button _quitButton;

        private ResumePanel _resumePanel;
        private ExitPanel _exitPanel;

        // 클릭 소리는 UIBinder가 붙여준다. 핸들러는 하는 일만 적는다.
        private void Awake()
        {
            UIBinder.Bind(_replayButton, OnClickReplayButton, this, nameof(_replayButton));
            UIBinder.Bind(_selectModeButton, OnClickSelectModeButton, this, nameof(_selectModeButton));
            UIBinder.Bind(_quitButton, OnClickQuitButton, this, nameof(_quitButton));
        }

        private void OnDestroy()
        {
            UIBinder.Unbind(_replayButton);
            UIBinder.Unbind(_selectModeButton);
            UIBinder.Unbind(_quitButton);
        }

        private void Start()
        {
            Setup();
        }

        public void Setup()
        {
            // ResumePanel · ExitPanel은 DontDestroyOnLoad 메뉴 캔버스에 있어 이 씬의
            // 인스펙터로는 연결할 수 없다. UI를 씬별로 분리하면(L5) 이 조회는 사라진다.
            _resumePanel = FindObjectOfType<ResumePanel>(true);
            _exitPanel = FindObjectOfType<ExitPanel>(true);
        }

        public void ResultInitialize(string winPlayer, int endTurn)
        {
            _winPlayerText.text = "WINNER - " + winPlayer.ToUpper();
            _endTurnText.text = "End Turn - " + endTurn.ToString();
        }

        private void OnClickReplayButton()
        {
            if (_resumePanel != null)
            {
                _resumePanel.RequestReplay();
            }
        }

        private void OnClickSelectModeButton()
        {
            if (_resumePanel != null)
            {
                _resumePanel.RequestSelectMode();
            }
        }

        private void OnClickQuitButton()
        {
            if (_exitPanel != null)
            {
                _exitPanel.Show();
            }
        }
    }
}
