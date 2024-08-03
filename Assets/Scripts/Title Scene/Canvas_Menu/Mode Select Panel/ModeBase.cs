using UnityEngine;

public abstract class ModeBase : MonoBehaviour
{
    protected bool isPreparing;

    public bool IsPreparing => isPreparing;

    // ��ư �ʱ�ȭ �޼ҵ�
    public abstract void Setup();

    // �غ����� ������� �˻� �� �׿� �´� �ൿ�� �ϴ� virtual �޼ҵ�
    public virtual void PreparingMode() { }
}

