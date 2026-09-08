using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class InGameUIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _turnText;

        [SerializeField] private CharacterFaceCam _faceCam;

        private PlayerPanel[] _playerPanels;

        // 페이드인이 끝난 뒤 현재 턴이 아닌 패널을 되돌릴 투명도.
        // 나머지 강조 처리는 PlayerPanel.SetTurnHighlight가 들고 있다.
        private const float ALPHA_DIMMED = .4f;

        /// <summary>
        /// PlayerName으로 패널을 찾기 위한 표. 예전에는 매 프레임 List.Find(람다)로 훑었는데,
        /// 람다가 인자를 캡처해서 호출마다 클로저가 할당됐다.
        /// 패널의 PlayerName은 SetName()에서 바뀌므로(Player2 ↔ PlayerAI) 그 뒤에 반드시 다시 만든다.
        /// </summary>
        private readonly Dictionary<PlayerName, PlayerPanel> _panelsByPlayerName = new();

        private void Awake()
        {
            Setup();
        }
        private void Setup()
        {
            _playerPanels = GetComponentsInChildren<PlayerPanel>(true);
            RebuildPanelLookup();
        }

        private void RebuildPanelLookup()
        {
            _panelsByPlayerName.Clear();

            if (_playerPanels == null)
            {
                return;
            }

            for (int i = 0; i < _playerPanels.Length; i++)
            {
                PlayerName key = _playerPanels[i].OwnerName;

                // 예전 List.Find는 같은 PlayerName이 둘이면 앞의 것을 썼다. 표는 뒤의 것으로 덮어쓰므로
                // 그런 상황이 생기면 조용히 넘어가지 않도록 알린다(현재 씬은 패널이 2개라 발생하지 않음).
                if (_panelsByPlayerName.ContainsKey(key))
                {
                    Debug.LogWarning("PlayerPanel의 PlayerName이 중복됩니다: " + key);
                }

                _panelsByPlayerName[key] = _playerPanels[i];
            }
        }

        private PlayerPanel FindPanel(PlayerName playerName)
        {
            _panelsByPlayerName.TryGetValue(playerName, out PlayerPanel panel);
            return panel;
        }

        public void ShowAllPanel()
        {
            SetAllPanelsActive(true);
        }

        public void HideAllPanel()
        {
            SetAllPanelsActive(false);
        }

        private void SetAllPanelsActive(bool isActive)
        {
            if (_playerPanels == null)
            {
                Debug.Log("PlayerPanel is null!");
                return;
            }

            for (int i = 0; i < _playerPanels.Length; i++)
            {
                _playerPanels[i].gameObject.SetActive(isActive);
            }
        }

        public void UpdateUIChessPieceText(int count, PlayerName currentPlayer)
        {
            PlayerPanel panel = FindPanel(currentPlayer);
            if (panel != null)
            {
                panel.UpdatePieceCount(count);
            }
        }

        public void UpdateUIWaitingTimeText(float time, PlayerName currentPlayer)
        {
            PlayerPanel panel = FindPanel(currentPlayer);
            if (panel != null)
            {
                panel.UpdateTimer(time);
            }
        }


        public void InitializeUI()
        {
            if (_playerPanels != null)
            {
                for (int i = 0; i < _playerPanels.Length; i++)
                {
                    _playerPanels[i].InitializePanel();
                }

                // InitializePanel 안의 SetName()이 PlayerName을 바꿀 수 있으므로(Player2 ↔ PlayerAI)
                // 조회 표는 반드시 그 뒤에 다시 만든다.
                RebuildPanelLookup();
            }

            if (_faceCam != null)
            {
                _faceCam.Initialize();
            }
            else
            {
                Debug.Log("_faceCam is null!!");
            }
        }
        public void CurrentTurnPlayerPanelEffect(PlayerName currentPlayer)
        {
            for (int i = 0; i < _playerPanels.Length; i++)
            {
                PlayerPanel panel = _playerPanels[i];
                panel.SetTurnHighlight(panel.OwnerName == currentPlayer);
            }
        }
        public void CurrentTurnPlayerPanelFadeInEffect()
        {
            for (int i = 0; i < _playerPanels.Length; i++)
            {
                PlayerPanel panel = _playerPanels[i];
                CanvasGroup canvasGroup = panel.PanelCanvasGroup;

                StartCoroutine(FadeEffectUI.FadeInCanvasGroup(canvasGroup, 0.3f,
                    () =>
                    {
                        if (panel.OwnerName != PlayerName.Player1)
                        {
                            canvasGroup.alpha = ALPHA_DIMMED;
                        }
                    }));
            }
        }
        public void UpdateUITurnText(int turnCount)
        {
            if (GameManager.Instance.CurrentSetting.maxTurn > 100)
            {
                _turnText.text = "Max Turn [ Infinity ]\n<size=80>Turn [ " + turnCount.ToString() + " ]";
            }
            else
            {
                _turnText.text = "Max Turn [ " + GameManager.Instance.CurrentSetting.maxTurn.ToString() + " ]\n<size=80>Turn [ " + turnCount.ToString() + " ]";
            }
        }
        public void HideTurnText()
        {
            _turnText.text = "";
        }
    }
}
