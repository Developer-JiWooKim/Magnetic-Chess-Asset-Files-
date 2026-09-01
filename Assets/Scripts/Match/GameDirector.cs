using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.AI;
using Assets.Scripts.Core;
using Assets.Scripts.Magnet;
using Assets.Scripts.UI;

namespace Assets.Scripts.Match
{
    public sealed class GameDirector : Singleton<GameDirector>
    {
        // GameScene에만 존재하고 씬과 함께 사라져야 하므로 DontDestroyOnLoad 대상이 아니다.
        protected override bool IsPersistent => false;

        [SerializeField]
        private MagnetBallSpawner magnetBallSpawner;
        [SerializeField]
        private MagnetWorld magnetWorld;
        [SerializeField]
        private CameraView cameraView;
        [SerializeField]
        private InGameUI_Manager inGameUI_Manager;
        [SerializeField]
        private Result_Panel result_Panel;
        [SerializeField]
        private AI_FSM aiFSM;
        [SerializeField]
        private GameObject preventImage;

        private List<Player> playerList = new List<Player>();

        private const int __PLAYER__1 = 0;
        private const int __PLAYER__2 = 1;
        private const int __TURN_INFINITY__ = 100;
        private const float __SPAWN__POINT_Y = 0.7f;

        private enum E_GameState
        {
            None,
            Player_1,
            Player_2,
            End,
        }

        private E_GameState gameState;
        private E_GameState winPlayer;
        private int turnCount;
        private bool isTouch;

        public float confirmTime;

        private GameSetting currentSetting;
        public bool isPlaying { get; private set; }

        private void Start()
        {
            Setup();
        }
        private void Update()
        {
            PlayerTouchScreen();
        }

        private void GameFSM()
        {
            switch (gameState)
            {
                case E_GameState.None:

                    BattleStart();
                    break;
                case E_GameState.Player_1:
                    inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__1].playerName);
                    break;
                case E_GameState.Player_2:
                    inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__2].playerName);

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
            gameState = E_GameState.Player_1;


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
            confirmTime = currentSetting.waitingTime;

            int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

            while (confirmTime > 0)
            {
                confirmTime -= Time.deltaTime;

                if (confirmTime >= 0)
                {
                    inGameUI_Manager.UpdateUI_WaitingTime_Text(confirmTime, playerList[player_index].playerName);
                }
                else
                {
                    inGameUI_Manager.UpdateUI_WaitingTime_Text(0, playerList[player_index].playerName);
                }
                yield return null;
            }


            magnetWorld.IsActive = false;


            bool isContact = IncreasePieceCount() > 0;


            inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);


            if (isContact)
            {
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
                FindAll(contactMagnetBall =>
                    contactMagnetBall.gameObject.GetComponent<MagnetContact>().IsContact == true).
                ForEach(magnetBall =>
                    magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().DeactivateMagnetBall(magnetBall.gameObject));
            }


            if (playerList.Find(player => player.PieceCount <= 0) != null)
            {
                isPlaying = false;
            }


            if (currentSetting.maxTurn < __TURN_INFINITY__)
            {

                if (currentSetting.maxTurn == turnCount && gameState == E_GameState.Player_2)
                {

                    isPlaying = false;
                }
            }


            if (isPlaying == false)
            {
                winPlayer = gameState;
                gameState = E_GameState.End;
                GameFSM();
            }

            else
            {

                if (gameState == E_GameState.Player_2)
                {
                    turnCount++;
                    inGameUI_Manager.UpdateUI_TurnText(turnCount);
                }

                gameState = ChangeTurn(gameState);


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


                SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN); // ���� ���� ���
                magnetBallSpawner.SpawnMagnetBall(hitPos, Random.rotation);


                CurrentTurnPieceDecrease(gameState);


                int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;


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


            CurrentTurnPieceDecrease(gameState);


            int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;


            inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);

            StartCoroutine(StartTimer());
        }
        private E_GameState ChangeTurn(E_GameState gameState)
        {
            return gameState == E_GameState.Player_1 ? E_GameState.Player_2 : E_GameState.Player_1;
        }

        private void CurrentTurnPieceDecrease(E_GameState currTurn)
        {
            if (currTurn == E_GameState.Player_1)
            {
                playerList[__PLAYER__1].PieceCount--;
            }
            else if (currTurn == E_GameState.Player_2)
            {
                playerList[__PLAYER__2].PieceCount--;
            }
        }
        private void EndBattle()
        {
            StopAllCoroutines();

            magnetBallSpawner.DeactivateAllMagnetBall();


            inGameUI_Manager.Hide_All_Panel();
            inGameUI_Manager.Hide_TurnText();

            result_Panel.Show();


            if (currentSetting.gameMode == GameMode.AI)
            {

                if (winPlayer == E_GameState.Player_2)
                {
                    result_Panel.Result_Initialize("AI", turnCount);
                }
                else
                {

                    result_Panel.Result_Initialize(winPlayer.ToString(), turnCount);
                }
            }
            else
            {
                result_Panel.Result_Initialize(winPlayer.ToString(), turnCount);
            }

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

                    magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().InstantiateMagnetBall(totalPieceCount);
                    break;
                case GameMode.AI:
                    playerList.Add(new Player(PlayerName.Player_1));
                    playerList.Add(new Player(PlayerName.Player_AI));
                    totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_1).PieceCount = currentSetting.pieceCount;
                    totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_AI).PieceCount = currentSetting.pieceCount_AI;


                    magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().InstantiateMagnetBall(totalPieceCount);
                    break;
                case GameMode.OnlineMulti:
                    break;
            }
        }

        private int IncreasePieceCount()
        {

            int contactMagnetBallCount = magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
                FindAll(magnet => magnet.gameObject.GetComponent<MagnetContact>().IsContact == true).Count;


            if (gameState == E_GameState.Player_1)
            {
                playerList[__PLAYER__1].PieceCount += contactMagnetBallCount;
            }
            else if (gameState == E_GameState.Player_2)
            {
                playerList[__PLAYER__2].PieceCount += contactMagnetBallCount;
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
            inGameUI_Manager.UpdateUI_TurnText(turnCount + 1);
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

            gameState = E_GameState.None;
            winPlayer = E_GameState.None;

            isPlaying = false;
            isTouch = false;

            turnCount = 0;
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
            gameState = E_GameState.None;
        }
    }
}
