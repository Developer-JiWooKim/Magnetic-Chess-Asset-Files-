using UnityEngine;

namespace Assets.MyAssets.Scripts.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.Log(typeof(T).Name + " _instance is null!!");
                }
                return _instance;
            }
        }

        /// <summary>
        /// 씬이 바뀌어도 유지할지 여부.
        /// </summary>
        protected virtual bool IsPersistent => true;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (IsPersistent)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// 씬을 다시 읽어 판을 재시작하게 되면서(GameManager.ReloadMatch) 이 정리가 필요해졌다.
        /// static 필드는 씬 로드로 초기화되지 않으므로, 파괴된 오브젝트를 가리킨 채 남는다.
        /// Unity가 파괴된 객체를 null처럼 보이게 해 주긴 하지만 그 사실에 기대지 않고 직접 끊는다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
