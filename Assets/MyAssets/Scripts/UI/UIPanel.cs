using UnityEngine;

namespace Assets.MyAssets.Scripts.UI
{
    public abstract class UIPanel : MonoBehaviour
    {
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}