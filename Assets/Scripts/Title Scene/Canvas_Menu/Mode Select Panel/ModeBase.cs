using UnityEngine;

public abstract class ModeBase : MonoBehaviour
{
    protected bool isPreparing;

    public bool IsPreparing => isPreparing;


    public abstract void Setup();


    public virtual void PreparingMode() { }
}

