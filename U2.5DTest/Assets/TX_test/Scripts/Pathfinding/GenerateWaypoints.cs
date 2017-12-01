using UnityEngine;
using System.Collections;

namespace TencentProgramerTest {
    public class GenerateWaypoints : MonoBehaviour {
        public GameObject waypointPrefab;
        public Transform startPoint;
        public Transform endPoint;
        public float interval = 5f;
        public float distanceToCheck = 0.3f;
        public float minDistanceToCollider = 0.1f;
        // Use this for initialization
        void Awake() {

            Vector3 distance = endPoint.position - startPoint.position;
            Vector3 dir = distance.normalized;
            GameObject waypoint;
            for (float f = 0; f < distance.magnitude; f += interval)
            {
                waypoint = GameObject.Instantiate<GameObject>(waypointPrefab);
                waypoint.transform.position = startPoint.position + f * dir;
            }
            waypoint = GameObject.Instantiate<GameObject>(waypointPrefab);
            waypoint.transform.position = endPoint.position;
        }
    }
}
