using UnityEngine;

namespace SnowLib.Unity
{
    public abstract class SingletonBehaviourOnScene<T> : MonoBehaviour where T : SingletonBehaviourOnScene<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
        }
    }
}
