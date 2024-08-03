using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameDirector : MonoBehaviour
{
    [SerializeField]
    private MagnetBallSpawner magnetBallSpawner;    // �� ������ ������Ʈ
    [SerializeField]
    private MagnetWorld magnetWorld;                // �ڷ��� �����ϴ� ��ũ��Ʈ
    [SerializeField]
    private CameraView cameraView;                  // ī�޶� ���� ��ȯ ����� ���� ��ũ��Ʈ
    [SerializeField]
    private InGameUI_Manager inGameUI_Manager;      // �ΰ��� UI�� �����ϴ� ��ũ��Ʈ
    [SerializeField]
    private Result_Panel result_Panel;              // ��� �г��� ������ ���� ��ũ��Ʈ
    [SerializeField] 
    private AI_FSM aiFSM;                           // AI ������ ���� ��ũ��Ʈ
    [SerializeField]
    private GameObject preventImage;                // ī�޶� �̵� �� User�� ��ġ�� ���� �̹���

    /// <summary>
    /// ���� ���� �� �÷��̾� ������ ���� ����Ʈ�� ����
    /// </summary>
    private List<Player> playerList = new List<Player>();

    /// <summary>
    /// ��� ����
    /// </summary>
    private const int __PLAYER__1 = 0;              // �÷��̾� ����Ʈ�� �÷��̾�1�� �ε���
    private const int __PLAYER__2 = 1;              // �÷��̾� ����Ʈ�� �÷��̾�2�� �ε���
    private const int __TURN_INFINITY__ = 100;
    private const float __SPAWN__POINT_Y = 0.7f;    // ���� �������� ���� ���� ��� Y��

    private enum E_GameState
    {
        None,
        Player_1,
        Player_2,
        End,
    }

    private E_GameState gameState;                  // ���� ����
    private E_GameState winPlayer;                  // �̱� �÷��̾ ������ ����
    private int turnCount;                          // ���� ����ϴ� ����
    private bool isTouch;                           // ȭ���� ��ġ�ߴ��� ����

    public float confirmTime;                       // Ÿ�̸ӿ��� ������ �ð� ����(�ܺο����� ����)

    private GameSetting GameSetting { get; set; }   // ���� �Ŵ������� ������ ���� ���� ����
    public bool isPlaying { get; private set; }     // ������ ���� ������ ����

    private void Start()
    {
        Setup();
    }
    private void Update()
    {
        PlayerTouchScreen();
    }
    /// <summary>
    /// ���� �÷��� FSM
    /// </summary>
    private void GameFSM()
    {
        switch (gameState)
        {
            case E_GameState.None:
                // ���� ���� ��ư ������ �� ������ ���� �� ����
                BattleStart();
                break;
            case E_GameState.Player_1:
                inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__1].playerName);
                break;
            case E_GameState.Player_2:
                inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__2].playerName);
                // ���Ӹ�尡 AI�� Player 2�� AI
                if (GameSetting.gameMode == GameMode.AI)
                {
                    // ���Ӹ�尡 AI�� Player 2�� ���� �� ������ ��ġ�� ����
                    isTouch = true;
                    // AI ������ �۵�, 0.5�� ������ ��
                    Invoke("AI_SpawnAndStartTimer", 0.5f);
                }
                break;
            case E_GameState.End:
                // ���� ���� �� ����
                EndBattle();
                break;
        }
    }
    private void BattleStart()
    {
        preventImage.SetActive(true);       // ȭ�� �̵� �߿��� User��ġ ����
        gameState = E_GameState.Player_1;   // �÷��̾� 1�� ������ ���� ����

        // ���� ���� �� ī�޶� ������ ž��� ��ȯ / ��ȯ�� ������ ��ġ�� ���� �̹��� ��Ȱ��ȭ
        cameraView.ChangeCameraView(cameraView.TopView_tr, () => preventImage.SetActive(false)); 
        GameFSM();
    }
    /// <summary>
    /// �÷��̾� �Է� ����
    /// </summary>
    private void PlayerTouchScreen()
    {
        // ������ �÷��� ���� �ƴ� ��츦 ����
        if (isPlaying == false)
        {
            return;
        }
        // �ߺ� ��ġ�� ���� ����
        if (isTouch == true)
        {
            return;
        }
        // UI�� ��ġ �� �� ������ ����
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // �÷��̾� �Է�(��ġ �� �� �ϵ� -> �� ���� ��ġ ���ϱ�)
        if (Input.GetMouseButtonDown(0))
        {
            isTouch = true;
            SpawnAndStartTimer(); // Ŭ��(��ġ) ��ġ�� �� ���� �� Ÿ�̸� �۵��ϴ� �Լ�
        }
    }
    /// <summary>
    /// Ÿ�̸� �ڷ�ƾ
    /// </summary>
    private IEnumerator StartTimer()
    {
        // confirmTime�� ���� ������ ��� �ð����� ����
        // Ÿ�̸� �Լ� �ܺο��� �ð��� �߰��� �� ����(������ �ε�����) ->  MagnetContact.cs �� �ݸ������� �̺�Ʈ
        confirmTime = GameSetting.waitingTime; // �� ���� �� ��ٸ��� �ð�(Ÿ�̸�) �ʱ�ȭ

        // ���� ���� �÷��̾ ã�� ������ ����
        int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

        while (confirmTime > 0)
        {
            confirmTime -= Time.deltaTime;
            
            if (confirmTime >= 0)
            {
                // UI���� Ÿ�̸��� �ð��� ��� ����
                inGameUI_Manager.UpdateUI_WaitingTime_Text(confirmTime, playerList[player_index].playerName);
            }
            else
            {
                // 0���ϰ� �Ǹ� 0�ʷ� ����
                inGameUI_Manager.UpdateUI_WaitingTime_Text(0, playerList[player_index].playerName);
            }
            yield return null;
        }

        // ���׳ݿ��� ħ���� ��Ȱ��ȭ(�� �Ѿ�� ���׳ݺ����� �������°� ����)
        magnetWorld.IsActive = false;

        // Ÿ�̸Ӱ� ����Ǹ� ���� ���˵� ���� �ִ��� �˻� �� �ش� ���� �÷��̾��� ü���ǽ��� ���˵� ������ŭ ����
        bool isContact = IncreasePieceCount() > 0;

        // �ش� ���� �÷��̾��� ü�� �ǽ� UI�� ������Ʈ
        inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);

        // ���˵� ������ ������ ���忡�� ����(��Ȱ��ȭ)
        if (isContact)
        {
            magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
            FindAll(contactMagnetBall => 
                contactMagnetBall.gameObject.GetComponent<MagnetContact>().IsContact == true).
            ForEach(magnetBall =>
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().DeactivateMagnetBall(magnetBall.gameObject));
        }

        // ü�� �ǽ��� 0�� �� �÷��̾ ������ ������ �����Ű�� ������ Ȱ��ȭ
        if (playerList.Find(player => player.PieceCount <= 0) != null)
        {
            isPlaying = false;
        }

        // �� ������ Infinity�� ������ ��� �� �˻縦 ��������
        if (GameSetting.maxTurn < __TURN_INFINITY__)
        {
            // ���� ���� �ִ� �� ���� ����, ���� ���� player2�̸� ������ ����
            if (GameSetting.maxTurn == turnCount && gameState == E_GameState.Player_2)
            {
                // ������ ����
                isPlaying = false;
            }
        }

        // Ÿ�̸� ���� ������ ������ �������� ���� ���¸� ����� �ٲٰ� GameFSM()�� ��� �ѱ�
        if (isPlaying == false)
        {
            winPlayer = gameState;
            gameState = E_GameState.End;
            GameFSM();
        }
        // Ÿ�̸� ���� �������� ������ ������ �ʾ����� �� ��ü �� ��ġ ������ ����
        else
        {
            // Player_2�� ���� ������ turnCount�� �ϳ� ���� / �� �ؽ�Ʈ ������Ʈ
            if (gameState == E_GameState.Player_2)
            {
                turnCount++;
                inGameUI_Manager.UpdateUI_TurnText(turnCount);
            }
            // �� ��ü
            gameState = ChangeTurn(gameState);

            // ��ġ ���� ����
            isTouch = false;

            // �� �ѱ�� ȿ����
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.CHANGE_TURN);
            
            // ��� �ٽ� GameFSM()�� �ѱ�
            GameFSM();
        }
    }
    /// <summary>
    /// ���׳ݰ��� ���� �� Ÿ�̸Ӹ� �۵���Ű�� �Լ�
    /// </summary>
    private void SpawnAndStartTimer()
    {
        // Ray ray .... <- Ŭ���� ��ġ ���ϴ� �Լ� ȣ��
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int layerMask = (-1) - (1 << LayerMask.NameToLayer("SpawnPoint")); // Ray�� SpawnPoint�� ������

        bool isHit = Physics.Raycast(ray, out hit, 100f, layerMask);

        // ���̰� �ε��� ����� �����ϰ� �� ����� Board�̸�
        if (isHit && hit.collider.tag == "Board")
        {
            Vector3 hitPos = hit.point;
            hitPos.y = __SPAWN__POINT_Y;

            // ���׳� ������ ħ������ Ȱ��ȭ
            magnetWorld.IsActive = true;

            // ���׳� �� ����
            SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN); // ���� ���� ���
            magnetBallSpawner.SpawnMagnetBall(hitPos,Random.rotation);

            // ���� ���� �÷��̾��� ���� ���� �ϳ� ����
            CurrentTurnPieceDecrease(gameState);

            // ���� ���� �÷��̾ ã��
            int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

            // ���� ���� �÷��̾� ü�� �ǽ� UI�� ������Ʈ
            inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);
        }
        else
        {
            isTouch = false; // ���� �ǵ��� �� �̿��� ���� ��ġ�Ǹ� �ٽ� ��ġ �� �� �ְ�
            return;
        }

        StartCoroutine(StartTimer()); // �� ���� �� Ÿ�̸� �۵�
    }
    /// <summary>
    /// AI�Ͽ� ������ ��ġ�� ���ؼ� ���׳ݰ��� ���� �� Ÿ�̸Ӹ� �۵���Ű�� �Լ�
    /// </summary>
    private void AI_SpawnAndStartTimer()
    {
        // AI�� ���׳ݺ��� ������ ��ġ�� aiFSM��ũ��Ʈ�κ��� ���� ������ ���� 
        Vector3 aiSpawnPoint = aiFSM.AIMagnetBallSpawnPoint();
        aiSpawnPoint.y = __SPAWN__POINT_Y;          // y ��ǥ�� ��¦ ���� ����

        magnetWorld.IsActive = true;                // ���׳� ������ ħ������ Ȱ��ȭ

        // ���׳� �� ����
        SoundManager.Instance.Play_SFX(SoundManager.E_SFX_Name.MAGNETBALL_SPAWN); // ���� ���� ���
        magnetBallSpawner.SpawnMagnetBall(aiSpawnPoint, Random.rotation);

        // ���� �� Player�� PieceCount�� 1 ����
        CurrentTurnPieceDecrease(gameState);

        // ���� ���� �÷��̾ ã�� �ε����� ����
        int player_index = gameState == E_GameState.Player_1 ? __PLAYER__1 : __PLAYER__2;

        // ���� ���� �÷��̾� ü�� �ǽ� UI�� ������Ʈ
        inGameUI_Manager.UpdateUI_ChessPiece_Text(playerList[player_index].PieceCount, playerList[player_index].playerName);

        StartCoroutine(StartTimer()); // �� ���� �� Ÿ�̸� �۵�
    }
    private E_GameState ChangeTurn(E_GameState gameState)
    {
        return gameState == E_GameState.Player_1 ? E_GameState.Player_2 : E_GameState.Player_1;
    }
    /// <summary>
    /// ���� ���� �ǽ� ������ ���ҽ�Ű�� �Լ�
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
        StopAllCoroutines(); // ���� ���� ���� �ڷ�ƾ�� ������ ����

        magnetBallSpawner.DeactivateAllMagnetBall(); // ��� �� ��Ȱ��ȭ

        // ���� UI �Ⱥ��̰�(�� �ؽ�Ʈ��)
        inGameUI_Manager.Hide_All_Panel();
        inGameUI_Manager.Hide_TurnText();

        result_Panel.gameObject.SetActive(true);      // ��� UI ���

        // ���� ��尡 AI�� ��
        if (GameSetting.gameMode == GameMode.AI)
        {
            // �¸��ڰ� Player2(AI) �̸� "AI" �� ����
            if (winPlayer == E_GameState.Player_2)
            {
                result_Panel.Result_Initialize("AI", turnCount);
            }
            else // AI ����� �� �¸��ڰ� �������(Player 1 �̸�)
            {
                // �״�� ����
                result_Panel.Result_Initialize(winPlayer.ToString(), turnCount);
            }
        }
        // ���� ��尡 AI�� �ƴ� ��(offline multi)
        else 
        {
            result_Panel.Result_Initialize(winPlayer.ToString(), turnCount);
        }
       
        StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(result_Panel.gameObject.GetComponent<CanvasGroup>(), .3f));
    }
    /// <summary>
    /// ���� ���� �ʱ�ȭ
    /// </summary>
    private void Initialize_GameSettings()
    {
        // ���� �Ŵ����� ���� ���� ���� ������ ������ ����
        GameSetting = GameManager.Instance.gameSetting;

        // �÷��̾� ����Ʈ�� ������� ������ �ʱ�ȭ
        if (playerList != null)
        {
            playerList.Clear();
        }

        // User 1, 2�� ü�� �ǽ� ����
        int totalPieceCount = 0; // 0���� �ʱ�ȭ

        // ���� ���� ��忡 ���� �÷��̾ ����
        switch (GameSetting.gameMode)
        {
            case GameMode.OfflineMulti:
                // �÷��̾� 1,2�� �ǽ� ������ ���� �� �� �÷��̾��� �� �ǽ� ���� ������ ����
                playerList.Add(new Player(PlayerName.Player_1));
                playerList.Add(new Player(PlayerName.Player_2));
                playerList.ForEach(player => totalPieceCount += player.PieceCount = GameSetting.pieceCount);

                // �� �ǽ� ������ŭ ���� ����(Ǯ��)
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().InstantiateMagnetBall(totalPieceCount);
                break;
            case GameMode.AI:
                playerList.Add(new Player(PlayerName.Player_1));
                playerList.Add(new Player(PlayerName.Player_AI));
                totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_1).PieceCount = GameSetting.pieceCount;
                totalPieceCount += playerList.Find(player => player.playerName == PlayerName.Player_AI).PieceCount = GameSetting.pieceCount_AI;

                // �� �ǽ� ������ŭ ���� ����(Ǯ��)
                magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().InstantiateMagnetBall(totalPieceCount);
                break;
            case GameMode.OnlineMulti:
                break;
        }
    }
    /// <summary>
    /// ������ ���׳� ���� ������ŭ �ش� �� �÷��̾��� PieceCount�� ������Ű�� �Լ�
    /// </summary>
    private int IncreasePieceCount()
    {
        // ������ ���׳� ���� ������ ����
        int contactMagnetBallCount = magnetBallSpawner.GetComponent<MagnetBallMemoryPool>().GetPoolItemList().
            FindAll(magnet => magnet.gameObject.GetComponent<MagnetContact>().IsContact == true).Count;

        // ���� ���� �÷��̾��� PieceCount�� ����
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
    /// ���� ���� ��ư Ŭ�� �� ȣ��� �Լ�(��ư �̺�Ʈ�� ��ϵǴ� �Լ�)
    /// </summary>
    public void GamePlay()
    {
        // ���� ����, ī�޶� ����
        isPlaying = true;                                       // ������ �÷��������� �ٲ�
        preventImage.SetActive(true);                           // ī�޶� �̵� �߿��� User��ġ ���� �̹��� Ȱ��ȭ
        cameraView.ChangeCameraView(cameraView.TopView_tr,      // ī�޶� ������ TopView�� ��ȯ
            () => preventImage.SetActive(false));               // ���� ��ȯ ���� User��ġ ���� �̹��� ��Ȱ��ȭ

        // ���� ���۰� �Բ� �ΰ��� ���� ǥ�� UI ���̵� �� ȿ���� �Բ� ����
        inGameUI_Manager.Show_All_Panel();                      // �÷��̾� ������ ��Ÿ���� UI�� Ȱ��ȭ
        inGameUI_Manager.UpdateUI_TurnText(turnCount + 1);
        if (playerList != null)                                 // �ΰ��� ������ ǥ���� UI�� ������ ������Ʈ    
        {
            playerList.ForEach(player => 
                inGameUI_Manager.UpdateUI_ChessPiece_Text(player.PieceCount, player.playerName));
            playerList.ForEach(player => inGameUI_Manager.UpdateUI_WaitingTime_Text(0, player.playerName));
        }
        else
        {
            Debug.Log("playerList is null!!");
        }

        // �ΰ��� ���� UI ����
        inGameUI_Manager.CurrentTurnPlayer_Panel_Effect(playerList[__PLAYER__1].playerName);
        inGameUI_Manager.CurrentTurnPlayer_Panel_FadeIn_Effect();

        // ���� ���� �ѱ�
        GameFSM();
    }
    /// <summary>
    /// ������ ó�� �����ϰų�, ������ �ٽ� ����, ������ ���� �� ������ �� ���� ȣ��Ǵ� �ʱ�ȭ �Լ�
    /// </summary>
    public void Setup()
    {
        // gameDirector.cs : Start  replayButtonClick   modeSelectButtonClick ���� ��� ��
        preventImage.SetActive(true);   // �ʱ�ȭ �߿��� User��ġ ����

        gameState = E_GameState.None;   // ������ ���� �� ���·� ����
        winPlayer = E_GameState.None;   // ���� �¸��ڸ� �����ϴ� ���� �ʱ�ȭ

        isPlaying = false;              // ���� ���� �� X
        isTouch = false;                // ��ġ ���� X

        turnCount = 0;                  // ���� ���� 0���� �ʱ�ȭ
        magnetWorld.IsActive = false;   // �ڷ� ��Ȱ��ȭ
        
        Initialize_GameSettings();      // ���� ���� �� �ʱ�ȭ

        // ��� ���׳� �� ��Ȱ��ȭ
        magnetBallSpawner.DeactivateAllMagnetBall();

        // ���� ���� ��尡 AI�� Spawn Point ��ġ�� ���� ����������� �ʱ�ȭ
        if (GameSetting.gameMode == GameMode.AI)
        {
            aiFSM.SpawnPoint_Initialize();
        }

        // �ΰ��� UI ���� �ʱ�ȭ
        inGameUI_Manager.Show_All_Panel();              // �÷��̾� ������ ��Ÿ���� UI Panel�� Ȱ��ȭ
        inGameUI_Manager.Initialize_UI();               // UI���� �ʱ�ȭ(�̸��� ���� �Ŵ����� ���� ���� - ���Ӹ�忡�� ���� ������ ����)
        inGameUI_Manager.Hide_All_Panel();              // UI�ʱ�ȭ �Ϸ� �� �Ⱥ��̰�
        inGameUI_Manager.Hide_TurnText();               // �� �ؽ�Ʈ �ʱ�ȭ(�Ⱥ��̰�)

        // ��� UI �Ⱥ��̰�
        result_Panel.gameObject.SetActive(false);

        cameraView.ChangeCameraView(cameraView.QuarterView_tr,  // ���� ���� ��(Start��ư ������ ��) ī�޶� ������ ���ͺ��
            () => preventImage.SetActive(false));               // ī�޶� �̵� �Ϸ� �� User��ġ Ȱ��ȭ
    }

    public void StopGame()
    {
        StopAllCoroutines();
        isPlaying = false;
        gameState = E_GameState.None;
    }
}
