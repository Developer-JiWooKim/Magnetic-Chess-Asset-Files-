using System.Collections;
using UnityEngine;

public class Tablet_Logic : MonoBehaviour
{
    [SerializeField]
    private GameObject loading_UI; // 로딩 UI

    // 모드 선택 화면
    [SerializeField]
    private GameObject select_UI; // 모드 선택 UI(작은 버전)

    [SerializeField]
    private GameObject setting_UI; // 세팅 UI(작은 버전) 

    [SerializeField]
    private ModeSelectPanel modeSelectPanel;

    /// <summary>
    /// 카메라 무빙 후 화면이 켜지는 연출을 위해 한번만 사용될 코루틴 함수
    /// </summary>
    private IEnumerator Switching_Select_after_loading()
    {
        yield return new WaitForSeconds(1.5f);
        loading_UI.SetActive(false); // 로딩 효과 안보이게
        select_UI.SetActive(true);  // 테블릿의 select UI보이게
        modeSelectPanel.ShowPanel();    // ModeSelect Panel 보이게
    }
    /// <summary>
    /// 모드 선택 UI -> 세팅 UI로 변경 시
    /// </summary>
    public void Switching_Select_to_Setting()
    {
        select_UI.SetActive(false);
        setting_UI.SetActive(true);
    }
    /// <summary>
    /// 세팅 UI -> 모드 선택 UI로 변경 시
    /// </summary>
    public void Switching_Setting_to_Select()
    {
        setting_UI.SetActive(false);
        select_UI.SetActive(true);
    }
    /// <summary>
    /// 카메라에 붙어있는 애니메이션 이벤트에서 작동할 테블릿을 로직을 시작하는 함수
    /// 로딩화면 보여주고 1.5초뒤에 알아서 사라지게하는 함수
    /// 로딩화면 사라지고 SelectUI 자동으로 보여줌
    /// </summary>
    public void Tablet_Logic_Start()
    {
        loading_UI.SetActive(true);
        StartCoroutine(Switching_Select_after_loading());
    }
}
