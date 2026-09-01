using UnityEngine;

namespace Assets.Scripts.Match
{
    public abstract class Player_Panel_Base : MonoBehaviour
    {
        public virtual void Initiallize_Panel() { }
        public virtual void Update_PieceCount(int _count) { }
        public virtual void Update_Timer(float _time) { }
    }
}
