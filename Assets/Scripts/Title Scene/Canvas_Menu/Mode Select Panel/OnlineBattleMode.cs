using UnityEngine;
using UnityEngine.UI;

public class OnlineBattleMode : ModeBase
{
    [SerializeField]
    private GameObject preparing;

    // 해당 모드를 활성화 하려면 isPreparing 변수에 True값을 넣으면 됨(false -> 해당 모드 실행 가능)
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