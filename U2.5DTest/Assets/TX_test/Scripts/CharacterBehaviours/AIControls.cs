using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using System.Collections;
using TestTools;

namespace TencentProgramerTest {

    [RequireComponent(typeof(Seeker))]
    
    public class AIControls : MonoBehaviour {
        public enum AIState {
            KILL_CLOSEST,
            HUNT,
            PATROL,
            FLEE,
            FOLLOW_TARGET,
        }
        public Rect worldBounds = new Rect(-50, 0, 100, 50);
        public AIState state;
        public List<Transform> enemies;
        public List<Transform> allies;
        [Tooltip("The position of the eyes where raycasts will be made to determine if things are visible to the character")]
        public Transform eyes;
        public LayerMask enemiesLayers;
        public LayerMask terrainLayers;
       

        private CharacterController2D character;
        [SerializeField]
        private Transform currentEnemy = null;
        [SerializeField]
        private Transform currentTarget = null;
        [Header("Turn on to have the character display her thoughts in the console")]
        public bool verboseDebug = true;

        #region Control variables
        
        [Header("Hover over variables to display tooltip"), Tooltip("When the angle to the next waypoint is above this value the AI will jump towards it")]
        public float angleToJump = 55;
        [Header("How far the character is able to see")]
        public float maxViewDistance = 15f;
        [Tooltip("This is the distance in meters the AI will try to get within of its enemy when attacking it")]
        public float targetEnemyDistance = 5;
        [Tooltip("This is the distance in meters the AI digging tool can be used.")]
        public float maxDigDistance = 2;
        [Tooltip("This is the distance in meters the AI will try to get within of its target position to consider having reached it")]
        public float targetDistance = 1;

        [Tooltip("Minimum distance a fleeing character wants to keep between her and her opponent")]
        public float minFleeDistance = 15f;
        [Tooltip("After planning a path to an enemy the path will not be replanned unless the enemy moves more than this distance from the end point of the planned path.")]
        public float maxDistanceForEnemyToMoveBeforeRequestingNewPath = 5;
        [Header("The minimum distance we need to close towards our target before we consider our pathfinding a failure")]
        public float minGetCloserPerSecond = 0.1f;
        
        #endregion
       
        private Rigidbody2D body;
        private Rigidbody body3D;
        public float thinkPerSecond = 5;

        #region Feelers
        //These can be seen as senses for the character
        [Header("These are the boxes used to detect obstacles in front of the character. You can see them in the editor window during gameplay")]
        public Vector2 lowerFeelerSize = new Vector2(0.1f, 0.5f);
        public Vector2 lowerFeelerPos = new Vector2(0.1f, 0.5f);
        public bool isDetectingLowObstacle = false;
        public Vector2 upperFeelerSize = new Vector2(0.1f, 0.5f);
        public Vector2 upperFeelerPos = new Vector2(0.1f, 0.125f);
        public bool isDetectingHighObstacle = false;
        public LayerMask feelerDetectMask;
        #endregion

        #region Movement and control variables
        //These variables are set in Think() and are acted upon in Act()
        [SerializeField, Header("These variables are set in Think() and are acted upon in Act()")]
        private float run;
        [SerializeField]
        private bool doJump = false;

        [SerializeField]
        private bool doFire = false;

        [SerializeField]
        private Vector3 aimDirection;
        [SerializeField]
        private float nextTargetWantedDistance = 1;
        #endregion

        #region Pathfinding settings
        [Header("Pathfinding settings")]
        
        public SidescrollerPathfinding pathfinding;
        #endregion

        #region Properties
        protected bool is2D { get { return body != null; } }
        protected Vector2 position { get { return transform.position; } }
        protected Vector2 velocity { get { if (is2D) return body.velocity; else return body3D.velocity; } }
        #endregion

        // Use this for initialization
        public void Start() {
#if !UNITY_EDITOR
        verboseDebug = false;
#endif

            if (verboseDebug)
                Debug.Log(name + ": I have verbose debugging set to true so I will tell you what I am thinking. Turn this off in my AIControls script if you find it annoying");
            pathfinding.Setup(this);
            character = GetComponent<CharacterController2D>();
            body = GetComponent<Rigidbody2D>();
            body3D = GetComponent<Rigidbody>();
            StartCoroutine(Think());
        }

