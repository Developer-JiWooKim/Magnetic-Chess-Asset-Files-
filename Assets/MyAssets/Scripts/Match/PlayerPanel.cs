using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class PlayerPanel : PlayerPanelBase
    {
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _chessPieceText;
        [SerializeField] private TextMeshProUGUI _playerTimerText;
        [SerializeField] private Image _faceBorderImage;

        [SerializeField] private PlayerName _playerName;
        public PlayerName OwnerName => _playerName;

        // consts
        private const float ALPHA_OPAQUE = 1f;
        private const float ALPHA_DIMMED = .4f;
        private const float ALPHA_TRANSPARENT = 0f;

        /// <summary>
        /// 패널 자신의 CanvasGroup / Image. 턴이 바뀔 때마다 GetComponent를 부르지 않도록 캐싱한다.
        /// 패널은 비활성 상태로도 조회되는데(Awake가 아직 안 돌았을 수 있다) 그때도 안전하도록
        /// 접근 시점에 한 번 더 확인한다.
        /// </summary>
        private CanvasGroup _canvasGroup;
        private Image _panelImage;

        public CanvasGroup PanelCanvasGroup
        {
            get
            {
                CacheComponents();
                return _canvasGroup;
            }
        }

        /// <summary>
        /// 타이머 텍스트는 매 프레임 갱신되므로, 표시값이 실제로 바뀌었을 때만 TMP에 쓴다.
        /// 마지막으로 표시한 값(1/100초 단위 정수)을 들고 비교한다.
        /// </summary>
        private int _lastDisplayedTimerValue = int.MinValue;

        private void Awake() => CacheComponents();

        private void CacheComponents()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            if (_panelImage == null)
            {
                _panelImage = GetComponent<Image>();
            }
        }

        public override void InitializePanel()
        {
            SetName();
            UpdatePieceCount(0);

            // 판이 새로 시작될 때는 캐시와 무관하게 반드시 다시 그린다.
            _lastDisplayedTimerValue = int.MinValue;
            UpdateTimer(0);
        }

        public void SetName()
        {
            bool isAIMode = GameManager.Instance.CurrentSetting.gameMode == GameMode.AI;
            switch (_playerName)
            {
                case PlayerName.Player1:
                    _playerName = PlayerName.Player1;
                    _playerNameText.text = "Player 1";
                    break;
                case PlayerName.Player2:
                    _playerName = isAIMode == true ? PlayerName.PlayerAI : PlayerName.Player2;
                    _playerNameText.text = isAIMode == true ? "AI" : "Player 2";
                    break;
                case PlayerName.PlayerAI:
                    _playerName = isAIMode == true ? PlayerName.PlayerAI : PlayerName.Player2;
                    _playerNameText.text = isAIMode == true ? "AI" : "Player 2";
                    break;
            }
        }
        public override void UpdatePieceCount(int count)
        {
            _chessPieceText.text = "Piece : " + count.ToString();
        }
        public override void UpdateTimer(float time)
        {
            // 소수점 2자리까지만 보여주므로, 그 아래 변화로는 다시 그릴 필요가 없다.
            int timerValue = Mathf.RoundToInt(time * 100f);
            if (timerValue == _lastDisplayedTimerValue)
            {
                return;
            }
            _lastDisplayedTimerValue = timerValue;

            // TMP의 SetText(format, arg)는 내부 문자 버퍼에 직접 쓰므로 string 할당이 없다.
            // 기존 string.Format("{00:N2}", time)은 매 프레임 문자열 + 박싱을 만들어냈다.
            _playerTimerText.SetText("{0:2}", time);
        }

        /// <summary>
        /// 현재 턴 여부에 따른 패널 강조. 예전에는 InGameUIManager가 GetComponent로
        /// 패널 내부(CanvasGroup·Image)를 직접 만졌는데, 자기 컴포넌트는 자기가 알고 있는 편이 낫다.
        /// </summary>
        public void SetTurnHighlight(bool isCurrentTurn)
        {
            CacheComponents();

            _canvasGroup.alpha = isCurrentTurn ? ALPHA_OPAQUE : ALPHA_DIMMED;

            Color color = _panelImage.color;
            color.a = isCurrentTurn ? ALPHA_OPAQUE : ALPHA_TRANSPARENT;
            _panelImage.color = color;

            if (isCurrentTurn)
            {
                CurrentTurnFaceOn();
            }
            else
            {
                CurrentTurnFaceOff();
            }
        }
        public void CurrentTurnFaceOn()
        {
            Color color = _faceBorderImage.color;
            color.a = 1.0f;
            _faceBorderImage.color = color;
        }
        public void CurrentTurnFaceOff()
        {
            Color color = _faceBorderImage.color;
            color.a = 0.0f;
            _faceBorderImage.color = color;
        }
    }
}
