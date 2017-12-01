using UnityEngine;
using System.Collections;
using TestTools;

namespace TencentProgramerTest {
    public class BulletManager : MonoBehaviour {
        public static BulletManager instance;

        public ObjectPool bulletPool;

        // Use this for initialization
        void Start() {
            instance = this;
        }
        void Update() {

        }

        public BulletBehaviour Fire(Vector3 pos, Vector3 dir) {
            GameObject bullet = bulletPool.GetObject(false);
            bullet.GetComponent<TrailRenderer>().Clear();
            BulletBehaviour bulletScript = bullet.GetComponent<BulletBehaviour>();
            bulletScript.Setup();
            bulletScript.velocity = dir*50;
            bullet.transform.position = pos;
            
            return bulletScript;
        }
    }
}