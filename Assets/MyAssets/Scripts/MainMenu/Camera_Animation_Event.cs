using UnityEngine;

namespace Assets.MyAssets.Scripts.MainMenu
{
    public sealed class Camera_Animation_Event : MonoBehaviour
    {
        [SerializeField]
        private Tablet_Logic tablet_logic;

        public void Loading_UI_Show()
        {
            tablet_logic.Tablet_Logic_Start();
        }
    }
}
