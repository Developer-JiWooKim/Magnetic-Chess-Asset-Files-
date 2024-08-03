using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class GameSettingMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown pieceCountDropdown;

    [SerializeField]
    private TMP_Dropdown pieceCountDropdown_AI;

    [SerializeField]
    private TMP_Dropdown waitingTimeDropdown;

    [SerializeField]
    private TMP_Dropdown maxTurnDropdown;

    private void OnEnable()
    {
        SetDefaultSetting();
    }
    public void SetPieceCount()
    {
        string option = pieceCountDropdown.options[pieceCountDropdown.value].text;
        int option_value = int.Parse(option);
        GameManager.Instance.gameSetting.pieceCount = option_value;
    }
    public void SetPieceCount_AI()
    {
        string option = pieceCountDropdown_AI.options[pieceCountDropdown_AI.value].text;
        int option_value = int.Parse(option);
        GameManager.Instance.gameSetting.pieceCount_AI = option_value;
    }
    public void SetWaitingTime()
    {
        string option = waitingTimeDropdown.options[waitingTimeDropdown.value].text;
        string option_int = Regex.Replace(option, @"[^0-9]", "");
        int option_value = int.Parse(option_int);
        GameManager.Instance.gameSetting.waitingTime = option_value;
    }
    public void SetMaxTurn()
    {
        string option = maxTurnDropdown.options[maxTurnDropdown.value].text;
        int option_value;
        if (option == "Infinity")
        {
            option_value = 999;
        }
        else
        {
            option_value = int.Parse(option);
        }
        
        
        GameManager.Instance.gameSetting.maxTurn = option_value;
    }
    public void SetDefaultSetting()
    {
        GameManager.Instance.gameSetting.pieceCount = 15;
        GameManager.Instance.gameSetting.pieceCount_AI = 15;
        GameManager.Instance.gameSetting.waitingTime = 1;
        GameManager.Instance.gameSetting.maxTurn = 20;
    }
}