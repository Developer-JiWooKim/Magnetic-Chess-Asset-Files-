using UnityEngine;

public class Camera_Animation_Event : MonoBehaviour
{
    [SerializeField]
    private Tablet_Logic tablet_logic;

    /// <summary>
    /// �ε� ȭ�� �����ִ� �Լ�
    /// </summary>
    public void Loading_UI_Show()
    {
        tablet_logic.Tablet_Logic_Start();
    }
}
