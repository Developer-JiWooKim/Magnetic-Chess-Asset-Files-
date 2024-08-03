using UnityEngine;
using UnityEngine.UI;

public class AIBattleMode : ModeBase
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
    public void OnClickAIBattleButton()
    {
        GameManager.Instance.gameSetting.gameMode = GameMode.AI;
    }
}