        // Update is called once per frame
        void FixedUpdate() {
            Act();
        }

        /// <summary>
        /// This method handles interaction with the body. Everything should already be planned.
        /// </summary>
        private void Act() {
            DetectObstacles();//TODO: move this to Think when we do not need to debug it
            if (doJump) {
                character.Jump();
                doJump = false;
            }
            if (doFire)
                character.Fire();
            character.Run(run);
        }

        /// <summary>
        /// Put any decision making in here. We can set this to run less often if we want to spare some CPU cycles
        /// </summary>
        /// <returns></returns>
        private IEnumerator Think() {
            WaitForSeconds wait = new WaitForSeconds(1f / thinkPerSecond);

            while (true) {
                doFire = false;
                doJump = false;

                SelectClosestEnemy();

                //Shoot or dig depending on current needs
                if (ShootIfEnemyVisible()) {

                } else if (DigIfNeeded()) {

                }

                switch (state) {
                    case AIState.KILL_CLOSEST:
                        ThinkLikeAKiller();
                        break;
                    case AIState.HUNT:
                        ThinkLikeAHunter();
                        break;
                    case AIState.PATROL:
                        ThinkLikeAPatroller();
                        break;
                    case AIState.FLEE:
                        ThinkLikeACoward();
                        break;
                    case AIState.FOLLOW_TARGET:
                        ThinkLikeAFollower();
                        break;
                    default:
                        break;
                }
                
                yield return wait;
            }
        }

        public void DetectObstacles(float angle = 0) {
            //if (run == 0) return;
            Vector2 direction;
            if (run > 0) {
                direction = Vector2.right;
                upperFeelerPos.x = Mathf.Abs(upperFeelerPos.x);
                lowerFeelerPos.x = Mathf.Abs(lowerFeelerPos.x);
            } else {
                direction = Vector2.left;
                upperFeelerPos.x = -Mathf.Abs(upperFeelerPos.x);
                lowerFeelerPos.x = -Mathf.Abs(lowerFeelerPos.x);
            }
            if (is2D) {
                RaycastHit2D hit = Physics2D.BoxCast(position + upperFeelerPos, upperFeelerSize, angle, direction, 0, feelerDetectMask);
                if (hit.collider != null) {
                    isDetectingHighObstacle = true;
                    DebugDrawPhysics.DebugDrawBoxCast(position + upperFeelerPos, upperFeelerSize, angle, direction, 0, Color.red);
                } else {
                    DebugDrawPhysics.DebugDrawBoxCast(position + upperFeelerPos, upperFeelerSize, angle, direction, 0, Color.green);
                    isDetectingHighObstacle = false;
                }

                hit = Physics2D.BoxCast(position + lowerFeelerPos, lowerFeelerSize, angle, direction, 0, feelerDetectMask);
                if (hit.collider != null) {
                    DebugDrawPhysics.DebugDrawBoxCast(position + lowerFeelerPos, lowerFeelerSize, angle, direction, 0, Color.red);
                    isDetectingLowObstacle = true;
                } else {
                    DebugDrawPhysics.DebugDrawBoxCast(position + lowerFeelerPos, lowerFeelerSize, angle, direction, 0, Color.green);
                    isDetectingLowObstacle = false;
                }
            } else {

                //There's a difference between how 2D and 3D handles boxcast. In3D the boxcast cannot initially touch the object or it will return false
                //while in 2D it will return true if there's an object present in the cast. Therefore we must set the initial box inside the character
                //where we are sure to not hit any terrain
                RaycastHit hit;
                Vector3 upperfeelerSize3D = upperFeelerSize;
                Vector3 upperfeelerPos3D = upperFeelerPos;
                upperfeelerSize3D.z = 1;
                Vector3 lowerfeelerSize3D = lowerFeelerSize;
                Vector3 lowerfeelerPos3D = lowerFeelerPos;
                lowerfeelerSize3D.z = 1;

                //Upper box
                float distance = Mathf.Abs(upperfeelerPos3D.x);
                upperfeelerPos3D.x = 0;
                Physics.BoxCast(transform.position + upperfeelerPos3D, upperfeelerSize3D / 2, direction, out hit, Quaternion.Euler(0,0, angle), distance, feelerDetectMask);
                if (hit.collider != null) {
                    isDetectingHighObstacle = true;
                    DebugDrawPhysics.DebugDrawBoxCast(transform.position + upperfeelerPos3D, upperFeelerSize, angle, direction, distance, Color.red);
                } else {
                    DebugDrawPhysics.DebugDrawBoxCast(transform.position + upperfeelerPos3D, upperFeelerSize, angle, direction, distance, Color.green);
                    isDetectingHighObstacle = false;
                }

                //Lower box
                distance = Mathf.Abs(lowerfeelerPos3D.x);
                lowerfeelerPos3D.x = 0;
                Physics.BoxCast(transform.position + lowerfeelerPos3D, lowerFeelerSize / 2, direction, out hit, Quaternion.Euler(0, 0, angle), distance, feelerDetectMask);
                
                if (hit.collider != null) {
                    DebugDrawPhysics.DebugDrawBoxCast(transform.position + lowerfeelerPos3D, lowerFeelerSize, angle, direction, distance, Color.red);
                    isDetectingLowObstacle = true;
                } else {
                    DebugDrawPhysics.DebugDrawBoxCast(transform.position + lowerfeelerPos3D, lowerFeelerSize, angle, direction, distance, Color.green);
                    isDetectingLowObstacle = false;
                }
            }
            
        }

