using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Singleton Pattern
    /// 상속받는 쪽에서 Awake()를 직접 정의하면 이 셋업이 실행되지 않으므로,
    /// 추가 초기화가 필요하면 Awake()를 override 하고 base.Awake()를 먼저 호출할 것.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.Log(typeof(T).Name + " instance is null!!");
                }
                return instance;
            }
        }

        /// <summary>
        /// 씬이 바뀌어도 유지할지 여부. 씬에 종속된 매니저(예: GameDirector)는 false로 override.
        /// </summary>
        protected virtual bool IsPersistent => true;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;

                if (IsPersistent)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            else if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
