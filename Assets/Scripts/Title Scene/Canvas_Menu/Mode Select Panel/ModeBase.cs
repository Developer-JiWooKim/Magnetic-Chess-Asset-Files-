using UnityEngine;

public abstract class ModeBase : MonoBehaviour
{
    protected bool isPreparing;

    public bool IsPreparing => isPreparing;

    // 버튼 초기화 메소드
    public abstract void Setup();

    // 준비중인 모드인지 검사 후 그에 맞는 행동을 하는 virtual 메소드
    public virtual void PreparingMode() { }
}