        void ThinkLikeAHunter() {
            nextTargetWantedDistance = targetEnemyDistance;
            

            if (currentTarget != null) {
                ApproachTarget();
            } else {
                Transform newTarget = GetRandomEnemy();
                if (newTarget != null)
                    currentTarget = newTarget.transform;
            }
        }

        void ThinkLikeAFollower() {
            nextTargetWantedDistance = 1;
            
            if (currentTarget != null) {
                ApproachTarget();
            } else {
                Transform newTarget = GetRandomAlly();
                if (newTarget != null)
                    currentTarget = newTarget.transform;
            }
        }

        void ThinkLikeAKiller() {
            nextTargetWantedDistance = targetEnemyDistance;
            if (currentEnemy != null) {
                ApproachEnemy();
                
            }
           
        }

        void ThinkLikeACoward() {
            nextTargetWantedDistance = targetEnemyDistance;
           
            if (currentEnemy != null && GetDistanceToEnemy() < minFleeDistance) {
                FleeEnemy();
            } else {
                SelectClosestAlly();
                FollowTarget();
            }
        }

        void ThinkLikeAPatroller() {
            nextTargetWantedDistance = targetDistance;
           
            Patrol();
        }



        public static AIState GetRandomAIState() {
            return ((AIState)Random.Range(0, System.Enum.GetValues(typeof(AIState)).Length));
        }

        public void EnterAIState(AIState newState) {
            switch (state) {
                case AIState.KILL_CLOSEST:
                    if (verboseDebug)
                        Debug.Log(name + ": I am a killer, I will try to find the closest enemy and kill him");
                    break;
                case AIState.HUNT:
                    if (verboseDebug)
                        Debug.Log(name + ": I am a hunter, I will try to find a specific enemy and kill him");
                    break;
                case AIState.PATROL:
                    if (verboseDebug)
                        Debug.Log(name + ": I am a patrolman, I will walk to random waypoints");
                    break;
                case AIState.FLEE:
                    if (verboseDebug)
                        Debug.Log(name + ": I am a coward, whenever I see an enemy I will try to find a safe place to hide");
                    break;
                default:
                    break;
            }

            state = newState;
        }
        
        Transform GetRandomEnemy() {
            if (enemies.Count == 0)
                return null;
            else
                return enemies[Random.Range(0, enemies.Count - 1)];
        }

        Transform GetRandomAlly() {
            if (allies.Count == 0)
                return null;
            else
                return allies[Random.Range(0, allies.Count - 1)];
        }

        /// <summary>
        /// Selects the closest ally and sets it as target
        /// </summary>
        void SelectClosestAlly() {
            //This will select the closest ally by looping through the list and comparing distances 
            Transform closest = SelectClosest(allies, transform);
            if (closest != null) {
                currentTarget = closest.transform;
                pathfinding.targetPosWhenRequesting = currentTarget.position;
            }
        }

