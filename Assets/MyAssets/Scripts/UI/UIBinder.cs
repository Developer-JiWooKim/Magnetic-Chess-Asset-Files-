using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Core;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 버튼·슬라이더·드롭다운을 코드에서 연결한다.
    ///
    /// 왜 인스펙터가 아니라 코드인가: 인스펙터의 OnClick 목록은 호출할 메소드 이름을
    /// 문자열로 저장한다. 이름을 바꾸거나 지워도 컴파일러가 알려주지 않고,
    /// 실행 중에 버튼만 조용히 먹통이 된다. 코드로 연결하면 컴파일러가 검사한다.
    ///
    /// 남는 것은 "어느 버튼인가"를 가리키는 참조뿐인데, 비어 있으면 여기서 어느 필드가
    /// 비었는지 이름과 함께 알려주므로 조용히 넘어가지 않는다.
    ///
    /// 클릭 피드백은 여기서 자동으로 붙는다. 모든 버튼이 소리를 내야 한다는 규칙을
    /// 핸들러마다 한 줄씩 적어두면 언젠가 빠뜨린다. 배선하는 길목이 하나뿐이니
    /// 여기에 두면 빠뜨릴 수가 없다.
    /// </summary>
    internal static class UIBinder
    {
        public static void Bind(Button button, UnityAction handler, Object owner, string fieldName,
            SoundManager.SfxName sfx = SoundManager.SfxName.ButtonPress)
        {
            if (button == null)
            {
                LogMissing(owner, fieldName);
                return;
            }

            button.onClick.AddListener(() => PlaySfx(sfx));
            button.onClick.AddListener(handler);
        }

        public static void Bind(Slider slider, UnityAction<float> handler, Object owner, string fieldName,
            SoundManager.SfxName sfx = SoundManager.SfxName.Slider)
        {
            if (slider == null)
            {
                LogMissing(owner, fieldName);
                return;
            }

            slider.onValueChanged.AddListener(_ => PlaySfx(sfx));
            slider.onValueChanged.AddListener(handler);
        }

        public static void Bind(TMP_Dropdown dropdown, UnityAction<int> handler, Object owner, string fieldName,
            SoundManager.SfxName sfx = SoundManager.SfxName.DropdownPress)
        {
            if (dropdown == null)
            {
                LogMissing(owner, fieldName);
                return;
            }

            dropdown.onValueChanged.AddListener(_ => PlaySfx(sfx));
            dropdown.onValueChanged.AddListener(handler);
        }

        /// <summary>
        /// 이 프로젝트에서 런타임 리스너를 붙이는 곳은 UIBinder뿐이라 전부 지워도 안전하다.
        /// (RemoveAllListeners는 인스펙터에 저장된 호출은 건드리지 않는다.)
        /// 소리 리스너는 람다라 개별 RemoveListener로는 지울 수 없어서 이렇게 한다.
        /// </summary>
        public static void Unbind(Button button)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        public static void Unbind(Slider slider)
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveAllListeners();
            }
        }

        public static void Unbind(TMP_Dropdown dropdown)
        {
            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveAllListeners();
            }
        }

        private static void PlaySfx(SoundManager.SfxName sfx)
        {
            SoundManager soundManager = SoundManager.Instance;

            // 소리가 안 나는 것 때문에 버튼 동작 자체가 막히면 안 된다.
            // (Instance가 비면 Singleton<T>가 이미 로그를 남긴다.)
            if (soundManager != null)
            {
                soundManager.PlaySFX(sfx);
            }
        }

        private static void LogMissing(Object owner, string fieldName)
        {
            // 두 번째 인자를 넘기면 콘솔에서 클릭했을 때 해당 오브젝트가 선택된다.
            Debug.LogError(owner.GetType().Name + "." + fieldName + " 참조가 비어 있어 연결하지 못했다. 인스펙터에서 연결해야 한다.", owner);
        }
    }
}
