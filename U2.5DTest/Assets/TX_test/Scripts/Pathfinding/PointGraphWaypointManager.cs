using UnityEngine;
using System.Collections;
using Pathfinding;

namespace TencentProgramerTest {
    public class PointGraphWaypointManager : MonoBehaviour {
        public AstarPath aStar;

        void Awake() {

        }

        void CollectAllWaypoints() {
            PointGraphWaypointScript[] waypoints = FindObjectsOfType<PointGraphWaypointScript>();

            foreach (var waypoint in waypoints) {
                waypoint.transform.SetParent(transform);
            }
        }



        // Use this for initialization
        void Start() {
            CollectAllWaypoints();
            aStar.Scan();
        }

        // Update is called once per frame
        void Update() {

        }
    }
}