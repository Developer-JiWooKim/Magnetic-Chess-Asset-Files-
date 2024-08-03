using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuList : MonoBehaviour
{
    [SerializeField]
    private bool isShowButton = false; // ���� ��ư�� �Ⱥ������� ��

    private List<MenuButtonBase> buttons;
    private HorizontalLayoutGroup horizontalLayoutGroup;

    private const float TITLE_SPACING_HorizontalLayoutGroup = -80f;
    private const float GAME_SPACING_HorizontalLayoutGroup = 20f;

    private void Start()
    {
        Setup();
    }
    private void Setup()
    {
        buttons = GetComponentsInChildren<MenuButtonBase>(true).ToList();

        horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.spacing = TITLE_SPACING_HorizontalLayoutGroup;

        GameManager.Instance.ChangeSceneAction += () => horizontalLayoutGroup.spacing =
                DontDestroy_Menu.Instance.CurrentScene == DontDestroy_Menu.SceneName.Title ?
                TITLE_SPACING_HorizontalLayoutGroup : GAME_SPACING_HorizontalLayoutGroup;
    }
    /// <summary>
    /// isShowButton(��ư�� ���̴� �������� �ƴ���)�� ���� ��ư�� ���̰� �Ǵ� �Ⱥ��̰�
    /// </summary>
    public void OnClickListButton()
    {
        // ���� ��ư�� �������� ������
        if (isShowButton)
        {
            // ��� �޴��� ����
            buttons.ForEach((button) => button.Hide());

            // �ٽ� ��ư�� ������ �� �������� �ϱ� ����
            isShowButton = !isShowButton;
        }
        else // ���� ����Ʈ�� �������� ���� ���¶��
        {
            // ��� �޴��� �����ִµ� resume�� ���� �������� ������
            buttons.ForEach((button) => button.Show());

            // �ٽ� ��ư�� ������ �� �� �������� �ϱ� ����
            isShowButton = !isShowButton;
        }
    }
}
