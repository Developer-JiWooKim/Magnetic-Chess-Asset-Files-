using UnityEngine;

namespace Assets.MyAssets.Scripts.MainMenu
{
    /// <summary>
    /// 카메라 애니메이션(CameraMoving.anim)의 이벤트를 UI로 넘긴다.
    /// 메소드 이름은 애니메이션 클립에 문자열로 박혀 있으므로 바꾸면 클립도 함께 고쳐야 한다.
    /// </summary>
    public sealed class CameraAnimationEvent : MonoBehaviour
    {
        [SerializeField] private TitleUIController _titleUIController;

        /// <summary>CameraMoving.anim의 이벤트가 이 이름으로 부른다.</summary>
        public void OnCameraArrivedAtMenu()
        {
            if (_titleUIController == null)
            {
                Debug.LogError(nameof(CameraAnimationEvent) + "._titleUIController 참조가 비어 있다.", this);
                return;
            }

            _titleUIController.OnCameraArrivedAtMenu();
        }
    }
}
