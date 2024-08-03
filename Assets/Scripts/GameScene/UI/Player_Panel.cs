using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_Panel : Player_Panel_Base
{
    [SerializeField]
    private TextMeshProUGUI playerName_Text;
    [SerializeField]
    private TextMeshProUGUI chessPiece_Text;
    [SerializeField]
    private TextMeshProUGUI playerTimer_Text;
    [SerializeField]
    private Image faceBorder_Image;

    [SerializeField]
    private PlayerName playerName;
    public PlayerName Player_Panel_playerName
    {
        get
        {
            return playerName;
        }
    }
    public override void Initiallize_Panel()
    {
        SetName();
        Update_PieceCount(0);
        Update_Timer(0);
    }
    public void SetName()
    {
        // 현재 게임 모드가 AI이면 Player 2 패널의 플레이어 이름을 AI로 변경 아니면 
        bool isAIMode = GameManager.Instance.gameSetting.gameMode == GameMode.AI;
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
        playerTimer_Text.text = string.Format("{00:N2}", _time);
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
