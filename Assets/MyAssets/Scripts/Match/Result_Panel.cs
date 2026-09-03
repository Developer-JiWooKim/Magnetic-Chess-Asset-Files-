using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class Result_Panel : UIPanel
    {
        [SerializeField]
        private TextMeshProUGUI winPlayerText;
        [SerializeField]
        private TextMeshProUGUI endTurnText;

        private ResumePanel resumePanel;
        private ExitPanel exitPanel;

        private void Start()
        {
            Setup();
        }
        public void Setup()
        {
            resumePanel = FindObjectOfType<ResumePanel>(true);
            exitPanel = FindObjectOfType<ExitPanel>(true);
        }
        public void Result_Initialize(string _winPlayer, int endTurn)
        {
            winPlayerText.text = "WINNER - " + _winPlayer.ToUpper();
            endTurnText.text = "End Turn - " + endTurn.ToString();
        }
        public void OnClickReplayButton()
        {
            if (resumePanel != null)
            {
                resumePanel.OnClickReplayButton();
            }
        }
        public void OnClickSelectModeButton()
        {
            if (resumePanel != null)
            {
                resumePanel.OnClickSelectModeButton();
            }
        }
        public void OnClickQuitButton()
        {
            if (exitPanel != null)
            {
                exitPanel.Show();
            }
        }
    }
}
