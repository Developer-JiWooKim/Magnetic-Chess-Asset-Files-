using System.Collections;
using UnityEngine;

public class Tablet_Logic : MonoBehaviour
{
    [SerializeField]
    private GameObject loading_UI; // �ε� UI

    // ��� ���� ȭ��
    [SerializeField]
    private GameObject select_UI; // ��� ���� UI(���� ����)

    [SerializeField]
    private GameObject setting_UI; // ���� UI(���� ����) 

    [SerializeField]
    private ModeSelectPanel modeSelectPanel;

    /// <summary>
    /// ī�޶� ���� �� ȭ���� ������ ������ ���� �ѹ��� ���� �ڷ�ƾ �Լ�
    /// </summary>
    private IEnumerator Switching_Select_after_loading()
    {
        yield return new WaitForSeconds(1.5f);
        loading_UI.SetActive(false); // �ε� ȿ�� �Ⱥ��̰�
        select_UI.SetActive(true);  // �׺��� select UI���̰�
        modeSelectPanel.ShowPanel();    // ModeSelect Panel ���̰�
    }
    /// <summary>
    /// ��� ���� UI -> ���� UI�� ���� ��
    /// </summary>
    public void Switching_Select_to_Setting()
    {
        select_UI.SetActive(false);
        setting_UI.SetActive(true);
    }
    /// <summary>
    /// ���� UI -> ��� ���� UI�� ���� ��
    /// </summary>
    public void Switching_Setting_to_Select()
    {
        setting_UI.SetActive(false);
        select_UI.SetActive(true);
    }
    /// <summary>
    /// ī�޶� �پ��ִ� �ִϸ��̼� �̺�Ʈ���� �۵��� �׺��� ������ �����ϴ� �Լ�
    /// �ε�ȭ�� �����ְ� 1.5�ʵڿ� �˾Ƽ� ��������ϴ� �Լ�
    /// �ε�ȭ�� ������� SelectUI �ڵ����� ������
    /// </summary>
    public void Tablet_Logic_Start()
    {
        loading_UI.SetActive(true);
        StartCoroutine(Switching_Select_after_loading());
    }
}
