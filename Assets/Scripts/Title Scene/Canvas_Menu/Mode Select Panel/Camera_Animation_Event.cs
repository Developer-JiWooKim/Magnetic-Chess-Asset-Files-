using UnityEngine;

public class Camera_Animation_Event : MonoBehaviour
{
    [SerializeField]
    private Tablet_Logic tablet_logic;

    /// <summary>
    /// 로딩 화면 보여주는 함수
    /// </summary>
    public void Loading_UI_Show()
    {
        tablet_logic.Tablet_Logic_Start();
    }
}
