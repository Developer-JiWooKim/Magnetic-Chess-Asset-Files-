using UnityEngine;

namespace Assets.MyAssets.Scripts.Core
{
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
        /// 씬이 바뀌어도 유지할지 여부.
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
