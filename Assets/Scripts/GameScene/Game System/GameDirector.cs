using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameDirector : MonoBehaviour
{
    [SerializeField]
    private MagnetBallSpawner magnetBallSpawner;    // 공 생성기 오브젝트
    [SerializeField]
    private MagnetWorld magnetWorld;                // 자력을 관리하는 스크립트
    [SerializeField]
    private CameraView cameraView;                  // 카메라 시점 전환 기능을 가진 스크립트
    [SerializeField]
    private InGameUI_Manager inGameUI_Manager;      // 인게임 UI를 관리하는 스크립트
    [SerializeField]
    private Result_Panel result_Panel;              // 결과 패널의 동작을 가진 스크립트
    [SerializeField] 
    private AI_FSM aiFSM;                           // AI 동작을 가진 스크립트
    [SerializeField]
    private GameObject preventImage;                // 카메라 이동 중 User의 터치를 막는 이미지

    /// <summary>
    /// 게임 시작 시 플레이어 정보를 담은 리스트를 생성
    /// </summary>
    private List<Player> playerList = new List<Player>();

    /// <summary>
    /// 상수 정의
    /// </summary>
    private const int __PLAYER__1 = 0;              // 플레이어 리스트의 플레이어1의 인덱스
    private const int __PLAYER__2 = 1;              // 플레이어 리스트의 플레이어2의 인덱스
    private const int __TURN_INFINITY__ = 100;
    private const float __SPAWN__POINT_Y = 0.7f;    // 스폰 지점으로 부터 위로 띄울 Y값

    private enum E_GameState
    {
        None,
        Player_1,
        Player_2,
        End,
    }

    private E_GameState gameState;                  // 현재 상태
    private E_GameState winPlayer;                  // 이긴 플레이어를 저장할 변수
    private int turnCount;                          // 턴을 기록하는 변수
    private bool isTouch;                           // 화면을 터치했는지 여부

    public float confirmTime;                       // 타이머에서 참조할 시간 변수(외부에서도 참조)

    private GameSetting GameSetting { get; set; }   // 게임 매니저에서 설정한 세팅 값을 저장
    public bool isPlaying { get; private set; }     // 게임이 실행 중인지 여부

    private void Start()
    {
        Setup();
    }
    private void Update()
    {
        PlayerTouchScreen();
    }
    /// <summary>
    /// 게임 플레이 FSM
    /// </summary>
    private void GameFSM()
    {
        switch (gameState)
        {
            case E_GameState.None:
                // 게임 시작 버튼 눌렀을 때 게임을 세팅 후 시작
                BattleStart();
                break;
            case E_GameState.Player_1:
                inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__1].playerName);
                break;
            case E_GameState.Player_2:
                inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__2].playerName);
                // 게임모드가 AI면 Player 2가 AI
                if (GameSetting.gameMode == GameMode.AI)
                {
                    // 게임모드가 AI면 Player 2의 턴일 때 유저의 터치를 막음
                    isTouch = true;
                    // AI 스포너 작동, 0.5초 딜레이 줌
                    Invoke("AI_SpawnAndStartTimer", 0.5f);
                }
                break;
            case E_GameState.End:
                // 게임 종료 시 실행
                EndBattle();
                break;
        }
    }
    private void BattleStart()
    {
        preventImage.SetActive(true);       // 화면 이동 중에는 User터치 막음
        gameState = E_GameState.Player_1;   // 플레이어 1이 무조건 먼저 시작

        // 게임 시작 시 카메라 시점을 탑뷰로 전환 / 전환이 끝나면 터치를 막는 이미지 비활성화
        cameraView.ChangeCameraView(cameraView.TopView_tr, () => preventImage.SetActive(false)); 
        GameFSM();
    }
    /// <summary>
    /// 플레이어 입력 감지
    /// </summary>
    private void PlayerTouchScreen()
    {
        // 게임을 플레이 중이 아닐 경우를 막음
        if (isPlaying == false)
        {
            return;
        }
        // 중복 터치를 막기 위해
        if (isTouch == true)
        {
            return;
        }
        // UI를 터치 시 공 생성을 막음
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // 플레이어 입력(터치 시 할 일들 -> 공 생성 위치 구하기)
        if (Input.GetMouseButtonDown(0))
        {
            isTouch = true;
            SpawnAndStartTimer(); // 클릭(터치) 위치에 공 생성 후 타이머 작동하는 함수
        }
    }
    /// <summary>
    /// 타이머 코루틴
    /// </summary>
    private IEnumerator StartTimer()
    {
        // confirmTime을 게임 세팅의 대기 시간으로 설정
        // 타이머 함수 외부에서 시간이 추가될 수 있음(공끼리 부딪히면) ->  MagnetContact.cs 의 콜리전엔터 이벤트
        confirmTime = GameSetting.waitingTime; // 공 생성 후 기다리는 시간(타이머) 초기화

        // 현재 턴의 플레이어를 찾아 변수에 저장
        int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

        while (confirmTime > 0)
        {
            confirmTime -= Time.deltaTime;
            
            if (confirmTime >= 0)
            {
                // UI에서 타이머의 시간을 계속 감소
                inGameUI_Manager.UpdateUI_WaitingTime_Text(confirmTime, playerList[player_index].playerName);
            }
            else
            {
                // 0이하가 되면 0초로 고정
                inGameUI_Manager.UpdateUI_WaitingTime_Text(0, playerList[player_index].playerName);
            }
            yield return null;
        }

        // 마그넷월드 침투력 비활성화(턴 넘어가서 마그넷볼끼리 합쳐지는걸 방지)
        magnetWorld.IsActive = false;

        // 타이머가 종료되면 현재 접촉된 공이 있는지 검사 후 해당 턴의 플레이어의 체스피스를 접촉된 개수만큼 증가
        bool isContact = IncreasePieceCount() > 0;

        // 해당 턴의 플레이어의 체스 피스 UI를 업데이트
        inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);

        // 접촉된 공들이 있으면 보드에서 제거(비활성화)
        if (isContact)
        {
            magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
            FindAll(contactMagnetBall => 
                contactMagnetBall.gameObject.GetComponent<MagnetContact>().IsContact == true).
            ForEach(magnetBall =>
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().DeactivateMagnetBall(magnetBall.gameObject));
        }

        // 체스 피스가 0이 된 플레이어가 있으면 게임을 종료시키는 변수를 활성화
        if (playerList.Find(player => player.PieceCount <= 0) != null)
        {
            isPlaying = false;
        }

        // 턴 세팅을 Infinity로 설정한 경우 턴 검사를 하지않음
        if (GameSetting.maxTurn < __TURN_INFINITY__)
        {
            // 현재 턴이 최대 턴 수와 같고, 현재 턴이 player2이면 게임을 종료
            if (GameSetting.maxTurn == turnCount && gameState == E_GameState.Player_2)
            {
                // 게임을 종료
                isPlaying = false;
            }
        }

        // 타이머 종료 시점에 게임이 끝났으면 게임 상태를 종료로 바꾸고 GameFSM()에 제어를 넘김
        if (isPlaying == false)
        {
            winPlayer = gameState;
            gameState = E_GameState.End;
            GameFSM();
        }
        // 타이머 종료 시점에도 게임이 끝나지 않았으면 턴 교체 후 터치 제한을 해제
        else
        {
            // Player_2의 턴이 끝나면 turnCount를 하나 증가 / 턴 텍스트 업데이트
            if (gameState == E_GameState.Player_2)
            {
                turnCount++;
                inGameUI_Manager.UpdateUI_TurnText(turnCount);
            }
            // 턴 교체
            gameState = ChangeTurn(gameState);

            // 터치 제한 해제
            isTouch = false;

            // 턴 넘기는 효과음
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.CHANGE_TURN);
            
            // 제어를 다시 GameFSM()에 넘김
            GameFSM();
        }
    }
    /// <summary>
    /// 마그넷공을 스폰 후 타이머를 작동시키는 함수
    /// </summary>
    private void SpawnAndStartTimer()
    {
        // Ray ray .... <- 클릭한 위치 구하는 함수 호출
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int layerMask = (-1) - (1 << LayerMask.NameToLayer("SpawnPoint")); // Ray가 SpawnPoint를 무시함

        bool isHit = Physics.Raycast(ray, out hit, 100f, layerMask);

        // 레이가 부딪힌 대상이 존재하고 그 대상이 Board이면
        if (isHit && hit.collider.tag == "Board")
        {
            Vector3 hitPos = hit.point;
            hitPos.y = __SPAWN__POINT_Y;

            // 마그넷 월드의 침투력을 활성화
            magnetWorld.IsActive = true;

            // 마그넷 볼 생성
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN); // 생성 사운드 재생
            magnetBallSpawner.SpawnMagnetBall(hitPos,Random.rotation);

            // 현재 턴의 플레이어의 공의 개수 하나 감소
            CurrentTurnPieceDecrease(gameState);

            // 현재 턴의 플레이어를 찾음
            int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

            // 현재 턴의 플레이어 체스 피스 UI를 업데이트
            inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);
        }
        else
        {
            isTouch = false; // 만약 의도한 곳 이외의 곳에 터치되면 다시 터치 할 수 있게
            return;
        }

        StartCoroutine(StartTimer()); // 공 생성 후 타이머 작동
    }
    /// <summary>
    /// AI턴에 스폰할 위치를 구해서 마그넷공을 스폰 후 타이머를 작동시키는 함수
    /// </summary>
    private void AI_SpawnAndStartTimer()
    {
        // AI가 마그넷볼을 생성할 위치를 aiFSM스크립트로부터 얻어와 변수에 저장 
        Vector3 aiSpawnPoint = aiFSM.AIMagnetBallSpawnPoint();
        aiSpawnPoint.y = __SPAWN__POINT_Y;          // y 좌표를 살짝 높게 조정

        magnetWorld.IsActive = true;                // 마그넷 월드의 침투력을 활성화

        // 마그넷 볼 생성
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN); // 생성 사운드 재생
        magnetBallSpawner.SpawnMagnetBall(aiSpawnPoint, Random.rotation);

        // 현재 턴 Player의 PieceCount를 1 감소
        CurrentTurnPieceDecrease(gameState);

        // 현재 턴의 플레이어를 찾아 인덱스를 저장
        int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

        // 현재 턴의 플레이어 체스 피스 UI를 업데이트
        inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);

        StartCoroutine(StartTimer()); // 공 생성 후 타이머 작동
    }
    private E_GameState ChangeTurn(E_GameState gameState)
    {
        return gameState == E_GameState.Player_1 ? E_GameState.Player_2 : E_GameState.Player_1;
    }
    /// <summary>
    /// 현재 턴의 피스 개수를 감소시키는 함수
    /// </summary>
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
        StopAllCoroutines(); // 아직 실행 중인 코루틴이 있으면 제거

        magnetBallSpawner.DeactivateAllMagnetBall(); // 모든 공 비활성화

        // 게임 UI 안보이게(턴 텍스트도)
        inGameUI_Manager.Hide_All_Panel();
        inGameUI_Manager.Hide_TurnText();

        result_Panel.gameObject.SetActive(true);      // 결과 UI 출력

        // 게임 모드가 AI일 때
        if (GameSetting.gameMode == GameMode.AI)
        {
            // 승리자가 Player2(AI) 이면 "AI" 를 대입
            if (winPlayer == E_GameState.Player_2)
            {
                result_Panel.Result_Initialize("AI", turnCount);
            }
            else // AI 모드일 때 승리자가 유저라면(Player 1 이면)
            {
                // 그대로 실행
                result_Panel.Result_Initialize(winPlayer.ToString(), turnCount);
            }
        }
        // 게임 모드가 AI가 아닐 때(offline multi)
        else 
        {
            result_Panel.Result_Initialize(winPlayer.ToString(), turnCount);
        }
       
        StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(result_Panel.gameObject.GetComponent<CanvasGroup>(), .3f));
    }
    /// <summary>
    /// 게임 세팅 초기화
    /// </summary>
    private void Initialize_GameSettings()
    {
        // 게임 매니저의 게임 세팅 값을 가져와 변수에 저장
        GameSetting = GameManager.Instance.gameSetting;

        // 플레이어 리스트가 비어있지 않으면 초기화
        if (playerList != null)
        {
            playerList.Clear();
        }

        // User 1, 2의 체스 피스 총합
        int totalPieceCount = 0; // 0으로 초기화

        // 현재 게임 모드에 따라 플레이어를 생성
        switch (GameSetting.gameMode)
        {
            case GameMode.OfflineMulti:
                // 플레이어 1,2의 피스 개수를 설정 후 두 플레이어의 총 피스 수를 변수에 저장
                playerList.Add(new Player(PlayerName.Player_1));
                playerList.Add(new Player(PlayerName.Player_2));
                playerList.ForEach(player => totalPieceCount += player.PieceCount = GameSetting.pieceCount);

                // 총 피스 개수만큼 공을 생성(풀링)
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().InstantiateMagnetBall(totalPieceCount);
                break;
            case GameMode.AI:
                playerList.Add(new Player(PlayerName.Player_1));
                playerList.Add(new Player(PlayerName.Player_AI));
                totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_1).PieceCount = GameSetting.pieceCount;
                totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_AI).PieceCount = GameSetting.pieceCount_AI;

                // 총 피스 개수만큼 공을 생성(풀링)
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().InstantiateMagnetBall(totalPieceCount);
                break;
            case GameMode.OnlineMulti:
                break;
        }
    }
    /// <summary>
    /// 접촉한 마그넷 볼의 개수만큼 해당 턴 플레이어의 PieceCount를 증가시키는 함수
    /// </summary>
    private int IncreasePieceCount()
    {
        // 접촉한 마그넷 볼의 개수를 구함
        int contactMagnetBallCount = magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
            FindAll(magnet => magnet.gameObject.GetComponent<MagnetContact>().IsContact == true).Count;

        // 현재 턴의 플레이어의 PieceCount을 증가
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
    /// <summary>
    /// 게임 시작 버튼 클릭 시 호출될 함수(버튼 이벤트에 등록되는 함수)
    /// </summary>
    public void GamePlay()
    {
        // 게임 시작, 카메라 무빙
        isPlaying = true;                                       // 게임을 플레이중으로 바꿈
        preventImage.SetActive(true);                           // 카메라 이동 중에는 User터치 막는 이미지 활성화
        cameraView.ChangeCameraView(cameraView.TopView_tr,      // 카메라 시점을 TopView로 전환
            () => preventImage.SetActive(false));               // 시점 변환 이후 User터치 막는 이미지 비활성화

        // 게임 시작과 함께 인게임 정보 표기 UI 페이드 인 효과와 함께 띄우기
        inGameUI_Manager.Show_All_Panel();                      // 플레이어 정보를 나타내는 UI다 활성화
        inGameUI_Manager.UpdateUI_TurnText(turnCount + 1);
        if (playerList != null)                                 // 인게임 정보를 표기할 UI의 정보를 업데이트    
        {
            playerList.ForEach(player => 
                inGameUI_Manager.UpdateUI_ChessPiece_Text(player.PieceCount, player.playerName));
            playerList.ForEach(player => inGameUI_Manager.UpdateUI_WaitingTime_Text(0, player.playerName));
        }
        else
        {
            Debug.Log("playerList is null!!");
        }

        // 인게임 정보 UI 연출
        inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__1].playerName);
        inGameUI_Manager.CurrentTurnPlayer_Panel_FadeIn_Effect();

        // 게임 제어 넘김
        GameFSM();
    }
    /// <summary>
    /// 게임을 처음 실행하거나, 게임을 다시 시작, 세팅을 변경 후 시작할 때 마다 호출되는 초기화 함수
    /// </summary>
    public void Setup()
    {
        // gameDirector.cs : Start  replayButtonClick   modeSelectButtonClick 에서 사용 중
        preventImage.SetActive(true);   // 초기화 중에는 User터치 막음

        gameState = E_GameState.None;   // 게임이 시작 전 상태로 설정
        winPlayer = E_GameState.None;   // 게임 승리자를 저장하는 변수 초기화

        isPlaying = false;              // 게임 실행 중 X
        isTouch = false;                // 터치 감지 X

        turnCount = 0;                  // 현재 턴을 0으로 초기화
        magnetWorld.IsActive = false;   // 자력 비활성화
        
        Initialize_GameSettings();      // 게임 세팅 값 초기화

        // 모든 마그넷 볼 비활성화
        magnetBallSpawner.DeactivateAllMagnetBall();

        // 현재 게임 모드가 AI면 Spawn Point 위치를 전부 비어있음으로 초기화
        if (GameSetting.gameMode == GameMode.AI)
        {
            aiFSM.SpawnPoint_Initialize();
        }

        // 인게임 UI 정보 초기화
        inGameUI_Manager.Show_All_Panel();              // 플레이어 정보를 나타내는 UI Panel들 활성화
        inGameUI_Manager.Initialize_UI();               // UI들을 초기화(이름을 게임 매니저의 게임 세팅 - 게임모드에서 직접 가져와 지정)
        inGameUI_Manager.Hide_All_Panel();              // UI초기화 완료 후 안보이게
        inGameUI_Manager.Hide_TurnText();               // 턴 텍스트 초기화(안보이게)

        // 결과 UI 안보이게
        result_Panel.gameObject.SetActive(false);

        cameraView.ChangeCameraView(cameraView.QuarterView_tr,  // 게임 시작 전(Start버튼 누르기 전) 카메라 시점을 쿼터뷰로
            () => preventImage.SetActive(false));               // 카메라 이동 완료 후 User터치 활성화
    }

    public void StopGame()
    {
        StopAllCoroutines();
        isPlaying = false;
        gameState = E_GameState.None;
    }
}
