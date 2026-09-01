using UnityEngine;
using UnityEngine.UI;

public class OnlineBattleMode : ModeBase
{
    [SerializeField]
    private GameObject preparing;

    public override void Setup()
    {
        isPreparing = true;
    }
    public override void PreparingMode()
    {
        if (IsPreparing == true)
        {
            preparing.SetActive(true);
            gameObject.GetComponent<Button>().interactable = false;
        }
        else
        {
            preparing.SetActive(false);
            gameObject.GetComponent<Button>().interactable = true;
        }
    }
    public void OnClickOnlineBattleButton()
    {
        GameManager.Instance.gameSetting.gameMode = GameMode.OnlineMulti;
    }
}