        /// <summary>
        /// Selects closest enemy and sets it as current enemy
        /// </summary>
        void SelectClosestEnemy() {
            //This will select the closest enemy by looping through the list and comparing distances 
            Transform closest = SelectClosest(enemies);
            if (closest != null)
                SelectEnemy(closest);
        }

        /// <summary>
        /// This will select the closest by looping through the list and comparing distances 
        /// 
        /// If the character itself is included in the list and you do not want it to be selected
        /// then pass it as the ignore parameter
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        Transform SelectClosest(List<Transform> list, Transform ignore = null) {
            //This will select the closest by looping through the list and comparing distances 
            float currentDistance = float.PositiveInfinity;
            Transform closest = null;
            foreach (var item in list) {
                if (item != null && item != ignore) {
                    
                    float newDist = Vector2.Distance(item.position, transform.position);
                    if (newDist < currentDistance) {
                        currentDistance = newDist;
                        closest = item;
                    }
                }
            }
            return closest;
        }

        void SelectTarget(Transform newTarget) {
            if (newTarget == currentTarget)
                return;
            else
                pathfinding.SetPathUnvalid();

            currentTarget = newTarget;

            if (verboseDebug)
                Debug.Log(name + ": I selected a new target: " + currentTarget.name);
        }

        void SelectEnemy(Transform enemy) {
            //Check if this is a different enemy from the one we already had
            if (enemy != currentEnemy) {
                //If it is a new and we have one we need to deselect it
                if (currentEnemy != null)
                    DeselectEnemy(currentEnemy);
                //Set path to null so a new one will be requested
                pathfinding.SetPathUnvalid();
            } else {
                return;
            }
            currentEnemy = enemy;
            currentEnemy.GetComponent<TakeDamageBehaviour>().OnDeath.AddListener(EnemyDiedCallback);
            if (verboseDebug)
                Debug.Log(name + ": I selected a new enemy: " + enemy.name);
        }

        void DeselectEnemy(Transform enemy) {
            if (enemy != currentEnemy) {
                Debug.LogWarning("Tried to deselect a target we did not have. Check this.");
                return;
            }
            currentEnemy.GetComponent<TakeDamageBehaviour>().OnDeath.RemoveListener(EnemyDiedCallback);
            currentEnemy = null;
        }

        void EnemyDiedCallback(TakeDamageBehaviour target) {
            if (verboseDebug)
                Debug.Log(name + ": my enemy died: " + target.name);
            DeselectEnemy(target.transform);
        }

        

        void ApproachTarget() {
            if (pathfinding.HasPath()) {//If we have no path then request one
                //First, if we are too close (and can see the target) then we need to create some distance!
                if (GetDistanceToTarget() < nextTargetWantedDistance && 
                    CheckLineOfSight(currentTarget, nextTargetWantedDistance)) {
                    run = GetDirectionToTarget().x < 0 ? -1 : 1;
                    return;
                }

                if (IsPathToTargetStillValid()) {
                    if (FollowPath()) {
                        if (verboseDebug)
                            Debug.Log(name + ": " + state + ": I reached the end of my path so requested a new one");
                    }
                } else {
                    if (verboseDebug)
                        Debug.Log(name + ": " + state + ": my path was no longer valid so I requested a new one");
                    pathfinding.SetPathUnvalid();
                }
            } else {//We have a path
                    //First, if we are too close then we need to create some distance!
                
                if (!pathfinding.hasRequestedPath) {
                    pathfinding.RequestPath(currentTarget);
                }
                if (currentTarget.position.x > transform.position.x)
                    run = 1;
                else
                    run = -1;
            }
        }

        void ApproachEnemy() {
            //First, check if we can see the enemy and if we are close enough to hit!
            if (CheckLineOfSight(currentEnemy, maxViewDistance) && GetDistanceToEnemy() < targetEnemyDistance) {
                //if we are too close then we need to create some distance!
                if (GetDistanceToEnemy() < nextTargetWantedDistance) {
                    run = (-GetDirectionToEnemy().x) < 0 ? -1 : 1;
                    return;
                } else {
                    //We're good.. can focus on shooting and do not need to move
                }
            } else if (pathfinding.HasPath()) {//We have a path
               
                //Check if path is valid
                if (IsPathToEnemyStillValid()) {
                    //Follow the path to the end (Note that we didn't keep running closer if we got too close to the enemy though)
                    if (FollowPath()) {
                        if (verboseDebug)
                            Debug.Log(name + ": " + state + ": I reached the end of my path so I'll just stay here and shoot for a while");
                    }
                } else {//Path NOT valid so request a new one
                    pathfinding.SetPathUnvalid();
                }
            } else {//If we have no path then request one
                if (!pathfinding.hasRequestedPath) {
                    pathfinding.RequestPath(currentEnemy);
                }
                //but we can't wait for that path for too long so start running while we wait
                if (currentEnemy.position.x > transform.position.x)
                    run = 1;
                else
                    run = -1;
            } 
        }

