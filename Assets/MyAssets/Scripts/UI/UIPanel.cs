using UnityEngine;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 켜고 끌 수 있는 UI 조각의 공통 바탕.
    ///
    /// 버튼 배선을 Awake에 적으면, 그 패널이 씬에 <b>비활성 상태로</b> 놓여 있을 때 Awake가 돌지 않아
    /// 배선이 통째로 빠진다. 처음 열 때까지 아무 증상이 없다가 버튼만 조용히 먹통이 된다.
    /// 그래서 배선은 Awake와 Show 양쪽에서 부르되 한 번만 실행되도록 여기서 묶는다.
    /// 파생 클래스는 Awake 대신 Bind / Unbind를 적는다.
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        private bool _isBound;

        protected virtual void Bind() { }
        protected virtual void Unbind() { }

        protected void EnsureBound()
        {
            if (_isBound)
            {
                return;
            }

            _isBound = true;
            Bind();
        }

        protected virtual void Awake() => EnsureBound();

        protected virtual void OnDestroy()
        {
            if (_isBound)
            {
                Unbind();
            }
        }

        public virtual void Show()
        {
            EnsureBound();
            gameObject.SetActive(true);
        }

        public virtual void Hide() => gameObject.SetActive(false);
    }
}
