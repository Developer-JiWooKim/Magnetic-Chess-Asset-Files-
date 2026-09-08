using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class ResultPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI _winPlayerText;
        [SerializeField] private TextMeshProUGUI _endTurnText;

        private ResumePanel _resumePanel;
        private ExitPanel _exitPanel;

        private void Start()
        {
            Setup();
        }
        public void Setup()
        {
            _resumePanel = FindObjectOfType<ResumePanel>(true);
            _exitPanel = FindObjectOfType<ExitPanel>(true);
        }
        public void ResultInitialize(string winPlayer, int endTurn)
        {
            _winPlayerText.text = "WINNER - " + winPlayer.ToUpper();
            _endTurnText.text = "End Turn - " + endTurn.ToString();
        }
        public void OnClickReplayButton()
        {
            if (_resumePanel != null)
            {
                _resumePanel.OnClickReplayButton();
            }
        }
        public void OnClickSelectModeButton()
        {
            if (_resumePanel != null)
            {
                _resumePanel.OnClickSelectModeButton();
            }
        }
        public void OnClickQuitButton()
        {
            if (_exitPanel != null)
            {
                _exitPanel.Show();
            }
        }
    }
}