        /// <summary>
        /// Follows a target. Target may be moving
        /// Stops when within a certain distance of it
        /// </summary>
        void FollowTarget() {
            //If we're close enough then don't move
            if (GetDistanceToTarget() < nextTargetWantedDistance)
                return;
            //If we have a path then follow it
            if (pathfinding.HasPath()) {
                //Check if path is valid 
                if (IsPathToTargetStillValid()) {
                    //Follow the path to the end
                    if (FollowPath()) {
                        //Reached the end
                        if (verboseDebug)
                            Debug.Log(name + ": " + state + ": I reached the end of my path but I am still too far from my ally so I'll request a new path");
                    }
                } else {
                    pathfinding.SetPathUnvalid();
                    if (verboseDebug)
                        Debug.Log(name + ": " + state + ": Path was unvalid so requested a new one");
                }
            } else {//If we have no path then request one
                if (!pathfinding.hasRequestedPath) {
                    pathfinding.RequestPath(currentTarget);
                }
            }
        }

        /// <summary>
        /// Goes to a random position inside world bounds
        /// Will currently not stop for enemies on the way
        /// </summary>
        void Patrol() {
            //If we have a path then follow it
            if (pathfinding.HasPath()) {
                //Follow the path to the end
                if (FollowPath()) {
                    //Reached the end
                    pathfinding.SetPathUnvalid();
                    if (verboseDebug)
                        Debug.Log(name + ": " + state + ": I reached the end of my path so I'll immediately request a new patrol path to a random position");
                }
            } else {//If we have no path then request one
                if (!pathfinding.hasRequestedPath)
                    pathfinding.RequestRandomPath(worldBounds);
            }
                
        }

        /// <summary>
        /// Will find a position that is 50 meters away from the enemy and try to hide there
        /// </summary>
        void FleeEnemy() {
            if (pathfinding.HasPath()) {//If we have no path then request one
                //Check if path is valid and we are still on it
                if (IsPathToEnemyStillValid()) {
                    //Follow the path to the end
                    if (FollowPath()) {
                        //Reached the end
                        if (verboseDebug)
                            Debug.Log(name + ": " + state + ": I reached the end of my path so I'll just stay here and hide for a while");
                    }
                } else {
                    pathfinding.SetPathUnvalid();
                }
                
            } else {//We have no path
                if (!pathfinding.hasRequestedPath) {
                    //Determine where to go
                    Vector2 dir = -GetDirectionToEnemy();
                    dir *= 50;
                    pathfinding.targetPosWhenRequesting = currentEnemy.position;
                    pathfinding.RequestPath(position + dir);
                }
                if (currentEnemy.position.x > transform.position.x)
                    run = -1;
                else
                    run = 1;
            }
        }
        
