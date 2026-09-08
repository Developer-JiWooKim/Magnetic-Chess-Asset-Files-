using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.MyAssets.Scripts.AI;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.Magnet;
using Assets.MyAssets.Scripts.UI;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class GameDirector : Singleton<GameDirector>
    {
        [SerializeField] private MagnetBallSpawner _magnetBallSpawner;
        [SerializeField] private MagnetWorld _magnetWorld;
        [SerializeField] private CameraView _cameraView;
        [SerializeField] private InGameUIManager _inGameUIManager;
        [SerializeField] private ResultPanel _resultPanel;
        [SerializeField] private AIFSM _aiFSM;
        [SerializeField] private GameObject _preventImage;

        private List<Player> _playerList = new();

        protected override bool IsPersistent => false;

        // consts
        private const int PLAYER_1_INDEX = 0;
        private const int PLAYER_2_INDEX = 1;
        private const int TURN_INFINITY = 100;
        private const float SPAWN_POINT_Y = 0.7f;

        private readonly TurnStateMachine _turnState = new TurnStateMachine();
        private bool _isTouch;

        private readonly MatchTimer _matchTimer = new MatchTimer();

        /// <summary>
        /// 자석볼 충돌처럼 결과가 아직 확정되지 않았을 때 대기 시간을 늘린다(MagnetContact에서 호출).
        /// </summary>
        public void ExtendConfirmTime(float seconds) => _matchTimer.ExtendTime(seconds);

        private GameSetting _currentSetting;
        public bool isPlaying { get; private set; }

        /// <summary>
        /// 터치할 때마다 Camera.main 조회와 LayerMask.NameToLayer 문자열 조회를 반복하지 않도록
        /// 한 번만 구해 둔다. 결과 패널 CanvasGroup도 판이 끝날 때마다 찾을 이유가 없다.
        /// </summary>
        private Camera _mainCamera;
        private int _spawnRaycastLayerMask;
        private CanvasGroup _resultPanelCanvasGroup;

        /// <summary>
        /// 캐싱해 두되, 어떤 이유로든 비어 있으면 그때 한 번 더 찾는다.
        /// (예전에는 클릭할 때마다 Camera.main을 조회했다.)
        /// </summary>
        private Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }

        // Singleton<T>.Awake가 인스턴스 등록과 중복 제거를 하므로 반드시 먼저 호출한다.
        protected override void Awake()
        {
            base.Awake();

            _mainCamera = Camera.main;
            _spawnRaycastLayerMask = (-1) - (1 << LayerMask.NameToLayer("SpawnPoint"));
            _resultPanelCanvasGroup = _resultPanel.GetComponent<CanvasGroup>();
        }

        private void Start() => Setup();

        private void Update()
        {
            PlayerTouchScreen();
        }

        private void GameFSM()
        {
            switch (_turnState.Current)
            {
                case GameState.None:
                    BattleStart();
                    break;
                case GameState.Player1:
                    _inGameUIManager.CurrentTurnPlayerPanelEffect(_playerList[_turnState.CurrentPlayerIndex].playerName);
                    break;
                case GameState.Player2:
                    _inGameUIManager.CurrentTurnPlayerPanelEffect(_playerList[_turnState.CurrentPlayerIndex].playerName);

                    if (_currentSetting.gameMode == GameMode.AI)
                    {
                        _isTouch = true;

                        Invoke(nameof(AISpawnAndStartTimer), 0.5f);
                    }
                    break;
                case GameState.End:
                    EndBattle();
                    break;
            }
        }
        private void BattleStart()
        {
            _preventImage.SetActive(true);
            _turnState.BeginFirstTurn();

            _cameraView.ChangeCameraView(_cameraView.TopView, () => _preventImage.SetActive(false));
            GameFSM();
        }
        private void PlayerTouchScreen()
        {
            if (isPlaying == false)
            {
                return;
            }

            if (_isTouch == true)
            {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _isTouch = true;
                SpawnAndStartTimer();
            }
        }
        private IEnumerator StartTimer()
        {
            _matchTimer.Begin(_currentSetting.waitingTime);

            int playerIndex = _turnState.CurrentPlayerIndex;

            while (!_matchTimer.IsFinished)
            {
                _matchTimer.Tick(Time.deltaTime);
                _inGameUIManager.UpdateUIWaitingTimeText(_matchTimer.DisplayTime, _playerList[playerIndex].playerName);
                yield return null;
            }

            _magnetWorld.IsActive = false;


            bool isContact = IncreasePieceCount() > 0;


            _inGameUIManager.UpdateUIChessPieceText(_playerList[playerIndex].PieceCount, _playerList[playerIndex].playerName);


            if (isContact)
            {
                // 반납하면 목록이 줄어들므로 뒤에서부터 순회한다.
                IReadOnlyList<MagnetContact> activeMagnetContacts = _magnetBallSpawner.ActiveMagnetContacts;
                for (int i = activeMagnetContacts.Count - 1; i >= 0; i--)
                {
                    MagnetContact magnetContact = activeMagnetContacts[i];
                    if (magnetContact.IsContact)
                    {
                        _magnetBallSpawner.DeactivateMagnetBall(magnetContact.gameObject);
                    }
                }
            }


            bool someoneEmptiedPieces = _playerList.Find(player => player.PieceCount <= 0) != null;

            bool maxTurnReached = _currentSetting.maxTurn < TURN_INFINITY
                                  && _turnState.IsMaxTurnReached(_currentSetting.maxTurn);

            // 조각을 다 털어낸 사람이 승리한다. 조각 수는 자기 턴에만 변하므로(놓으면 -1, 붙으면 +N)
            // 0이 된 사람은 항상 현재 턴 플레이어다.
            if (someoneEmptiedPieces)
            {
                isPlaying = false;
                _turnState.FinishWith(_turnState.Current);
                GameFSM();
            }
            // 최대 턴까지 아무도 못 털어냈으면 남은 조각이 더 적은 쪽이 승리한다.
            else if (maxTurnReached)
            {
                isPlaying = false;
                _turnState.FinishWith(DecideWinnerByFewestPieces());
                GameFSM();
            }

            else
            {
                if (_turnState.Current == GameState.Player2)
                {
                    _turnState.IncreaseTurnCount();
                    _inGameUIManager.UpdateUITurnText(_turnState.TurnCount);
                }

                _turnState.ChangeTurn();

                _isTouch = false;

                SoundManager.Instance.PlaySFX(SoundManager.SfxName.ChangeTurn);

                GameFSM();
            }
        }
        private void SpawnAndStartTimer()
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);

            bool isHit = Physics.Raycast(ray, out RaycastHit hit, 100f, _spawnRaycastLayerMask);


            if (isHit && hit.collider.CompareTag("Board"))
            {
                Vector3 hitPos = hit.point;
                hitPos.y = SPAWN_POINT_Y;

                _magnetWorld.IsActive = true;

                SoundManager.Instance.PlaySFX(SoundManager.SfxName.MagnetBallSpawn);
                _magnetBallSpawner.SpawnMagnetBall(hitPos, Random.rotation);

                CurrentTurnPieceDecrease();

                int playerIndex = _turnState.CurrentPlayerIndex;

                _inGameUIManager.UpdateUIChessPieceText(_playerList[playerIndex].PieceCount, _playerList[playerIndex].playerName);
            }
            else
            {
                _isTouch = false;
                return;
            }

            StartCoroutine(StartTimer());
        }

        private void AISpawnAndStartTimer()
        {
            Vector3 aiSpawnPoint = _aiFSM.AIMagnetBallSpawnPoint();
            aiSpawnPoint.y = SPAWN_POINT_Y;

            _magnetWorld.IsActive = true;

            SoundManager.Instance.PlaySFX(SoundManager.SfxName.MagnetBallSpawn);
            _magnetBallSpawner.SpawnMagnetBall(aiSpawnPoint, Random.rotation);

            CurrentTurnPieceDecrease();

            int playerIndex = _turnState.CurrentPlayerIndex;

            _inGameUIManager.UpdateUIChessPieceText(_playerList[playerIndex].PieceCount, _playerList[playerIndex].playerName);

            StartCoroutine(StartTimer());
        }

        private void CurrentTurnPieceDecrease()
        {
            if (_turnState.IsPlayerTurn == false)
            {
                return;
            }
            _playerList[_turnState.CurrentPlayerIndex].PieceCount--;
        }
        private void EndBattle()
        {
            StopAllCoroutines();

            _magnetBallSpawner.DeactivateAllMagnetBall();


            _inGameUIManager.HideAllPanel();
            _inGameUIManager.HideTurnText();

            _resultPanel.Show();


            _resultPanel.ResultInitialize(GetWinnerDisplayName(), _turnState.TurnCount);

            StartCoroutine(FadeEffectUI.FadeInCanvasGroup(_resultPanelCanvasGroup, .3f));
        }

        private void InitializeGameSettings()
        {
            // GameSetting은 struct라 여기서 값이 복사된다. 즉 이 시점 이후 GameManager 쪽 설정이
            // 바뀌어도 진행 중인 판에는 반영되지 않는다(판 시작 시점의 설정으로 끝까지 진행).
            _currentSetting = GameManager.Instance.CurrentSetting;

            if (_playerList != null)
            {
                _playerList.Clear();
            }

            int totalPieceCount = 0;

            switch (_currentSetting.gameMode)
            {
                case GameMode.OfflineMulti:
                    _playerList.Add(new Player(PlayerName.Player1));
                    _playerList.Add(new Player(PlayerName.Player2));
                    _playerList.ForEach(player => totalPieceCount += player.PieceCount = _currentSetting.pieceCount);

                    _magnetBallSpawner.InstantiateMagnetBall(totalPieceCount);
                    break;

                case GameMode.AI:
                    _playerList.Add(new Player(PlayerName.Player1));
                    _playerList.Add(new Player(PlayerName.PlayerAI));
                    totalPieceCount += _playerList.Find(player => player.playerName == PlayerName.Player1).PieceCount = _currentSetting.pieceCount;
                    totalPieceCount += _playerList.Find(player => player.playerName == PlayerName.PlayerAI).PieceCount = _currentSetting.pieceCountAI;

                    _magnetBallSpawner.InstantiateMagnetBall(totalPieceCount);
                    break;

                case GameMode.OnlineMulti:
                    break;
            }
        }

        /// <summary>
        /// 최대 턴까지 승부가 나지 않았을 때의 승자. 남은 조각이 더 적은 쪽이 이기고, 같으면 무승부다.
        /// </summary>
        private GameState DecideWinnerByFewestPieces()
        {
            int player1Pieces = _playerList[PLAYER_1_INDEX].PieceCount;
            int player2Pieces = _playerList[PLAYER_2_INDEX].PieceCount;

            if (player1Pieces == player2Pieces)
            {
                return GameState.None;
            }
            return player1Pieces < player2Pieces ? GameState.Player1 : GameState.Player2;
        }

        /// <summary>결과 화면에 표시할 승자 이름. AI 모드에서 Player2는 "AI"로 보여준다.</summary>
        private string GetWinnerDisplayName()
        {
            if (_turnState.WinPlayer == GameState.None)
            {
                return "DRAW";
            }
            if (_currentSetting.gameMode == GameMode.AI && _turnState.WinPlayer == GameState.Player2)
            {
                return "AI";
            }
            return _turnState.WinPlayer.ToString();
        }

        private int IncreasePieceCount()
        {
            int contactMagnetBallCount = 0;

            IReadOnlyList<MagnetContact> activeMagnetContacts = _magnetBallSpawner.ActiveMagnetContacts;
            for (int i = 0; i < activeMagnetContacts.Count; i++)
            {
                if (activeMagnetContacts[i].IsContact)
                {
                    contactMagnetBallCount++;
                }
            }

            if (_turnState.IsPlayerTurn)
            {
                _playerList[_turnState.CurrentPlayerIndex].PieceCount += contactMagnetBallCount;
            }

            return contactMagnetBallCount;
        }

        public void GamePlay()
        {
            isPlaying = true;
            _preventImage.SetActive(true);
            _cameraView.ChangeCameraView(_cameraView.TopView,
                () => _preventImage.SetActive(false));


            _inGameUIManager.ShowAllPanel();
            _inGameUIManager.UpdateUITurnText(_turnState.TurnCount + 1);
            if (_playerList != null)
            {
                _playerList.ForEach(player =>
                    _inGameUIManager.UpdateUIChessPieceText(player.PieceCount, player.playerName));
                _playerList.ForEach(player => _inGameUIManager.UpdateUIWaitingTimeText(0, player.playerName));
            }
            else
            {
                Debug.Log("_playerList is null!!");
            }


            _inGameUIManager.CurrentTurnPlayerPanelEffect(_playerList[PLAYER_1_INDEX].playerName);
            _inGameUIManager.CurrentTurnPlayerPanelFadeInEffect();


            GameFSM();
        }
        public void Setup()
        {

            _preventImage.SetActive(true);

            _turnState.Reset();

            isPlaying = false;
            _isTouch = false;

            _magnetWorld.IsActive = false;

            InitializeGameSettings();

            _magnetBallSpawner.DeactivateAllMagnetBall();

            if (_currentSetting.gameMode == GameMode.AI)
            {
                _aiFSM.SpawnPointInitialize();
            }

            _inGameUIManager.ShowAllPanel();
            _inGameUIManager.InitializeUI();
            _inGameUIManager.HideAllPanel();
            _inGameUIManager.HideTurnText();

            _resultPanel.Hide();

            _cameraView.ChangeCameraView(_cameraView.QuarterView,
                () => _preventImage.SetActive(false));
        }

        public void StopGame()
        {
            StopAllCoroutines();
            isPlaying = false;
            _turnState.Stop();
        }
    }
}
