using UnityEngine;

namespace Dots.Utils
{
    /// <summary>
    ///     Utility class to make any class a singleton in Unity.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        /// <summary>
        ///     To get the instance available and access the class.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                var existingInstance = FindObjectOfType<T>();
                if (existingInstance != null)
                {
                    _instance = existingInstance;
                }
                else
                {
                    Debug.LogError(string.Format("Singleton<{0}> couldn't be found.", typeof(T).Name));
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        /// <summary>
        ///     To see if the class has an instance or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> If the class has an instance; otherwise, <c>false</c>.
        /// </value>
        public static bool HasInstance
        {
            get { return _instance != null; }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                Instance = (T) this;
            }
            else if (_instance != this)
            {
                Debug.LogError(string.Format("An Singleton of type {0} already exists in the scene!", GetType()),
                    Instance);
            }
        }

        protected virtual void OnEnable()
        {
            if (_instance != null)
            {
                return;
            }
            Instance = (T) this;
        }

        protected virtual void OnDestroy()
        {
            if (HasInstance && (Instance == this))
            {
                Instance = null;
            }
        }
    }
}