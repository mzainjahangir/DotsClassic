using System.Collections.Generic;
using UnityEngine;

namespace Dots.Utils
{
    /// <summary>
    ///     Class used to create and manage pool-able objects for better performance.
    /// </summary>
    public class PoolManager : Singleton<PoolManager>
    {
        // TODO: Add a PoolableObject script on pool-able objects.
        [SerializeField] private GameObject _poolableObject;

        /// <summary>
        ///     The number of objects you want to be pooled before the game begins.
        /// </summary>
        private int _numberOfObjects;

        private Queue<GameObject> _availableObjects;

        protected override void Awake()
        {
            base.Awake();

            _numberOfObjects = GlobalConstants.MaxRows * GlobalConstants.MaxColumns * 2;
            _availableObjects = new Queue<GameObject>();

            for (var i = 0; i < _numberOfObjects; i++)
            {
                CreatePoolableObject();
            }
        }

        private void CreatePoolableObject()
        {
            var createdObject = Instantiate(_poolableObject);
            createdObject.transform.SetParent(transform);
            createdObject.SetActive(false);
            _availableObjects.Enqueue(createdObject);
        }

        /// <summary>
        ///     Gets a pooled game object.
        /// </summary>
        public GameObject GetPoolableObject()
        {
            if (_availableObjects.Count <= 0)
            {
                CreatePoolableObject();
            }

            var item = _availableObjects.Dequeue();
            item.SetActive(true);
            return item;
        }

        /// <summary>
        ///     Puts the game object back into pool.
        /// </summary>
        public void DestroyObject(GameObject poolableObject)
        {
            if (_availableObjects.Contains(poolableObject))
            {
                return;
            }
            poolableObject.SetActive(false);
            poolableObject.transform.SetParent(transform);
            _availableObjects.Enqueue(poolableObject);
        }
    }
}
