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
    }
}
