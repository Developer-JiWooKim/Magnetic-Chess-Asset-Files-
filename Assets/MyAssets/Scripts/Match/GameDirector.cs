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
        [SerializeField] private MagnetBallSpawner magnetBallSpawner;
        [SerializeField] private MagnetWorld magnetWorld;
        [SerializeField] private CameraView cameraView;
        [SerializeField] private InGameUI_Manager inGameUI_Manager;
        [SerializeField] private Result_Panel result_Panel;
        [SerializeField] private AI_FSM aiFSM;
        [SerializeField] private GameObject preventImage;

        private List<Player> playerList = new();

        protected override bool IsPersistent => false;

        // consts
        private const int __PLAYER__1 = 0;
        private const int __PLAYER__2 = 1;
        private const int __TURN_INFINITY__ = 100;
        private const float __SPAWN__POINT_Y = 0.7f;

        private readonly TurnStateMachine turnState = new TurnStateMachine();
        private bool isTouch;

        private readonly MatchTimer matchTimer = new MatchTimer();

        /// <summary>
        /// 자석볼 충돌처럼 결과가 아직 확정되지 않았을 때 대기 시간을 늘린다(MagnetContact에서 호출).
        /// </summary>
        public void ExtendConfirmTime(float seconds) => matchTimer.ExtendTime(seconds);

        private GameSetting currentSetting;
        public bool isPlaying { get; private set; }

        private void Start() => Setup();

        private void Update()
        {
            PlayerTouchScreen();
        }

        private void GameFSM()
        {
            switch (turnState.Current)
            {
                case E_GameState.None:
                    BattleStart();
                    break;
                case E_GameState.Player_1:
                    inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[turnState.CurrentPlayerIndex].playerName);
                    break;
                case E_GameState.Player_2:
                    inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[turnState.CurrentPlayerIndex].playerName);

                    if (currentSetting.gameMode == GameMode.AI)
                    {
                        isTouch = true;

                        Invoke(nameof(AI_SpawnAndStartTimer), 0.5f);
                    }
                    break;
                case E_GameState.End:
                    EndBattle();
                    break;
            }
        }
        private void BattleStart()
        {
            preventImage.SetActive(true);
            turnState.BeginFirstTurn();

            cameraView.ChangeCameraView(cameraView.TopView_tr, () => preventImage.SetActive(false));
            GameFSM();
        }
        private void PlayerTouchScreen()
        {
            if (isPlaying == false)
            {
                return;
            }

            if (isTouch == true)
            {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isTouch = true;
                SpawnAndStartTimer();
            }
        }
        private IEnumerator StartTimer()
        {
            matchTimer.Begin(currentSetting.waitingTime);

            int player_index = turnState.CurrentPlayerIndex;

            while (!matchTimer.IsFinished)
            {
                matchTimer.Tick(Time.deltaTime);
                inGameUI_Manager.UpdateUI_WaitingTime_Text(matchTimer.DisplayTime, playerList[player_index].playerName);
                yield return null;
            }

            magnetWorld.IsActive = false;


            bool isContact = IncreasePieceCount() > 0;


            inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);


            if (isContact)
            {
                // 반납하면 ActiveMagnetBalls가 줄어들므로 뒤에서부터 순회한다.
                IReadOnlyList<GameObject> activeMagnetBalls = magnetBallSpawner.ActiveMagnetBalls;
                for (int i = activeMagnetBalls.Count - 1; i >= 0; i--)
                {
                    GameObject magnetBall = activeMagnetBalls[i];
                    if (magnetBall.GetComponent<MagnetContact>().IsContact)
                    {
                        magnetBallSpawner.DeactivateMagnetBall(magnetBall);
                    }
                }
            }


            bool someoneEmptiedPieces = playerList.Find(player => player.PieceCount <= 0) != null;

            bool maxTurnReached = currentSetting.maxTurn < __TURN_INFINITY__
                                  && turnState.IsMaxTurnReached(currentSetting.maxTurn);

            // 조각을 다 털어낸 사람이 승리한다. 조각 수는 자기 턴에만 변하므로(놓으면 -1, 붙으면 +N)
            // 0이 된 사람은 항상 현재 턴 플레이어다.
            if (someoneEmptiedPieces)
            {
                isPlaying = false;
                turnState.FinishWith(turnState.Current);
                GameFSM();
            }
            // 최대 턴까지 아무도 못 털어냈으면 남은 조각이 더 적은 쪽이 승리한다.
            else if (maxTurnReached)
            {
                isPlaying = false;
                turnState.FinishWith(DecideWinnerByFewestPieces());
                GameFSM();
            }

            else
            {
                if (turnState.Current == E_GameState.Player_2)
                {
                    turnState.IncreaseTurnCount();
                    inGameUI_Manager.UpdateUI_TurnText(turnState.TurnCount);
                }

                turnState.ChangeTurn();

                isTouch = false;

                SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.CHANGE_TURN);

                GameFSM();
            }
        }
        private void SpawnAndStartTimer()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = (-1) - (1 << LayerMask.NameToLayer("SpawnPoint"));

            bool isHit = Physics.Raycast(ray, out hit, 100f, layerMask);


            if (isHit && hit.collider.CompareTag("Board"))
            {
                Vector3 hitPos = hit.point;
                hitPos.y = __SPAWN__POINT_Y;

                magnetWorld.IsActive = true;

                SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN);
                magnetBallSpawner.SpawnMagnetBall(hitPos, Random.rotation);

                CurrentTurnPieceDecrease();

                int player_index = turnState.CurrentPlayerIndex;

                inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);
            }
            else
            {
                isTouch = false;
                return;
            }

            StartCoroutine(StartTimer());
        }

        private void AI_SpawnAndStartTimer()
        {
            Vector3 aiSpawnPoint = aiFSM.AIMagnetBallSpawnPoint();
            aiSpawnPoint.y = __SPAWN__POINT_Y;

            magnetWorld.IsActive = true;

            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN);
            magnetBallSpawner.SpawnMagnetBall(aiSpawnPoint, Random.rotation);

            CurrentTurnPieceDecrease();

            int player_index = turnState.CurrentPlayerIndex;

            inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);

            StartCoroutine(StartTimer());
        }

        private void CurrentTurnPieceDecrease()
        {
            if (turnState.IsPlayerTurn == false)
            {
                return;
            }
            playerList[turnState.CurrentPlayerIndex].PieceCount--;
        }
        private void EndBattle()
        {
            StopAllCoroutines();

            magnetBallSpawner.DeactivateAllMagnetBall();


            inGameUI_Manager.Hide_All_Panel();
            inGameUI_Manager.Hide_TurnText();

            result_Panel.Show();


            result_Panel.Result_Initialize(GetWinnerDisplayName(), turnState.TurnCount);

            StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(result_Panel.gameObject.GetComponent<CanvasGroup>(), .3f));
        }

        private void Initialize_GameSettings()
        {
            // GameSetting은 struct라 여기서 값이 복사된다. 즉 이 시점 이후 GameManager 쪽 설정이
            // 바뀌어도 진행 중인 판에는 반영되지 않는다(판 시작 시점의 설정으로 끝까지 진행).
            currentSetting = GameManager.Instance.CurrentSetting;

            if (playerList != null)
            {
                playerList.Clear();
            }

            int totalPieceCount = 0;

            switch (currentSetting.gameMode)
            {
                case GameMode.OfflineMulti:
                    playerList.Add(new Player(PlayerName.Player_1));
                    playerList.Add(new Player(PlayerName.Player_2));
                    playerList.ForEach(player => totalPieceCount += player.PieceCount = currentSetting.pieceCount);

                    magnetBallSpawner.InstantiateMagnetBall(totalPieceCount);
                    break;

                case GameMode.AI:
                    playerList.Add(new Player(PlayerName.Player_1));
                    playerList.Add(new Player(PlayerName.Player_AI));
                    totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_1).PieceCount = currentSetting.pieceCount;
                    totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_AI).PieceCount = currentSetting.pieceCount_AI;

                    magnetBallSpawner.InstantiateMagnetBall(totalPieceCount);
                    break;

                case GameMode.OnlineMulti:
                    break;
            }
        }

        /// <summary>
        /// 최대 턴까지 승부가 나지 않았을 때의 승자. 남은 조각이 더 적은 쪽이 이기고, 같으면 무승부다.
        /// </summary>
        private E_GameState DecideWinnerByFewestPieces()
        {
            int player1Pieces = playerList[__PLAYER__1].PieceCount;
            int player2Pieces = playerList[__PLAYER__2].PieceCount;

            if (player1Pieces == player2Pieces)
            {
                return E_GameState.None;
            }
            return player1Pieces < player2Pieces ? E_GameState.Player_1 : E_GameState.Player_2;
        }

        /// <summary>결과 화면에 표시할 승자 이름. AI 모드에서 Player_2는 "AI"로 보여준다.</summary>
        private string GetWinnerDisplayName()
        {
            if (turnState.WinPlayer == E_GameState.None)
            {
                return "DRAW";
            }
            if (currentSetting.gameMode == GameMode.AI && turnState.WinPlayer == E_GameState.Player_2)
            {
                return "AI";
            }
            return turnState.WinPlayer.ToString();
        }

        private int IncreasePieceCount()
        {
            int contactMagnetBallCount = 0;
            foreach (GameObject magnetBall in magnetBallSpawner.ActiveMagnetBalls)
            {
                if (magnetBall.GetComponent<MagnetContact>().IsContact)
                {
                    contactMagnetBallCount++;
                }
            }

            if (turnState.IsPlayerTurn)
            {
                playerList[turnState.CurrentPlayerIndex].PieceCount += contactMagnetBallCount;
            }

            return contactMagnetBallCount;
        }

        public void GamePlay()
        {
            isPlaying = true;
            preventImage.SetActive(true);
            cameraView.ChangeCameraView(cameraView.TopView_tr,
                () => preventImage.SetActive(false));


            inGameUI_Manager.Show_All_Panel();
            inGameUI_Manager.UpdateUI_TurnText(turnState.TurnCount + 1);
            if (playerList != null)
            {
                playerList.ForEach(player =>
                    inGameUI_Manager.UpdateUI_ChessPiece_Text(player.PieceCount, player.playerName));
                playerList.ForEach(player => inGameUI_Manager.UpdateUI_WaitingTime_Text(0, player.playerName));
            }
            else
            {
                Debug.Log("playerList is null!!");
            }


            inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__1].playerName);
            inGameUI_Manager.CurrentTurnPlayer_Panel_FadeIn_Effect();


            GameFSM();
        }
        public void Setup()
        {

            preventImage.SetActive(true);

            turnState.Reset();

            isPlaying = false;
            isTouch = false;

            magnetWorld.IsActive = false;

            Initialize_GameSettings();

            magnetBallSpawner.DeactivateAllMagnetBall();

            if (currentSetting.gameMode == GameMode.AI)
            {
                aiFSM.SpawnPoint_Initialize();
            }

            inGameUI_Manager.Show_All_Panel();
            inGameUI_Manager.Initialize_UI();
            inGameUI_Manager.Hide_All_Panel();
            inGameUI_Manager.Hide_TurnText();

            result_Panel.Hide();

            cameraView.ChangeCameraView(cameraView.QuarterView_tr,
                () => preventImage.SetActive(false));
        }

        public void StopGame()
        {
            StopAllCoroutines();
            isPlaying = false;
            turnState.Stop();
        }
    }
}
