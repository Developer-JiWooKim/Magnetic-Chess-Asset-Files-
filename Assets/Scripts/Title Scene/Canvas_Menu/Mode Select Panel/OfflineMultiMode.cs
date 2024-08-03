using UnityEngine;
using UnityEngine.UI;

public class OfflineMultiMode : ModeBase
{
    [SerializeField]
    private GameObject preparing;

    // �ش� ��带 Ȱ��ȭ �Ϸ��� isPreparing ������ True���� ������ ��(false -> �ش� ��� ���� ����)
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