        /// <summary>
        /// The ai will try to determine if it needs to dig to get to the next waypoint
        /// and if it does it will decide to dig. 
        /// 
        /// Note that if the character detects and tries to kill an enemy after this call then that will 
        /// have precedence
        /// </summary>
        bool DigIfNeeded() {
            if (pathfinding.HasPath() && pathfinding.IsWaypointDiggable()) {
                Vector2 dist = pathfinding.GetCurrentWaypoint() - (position + Vector2.up);//We estimate crudely the weapon that will be aimed is at about 1m height
                //If we see ground when we look at the waypoint and we are close enough to dig then we should dig
                if (dist.magnitude < maxDigDistance) {
                    aimDirection = dist;
                    doFire = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Follows a path by running and jumping
        /// 
        /// returns true if reached end of path
        /// </summary>
        bool FollowPath() {
            if (!pathfinding.IsOnPath()) {
                pathfinding.SetPathUnvalid();
                if (verboseDebug)
                    Debug.Log(name + ": " + state + ": I was no longer on my path so requested a new one");
                return false;
            }
            //Find next waypoint that is on the ground. If the last one is in the air then we should jump to it
            pathfinding.FindNextVisibleGroundedWaypoint(terrainLayers);
            
            //Direction to the next waypoint
            Vector3 dist = (pathfinding.GetCurrentWaypoint() - position);
            Vector3 dir = dist.normalized;
            
            //If verbose is on then we can draw some helper symbols to see what the character is currently trying to do
            if (verboseDebug) {
                DebugDrawPhysics.DebugDrawCircle(pathfinding.GetCurrentWaypoint(), 1, Color.yellow);
            }

            //If there's a hole in the ground and the waypoint is not below us then jump
            doJump = DetectHolesInGround() && dir.y >= 0;
            
            //---Run---
            if (dir.x > 0) {
                run = 1;
                //Jump if we need to move up, double jumps will occur when we reach the max of the parabola and thus the upwards velocity is zero
                if (dist.y > 1 && //The next waypoint is at least one meter up
                    Vector3.Angle(Vector3.right, dir) > angleToJump && 
                    velocity.y <= 0) //A velocity zero we reached the top of the jump parabola
                    doJump = true;
            } else {
                run = -1;
                //Jump if we need to move up
                if (dist.y > 1 && 
                    Vector3.Angle(Vector3.left, dir) > angleToJump && 
                    velocity.y <= 0)
                    doJump = true;
            }

            //If we have a low obstacle then we should try jumping
            //Note that we will jump regardless of whether we detect an upper obstacle or not
            //We will only jump when we do not have upwards velocity so that we do not waste any double jumps
            if (isDetectingLowObstacle && velocity.y <= 0)
                doJump = true;
            //Check if we are close enough to the next waypoint
            //If we are, proceed to follow the next waypoint
            return pathfinding.SelectNextWaypointIfCloseEnough();
        }

        float GetDistanceToTarget() {
            if (currentTarget != null) {
                return (currentTarget.position - transform.position).magnitude;
            } else {
                return float.PositiveInfinity;
            }
        }

        float GetDistanceToEnemy() {
            if (currentEnemy != null) {
                return (currentEnemy.transform.position - transform.position).magnitude;
            } else {
                return float.PositiveInfinity;
            }
        }

        Vector2 GetDirectionToEnemy() {
            return (currentEnemy.transform.position - transform.position).normalized;
        }

        Vector2 GetDirectionToTarget() {
            return (currentTarget.position - transform.position).normalized;
        }
        
        bool IsPathToEnemyStillValid() {
            float targetMovedDistance = Vector3.Distance(currentEnemy.position, pathfinding.targetPosWhenRequesting);
            return targetMovedDistance < 5f;
        }

        bool IsPathToTargetStillValid() {
            float targetMovedDistance = Vector3.Distance(currentTarget.position, pathfinding.targetPosWhenRequesting);
            return targetMovedDistance < 5f;
        }

        bool ShootIfEnemyVisible() {
            
            
            if (currentEnemy != null && 
                CheckLineOfSight(currentEnemy, maxViewDistance)) {
                doFire = true;
                aimDirection = (currentEnemy.transform.position - transform.position).normalized;
                return true;
            }
            aimDirection = run > 0 ? Vector3.right : Vector3.left;
            return false;
        }

        /// <summary>
        /// Check if when the ai looks at a wayypoint she sees ground. This is important when determining if she sould
        /// start digging.
        /// </summary>
        /// <returns></returns>
        bool CanSeeGroundAtCurrentWaypoint() {
            Vector3 dist = pathfinding.GetCurrentWaypoint() - position;
            //by checking distance -1 we make sure we can see up to one meter hither of the waypoint That'll be good enough
            return CheckLineOfSight(pathfinding.GetCurrentWaypoint(), null, dist.magnitude - 1);
        }
        
        /// <summary>
        /// Checks that line of sight is entirely clear towards a world position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool CheckLineOfSight(Vector3 position) {
            return CheckLineOfSight(position, null);
        }

        /// <summary>
        /// Checks if line of sight is clear to a target. Returns true only if ai can see the target withiin maxdist
        /// </summary>
        /// <param name="target"></param>
        /// <param name="maxDist"></param>
        /// <returns></returns>
        public bool CheckLineOfSight(Transform target, float maxDist = float.PositiveInfinity) {
            if (target == null) return false;
            return CheckLineOfSight(target.position, target, maxDist);
        }

        private bool CheckLineOfSight(Vector3 position, Transform target, float maxDist = float.PositiveInfinity) {
            if (is2D) {
                Ray2D ray = new Ray2D(eyes.position, position - eyes.position);
                int oldLayer = character.gameObject.layer;

                //Temporarily ignore own body when raycasting
                SetBodypartLayers(Physics2D.IgnoreRaycastLayer);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxDist);
                SetBodypartLayers(oldLayer);

                //Looking for target
                if(target != null) {
                    //Saw target
                    //Note that the collider transform is not the same as the taarget, but the rigidbody transform is
                    if (hit.rigidbody != null && hit.rigidbody.transform == target) {
                        return true;
                    } else {
                        return false;
                    }
                    
                }
                //if we are not looking for a target then we need to see nothing in order to have free line of sight
                //If we saw nothing then there is free line of sight to position
                return hit.collider == null;
            } else {
                Ray ray = new Ray(eyes.position, position - eyes.position);
                int oldLayer = character.gameObject.layer;

                //Temporarily ignore own body when raycasting
                SetBodypartLayers(Physics.IgnoreRaycastLayer);
                RaycastHit hit;
                Physics.Raycast(ray.origin, ray.direction, out hit, maxDist);
                SetBodypartLayers(oldLayer);
                
                //Looking for target
                if (target != null) {
                    //Saw target
                    //Note that the collider transform is not the same as the taarget, but the rigidbody transform is
                    if (hit.rigidbody != null && hit.rigidbody.transform == target) {
                        return true;
                    } else {
                        return false;
                    }

                }
                //if we are not looking for a target then we need to see nothing in order to have free line of sight
                //If we saw nothing then there is free line of sight to position
                return hit.collider == null;
            }
        }

        Vector3 GetLineOfSightEndpoint(Vector3 position, Transform target, float maxDist = float.PositiveInfinity) {
            if (is2D) {
                Ray2D ray = new Ray2D(eyes.position, position - eyes.position);
                int oldLayer = character.gameObject.layer;

                //Temporarily ignore own body when raycasting
                SetBodypartLayers(Physics2D.IgnoreRaycastLayer);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxDist);
                SetBodypartLayers(oldLayer);
                return hit.point;
               
            } else {
                Ray ray = new Ray(eyes.position, position - eyes.position);
                int oldLayer = character.gameObject.layer;

                //Temporarily ignore own body when raycasting
                SetBodypartLayers(Physics.IgnoreRaycastLayer);
                RaycastHit hit;
                Physics.Raycast(ray.origin, ray.direction, out hit, maxDist);
                SetBodypartLayers(oldLayer);

                return hit.point;
            }
        }

        private void SetBodypartLayers(int newLayer) {
            //foreach (var bodypart in character.bodyParts) {
            //    bodypart.gameObject.layer = newLayer;
            //}
        }
        
        /// <summary>
        /// Detects if there is a hole in the ground in front of character
        /// 
        /// returns true if there is a hole
        /// </summary>
        bool DetectHolesInGround() {
            if (!character.isGrounded) return false;
            if (is2D) {
                Ray2D ray = new Ray2D(position, Vector2.down);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 1, terrainLayers);
                return hit.collider == null;
            } else {
                Ray ray = new Ray(position + 0.1f * Vector2.up, Vector2.down);
                return !Physics.Raycast(ray.origin, ray.direction, 1, terrainLayers);
            }
        }

        /// <summary>
        /// Use this to get some info about the currrent state of the ai. This can be useful when e.g.
        /// debugging errors
        /// </summary>
        /// <returns></returns>
        public string GetStateDescription() {
            return
                "Name: " + name + "\n" +
                "State: " + state + "\n" +
                "Has path: " + (pathfinding.HasPath() ? "Yes" : "No") + "\n" +
                "Current enemy: " + currentEnemy.name;

        }
    }
}