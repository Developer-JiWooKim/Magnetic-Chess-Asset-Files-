using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuList : MonoBehaviour
{
    [SerializeField]
    private bool isShowButton = false; // 현재 버튼들 안보여지는 중

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
    /// isShowButton(버튼이 보이는 상태인지 아닌지)에 따라 버튼을 보이게 또는 안보이게
    /// </summary>
    public void OnClickListButton()
    {
        // 현재 버튼이 보여지고 있으면
        if (isShowButton)
        {
            // 모든 메뉴를 닫음
            buttons.ForEach((button) => button.Hide());

            // 다시 버튼이 눌렸을 때 보여지게 하기 위해
            isShowButton = !isShowButton;
        }
        else // 현재 리스트가 보여지지 않은 상태라면
        {
            // 모든 메뉴를 보여주는데 resume은 게임 씬에서만 보여줌
            buttons.ForEach((button) => button.Show());

            // 다시 버튼이 눌렸을 때 안 보여지게 하기 위해
            isShowButton = !isShowButton;
        }
    }
}
