using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class Player_Panel : Player_Panel_Base
    {
        [SerializeField] private TextMeshProUGUI playerName_Text;
        [SerializeField] private TextMeshProUGUI chessPiece_Text;
        [SerializeField] private TextMeshProUGUI playerTimer_Text;
        [SerializeField] private Image faceBorder_Image;

        [SerializeField] private PlayerName playerName;
        public PlayerName Player_Panel_playerName => playerName;

        // consts
        private const float __ALPHA_VALUE_1 = 1f;
        private const float __ALPHA_VALUE_0_4 = .4f;
        private const float __ALPHA_VALUE_0 = 0f;

        /// <summary>
        /// 패널 자신의 CanvasGroup / Image. 턴이 바뀔 때마다 GetComponent를 부르지 않도록 캐싱한다.
        /// 패널은 비활성 상태로도 조회되는데(Awake가 아직 안 돌았을 수 있다) 그때도 안전하도록
        /// 접근 시점에 한 번 더 확인한다.
        /// </summary>
        private CanvasGroup canvasGroup;
        private Image panel_Image;

        public CanvasGroup PanelCanvasGroup
        {
            get
            {
                CacheComponents();
                return canvasGroup;
            }
        }

        /// <summary>
        /// 타이머 텍스트는 매 프레임 갱신되므로, 표시값이 실제로 바뀌었을 때만 TMP에 쓴다.
        /// 마지막으로 표시한 값(1/100초 단위 정수)을 들고 비교한다.
        /// </summary>
        private int lastDisplayedTimerValue = int.MinValue;

        private void Awake() => CacheComponents();

        private void CacheComponents()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if (panel_Image == null)
            {
                panel_Image = GetComponent<Image>();
            }
        }

        public override void Initiallize_Panel()
        {
            SetName();
            Update_PieceCount(0);

            // 판이 새로 시작될 때는 캐시와 무관하게 반드시 다시 그린다.
            lastDisplayedTimerValue = int.MinValue;
            Update_Timer(0);
        }

        public void SetName()
        {
            bool isAIMode = GameManager.Instance.CurrentSetting.gameMode == GameMode.AI;
            switch (playerName)
            {
                case PlayerName.Player_1:
                    playerName = PlayerName.Player_1;
                    playerName_Text.text = "Player 1";
                    break;
                case PlayerName.Player_2:
                    playerName = isAIMode == true ? PlayerName.Player_AI : PlayerName.Player_2;
                    playerName_Text.text = isAIMode == true ? "AI" : "Player 2";
                    break;
                case PlayerName.Player_AI:
                    playerName = isAIMode == true ? PlayerName.Player_AI : PlayerName.Player_2;
                    playerName_Text.text = isAIMode == true ? "AI" : "Player 2";
                    break;
            }
        }
        public override void Update_PieceCount(int _count)
        {
            chessPiece_Text.text = "Piece : " + _count.ToString();
        }
        public override void Update_Timer(float _time)
        {
            // 소수점 2자리까지만 보여주므로, 그 아래 변화로는 다시 그릴 필요가 없다.
            int timerValue = Mathf.RoundToInt(_time * 100f);
            if (timerValue == lastDisplayedTimerValue)
            {
                return;
            }
            lastDisplayedTimerValue = timerValue;

            // TMP의 SetText(format, arg)는 내부 문자 버퍼에 직접 쓰므로 string 할당이 없다.
            // 기존 string.Format("{00:N2}", _time)은 매 프레임 문자열 + 박싱을 만들어냈다.
            playerTimer_Text.SetText("{0:2}", _time);
        }

        /// <summary>
        /// 현재 턴 여부에 따른 패널 강조. 예전에는 InGameUI_Manager가 GetComponent로
        /// 패널 내부(CanvasGroup·Image)를 직접 만졌는데, 자기 컴포넌트는 자기가 알고 있는 편이 낫다.
        /// </summary>
        public void SetTurnHighlight(bool isCurrentTurn)
        {
            CacheComponents();

            canvasGroup.alpha = isCurrentTurn ? __ALPHA_VALUE_1 : __ALPHA_VALUE_0_4;

            Color color = panel_Image.color;
            color.a = isCurrentTurn ? __ALPHA_VALUE_1 : __ALPHA_VALUE_0;
            panel_Image.color = color;

            if (isCurrentTurn)
            {
                CurrentTurn_FaceOn();
            }
            else
            {
                CurrentTurn_FaceOff();
            }
        }
        public void CurrentTurn_FaceOn()
        {
            Color color = faceBorder_Image.color;
            color.a = 1.0f;
            faceBorder_Image.color = color;
        }
        public void CurrentTurn_FaceOff()
        {
            Color color = faceBorder_Image.color;
            color.a = 0.0f;
            faceBorder_Image.color = color;
        }
    }
}
