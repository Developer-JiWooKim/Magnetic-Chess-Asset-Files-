using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class InGameUI_Manager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnText;

        [SerializeField] private Character_FaceCam faceCam;

        private Player_Panel[] player_Panels;

        // 페이드인이 끝난 뒤 현재 턴이 아닌 패널을 되돌릴 투명도.
        // 나머지 강조 처리는 Player_Panel.SetTurnHighlight가 들고 있다.
        private const float __ALPHA_VALUE_0_4 = .4f;

        /// <summary>
        /// PlayerName으로 패널을 찾기 위한 표. 예전에는 매 프레임 List.Find(람다)로 훑었는데,
        /// 람다가 인자를 캡처해서 호출마다 클로저가 할당됐다.
        /// 패널의 PlayerName은 SetName()에서 바뀌므로(Player_2 ↔ Player_AI) 그 뒤에 반드시 다시 만든다.
        /// </summary>
        private readonly Dictionary<PlayerName, Player_Panel> panelsByPlayerName = new();

        private void Awake()
        {
            Setup();
        }
        private void Setup()
        {
            player_Panels = GetComponentsInChildren<Player_Panel>(true);
            RebuildPanelLookup();
        }

        private void RebuildPanelLookup()
        {
            panelsByPlayerName.Clear();

            if (player_Panels == null)
            {
                return;
            }

            for (int i = 0; i < player_Panels.Length; i++)
            {
                PlayerName key = player_Panels[i].Player_Panel_playerName;

                // 예전 List.Find는 같은 PlayerName이 둘이면 앞의 것을 썼다. 표는 뒤의 것으로 덮어쓰므로
                // 그런 상황이 생기면 조용히 넘어가지 않도록 알린다(현재 씬은 패널이 2개라 발생하지 않음).
                if (panelsByPlayerName.ContainsKey(key))
                {
                    Debug.LogWarning("Player_Panel의 PlayerName이 중복됩니다: " + key);
                }

                panelsByPlayerName[key] = player_Panels[i];
            }
        }

        private Player_Panel FindPanel(PlayerName _playerName)
        {
            panelsByPlayerName.TryGetValue(_playerName, out Player_Panel panel);
            return panel;
        }

        public void Show_All_Panel()
        {
            SetAllPanelsActive(true);
        }

        public void Hide_All_Panel()
        {
            SetAllPanelsActive(false);
        }

        private void SetAllPanelsActive(bool isActive)
        {
            if (player_Panels == null)
            {
                Debug.Log("Player_Panel is null!");
                return;
            }

            for (int i = 0; i < player_Panels.Length; i++)
            {
                player_Panels[i].gameObject.SetActive(isActive);
            }
        }

        public void UpdateUI_ChessPiece_Text(int _count, PlayerName _currPlayer)
        {
            Player_Panel panel = FindPanel(_currPlayer);
            if (panel != null)
            {
                panel.Update_PieceCount(_count);
            }
        }

        public void UpdateUI_WaitingTime_Text(float _time, PlayerName _currPlayer)
        {
            Player_Panel panel = FindPanel(_currPlayer);
            if (panel != null)
            {
                panel.Update_Timer(_time);
            }
        }


        public void Initialize_UI()
        {
            if (player_Panels != null)
            {
                for (int i = 0; i < player_Panels.Length; i++)
                {
                    player_Panels[i].Initiallize_Panel();
                }

                // Initiallize_Panel 안의 SetName()이 PlayerName을 바꿀 수 있으므로(Player_2 ↔ Player_AI)
                // 조회 표는 반드시 그 뒤에 다시 만든다.
                RebuildPanelLookup();
            }

            if (faceCam != null)
            {
                faceCam.Initialize();
            }
            else
            {
                Debug.Log("faceCam is null!!");
            }
        }
        public void CurrentTurnPlayer_Panel_Effect(PlayerName _currPlayer)
        {
            for (int i = 0; i < player_Panels.Length; i++)
            {
                Player_Panel panel = player_Panels[i];
                panel.SetTurnHighlight(panel.Player_Panel_playerName == _currPlayer);
            }
        }
        public void CurrentTurnPlayer_Panel_FadeIn_Effect()
        {
            for (int i = 0; i < player_Panels.Length; i++)
            {
                Player_Panel panel = player_Panels[i];
                CanvasGroup canvasGroup = panel.PanelCanvasGroup;

                StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(canvasGroup, 0.3f,
                    () =>
                    {
                        if (panel.Player_Panel_playerName != PlayerName.Player_1)
                        {
                            canvasGroup.alpha = __ALPHA_VALUE_0_4;
                        }
                    }));
            }
        }
        public void UpdateUI_TurnText(int _turnCount)
        {
            if (GameManager.Instance.CurrentSetting.maxTurn > 100)
            {
                turnText.text = "Max Turn [ Infinity ]\n<size=80>Turn [ " + _turnCount.ToString() + " ]";
            }
            else
            {
                turnText.text = "Max Turn [ " + GameManager.Instance.CurrentSetting.maxTurn.ToString() + " ]\n<size=80>Turn [ " + _turnCount.ToString() + " ]";
            }
        }
        public void Hide_TurnText()
        {
            turnText.text = "";
        }
    }
}
