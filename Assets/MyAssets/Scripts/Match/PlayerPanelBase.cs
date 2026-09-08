using UnityEngine;

namespace Assets.MyAssets.Scripts.Match
{
    public abstract class PlayerPanelBase : MonoBehaviour
    {
        public virtual void InitializePanel() { }
        public virtual void UpdatePieceCount(int count) { }
        public virtual void UpdateTimer(float time) { }
    }
}
