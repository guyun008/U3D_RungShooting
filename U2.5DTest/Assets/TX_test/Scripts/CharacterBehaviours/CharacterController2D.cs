using UnityEngine;
using System.Collections;


namespace TencentProgramerTest {

    public class CharacterController2D : MonoBehaviour {
        [Header("Hover over variable names to see tooltips!"), Tooltip("Increase this to make the character faster and stronger")]
        public float moveForce;
        [Tooltip("Increase this to make the character jump higher")]
        public float jumpVelocity;
        [Tooltip("This force is added to the character body when walking up an incline. Otherwise it might be very hard to wal up a hill or stairs")]
        public float upforceAtIncline;
        [Tooltip("When the character is crouching she should move slower. Thus we reduce the force a bit by multiplying with this value."), Range(0f,1f)]
        public float crouchingMoveForceMultiplier = 0.5f;

        [HideInInspector]
        public float currentDirection = 0f;

        public float bulletOffset = 0.6f;
        #region Physics properties
        private bool is2D { get { return rb2D; } }
        internal Vector2 velocity {
            get {
                if (is2D) return rb2D.velocity;
                else return rb3D.velocity;
            }
        }
        private float mass {
            get {
                if (is2D) return rb2D.mass;
                else return rb3D.mass;
            }
        }
        
        private Rigidbody2D rb2D;
        private Rigidbody rb3D;
        #endregion Physics properties

        public bool isGrounded;
        public bool isFacingRight;
        public bool isCrouching;
        public LayerMask groundableMaterials;

        public new AudioSource audio;
        public AudioClip jumpSound;
        public AudioClip landSound;

        public int maxDoubleJumps = 1;
        [SerializeField]
        private int currentDoubleJumps = 0;


        private bool hasBeenSetup = false;
        public float speed = 0.5f;

        // Use this for initialization
        void Start() {
            if (!hasBeenSetup)
                Setup();
        }

        public void Setup() {
            //bodyParts = feet.transform.parent.GetComponentsInChildren<BodyPartCollider>();
            //rb2D = GetComponent<Rigidbody2D>();
            rb3D = GetComponent<Rigidbody>();

            audio = GetComponent<AudioSource>();
            hasBeenSetup = true;
        }

        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
            if (is2D)
                rb2D.AddForce(force, mode);
            else
                rb3D.AddForce(force, (ForceMode)mode);
        }

        public void Run(float direction) {
            if (direction == 0) return;

            if (Mathf.Abs(direction) > 1)
                direction /= Mathf.Abs(direction);
            //AddForce(new Vector2(direction * moveForce, 0));
            Vector3 old_pos = transform.position;
            transform.position = new Vector3(old_pos.x + direction * speed, old_pos.y , old_pos.z);
            currentDirection = direction;
        }


        public void Jump() {
            if (isGrounded || currentDoubleJumps <= maxDoubleJumps) {
                //This represents a velocity change up to the velocity of jumpVelocity
                AddForce(Vector2.up * (jumpVelocity - velocity.y) * mass, ForceMode2D.Impulse);
                audio.PlayOneShot(jumpSound);

                currentDoubleJumps++;
            }
        }

        public void Fire() {
            Vector3 direction = transform.right;
            float currentBulletOffset = bulletOffset;
            if (currentDirection != 0)
            {
                currentBulletOffset = currentDirection * bulletOffset;
                direction = currentDirection * direction;
            } 
            BulletManager.instance.Fire(new Vector3(transform.position.x + currentBulletOffset, transform.position.y, transform.position.z), direction);
        }


        public void AutoFire() {
        }

    }
}