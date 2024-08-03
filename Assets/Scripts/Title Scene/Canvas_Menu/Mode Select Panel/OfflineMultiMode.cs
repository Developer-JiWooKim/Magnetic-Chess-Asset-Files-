using UnityEngine;
using UnityEngine.UI;

public class OfflineMultiMode : ModeBase
{
    [SerializeField]
    private GameObject preparing;

    // 해당 모드를 활성화 하려면 isPreparing 변수에 True값을 넣으면 됨(false -> 해당 모드 실행 가능)
    public override void Setup()
    {
        isPreparing = false;
    }
    public override void PreparingMode()
    {
        if (IsPreparing)
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
    public void OnClickOfflineMultiButton()
    {
        GameManager.Instance.gameSetting.gameMode = GameMode.OfflineMulti;
    }
}