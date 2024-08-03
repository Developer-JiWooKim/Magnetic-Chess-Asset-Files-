using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI_Manager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI turnText;

    /// <summary>
    /// 인게임 플레이어 정보를 나타내는 UI패널을 담는 리스트
    /// </summary>
    private List<Player_Panel> player_Panels;

    private Character_FaceCam faceCam;

    /// <summary>
    /// 상수 정의
    /// </summary>
    private const float __ALPHA_VALUE_1 = 1f;
    private const float __ALPHA_VALUE_0_4 = .4f;
    
    private void Awake()
    {
        Setup();
    }
    private void Setup()
    {
        faceCam = FindObjectOfType<Character_FaceCam>();
        player_Panels = GetComponentsInChildren<Player_Panel>(true).ToList();
    }
    /// <summary>
    /// 모든 플레이어 패널 활성화
    /// </summary>
    public void Show_All_Panel()
    {
        if (player_Panels != null)
        {
            Debug.Log("Player_Panel is show");
            player_Panels.ForEach((panel) =>
            {
                panel.gameObject.SetActive(true);
            });
        }
        else
        {
            Debug.Log("Player_Panel is null!");
        }
    }
    /// <summary>
    /// 모든 플레이어 패널 비활성화
    /// </summary>
    public void Hide_All_Panel()
    {
        if (player_Panels != null)
        {
            Debug.Log("Player_Panel is hide");
            player_Panels.ForEach((panel) =>
            {
                panel.gameObject.SetActive(false);
            });
        }
        else
        {
            Debug.Log("Player_Panel is null!");
        }
    }
    /// <summary>
    /// 패널 정보 갱신 - 체스 피스
    /// </summary>
    public void UpdateUI_ChessPiece_Text(int _count, PlayerName _currPlayer)
    {
        if (player_Panels != null)
        {
            player_Panels.Find(panel => panel.Player_Panel_playerName == _currPlayer).Update_PieceCount(_count);
        }
    }
    /// <summary>
    /// 패널 정보 갱신 - 타이머
    /// </summary>
    public void UpdateUI_WaitingTime_Text(float _time, PlayerName _currPlayer)
    {
        if (player_Panels != null)
        {
            player_Panels.Find(panel => panel.Player_Panel_playerName == _currPlayer).Update_Timer(_time);
        }
    }
    /// <summary>
    /// UI 초기화
    /// </summary>
    public void Initialize_UI()
    {
        // 플레이어 패널의 UI들 초기화
        if (player_Panels != null)
        {
            player_Panels.ForEach(panel => panel.Initiallize_Panel());
        }
        // UI 초기화 할때마다 캐릭터 초기화
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
        player_Panels.ForEach(panel =>
        {
            if(panel.Player_Panel_playerName == _currPlayer)
            {
                panel.GetComponent<CanvasGroup>().alpha = __ALPHA_VALUE_1;
                Color color = panel.GetComponent<Image>().color;
                color.a = 1.0f;
                panel.GetComponent<Image>().color = color;
                panel.CurrentTurn_FaceOn();
            }
            else
            {
                panel.GetComponent<CanvasGroup>().alpha = __ALPHA_VALUE_0_4;
                Color color = panel.GetComponent<Image>().color;
                color.a = 0f;
                panel.GetComponent<Image>().color = color;
                panel.CurrentTurn_FaceOff();
            }
        });
    }
    public void CurrentTurnPlayer_Panel_FadeIn_Effect()
    {
        player_Panels.ForEach(panel => StartCoroutine(FadeEffect_UI.FadeIn_CanvasGroup(panel.GetComponent<CanvasGroup>(), 0.3f, 
                    () => { 
                        if (panel.Player_Panel_playerName != PlayerName.Player_1)
                        {
                            panel.GetComponent<CanvasGroup>().alpha = __ALPHA_VALUE_0_4;
                        }
                    })));
    }
    public void UpdateUI_TurnText(int _turnCount)
    {
        if (GameManager.Instance.gameSetting.maxTurn > 100)
        {
            turnText.text = "Max Turn [ Infinity ]\n<size=80>Turn [ " + _turnCount.ToString() + " ]";
        }
        else
        {
            turnText.text = "Max Turn [ " + GameManager.Instance.gameSetting.maxTurn.ToString() + " ]\n<size=80>Turn [ " + _turnCount.ToString() + " ]";
        }
    }
    public void Hide_TurnText()
    {
        turnText.text = "";
    }
}
