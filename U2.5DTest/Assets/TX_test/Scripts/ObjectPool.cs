using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TestTools {
    public class ObjectPool : MonoBehaviour {

        public GameObject objectPrefab;

        public Stack<GameObject> pooledObjects;

        public int preheatAmountToBuffer;

        protected Transform containerTransform;

        void Awake() {
        }

        // Use this for initialization
        void Start() {
            containerTransform = transform;

            pooledObjects = new Stack<GameObject>();

            for (int n = 0; n < preheatAmountToBuffer; n++) {
                GameObject newObj = Instantiate(objectPrefab) as GameObject;
                newObj.name = objectPrefab.name;
                PoolObject(newObj);
            }
        }
        public GameObject GetObject(bool onlyPooled) {
            if (pooledObjects.Count > 0) {
                GameObject pooledObject = pooledObjects.Pop();
                pooledObject.transform.parent = null;
                pooledObject.SetActive(true);

                return pooledObject;
            } else if (!onlyPooled) {
                return Instantiate(objectPrefab) as GameObject;
            }
            return null;
        }

        public void PoolObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(containerTransform);
            pooledObjects.Push(obj);
        }

    }
}
