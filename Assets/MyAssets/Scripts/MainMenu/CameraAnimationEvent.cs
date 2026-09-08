using UnityEngine;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class CameraAnimationEvent : MonoBehaviour
    {
        [SerializeField] private TabletLogic _tabletLogic;

        public void LoadingUIShow()
        {
            _tabletLogic.TabletLogicStart();
        }
    }
}