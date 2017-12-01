using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

namespace TencentProgramerTest {
    [RequireComponent(typeof(CharacterController2D))]
    public class PlayerControls : MonoBehaviour {
        
        public enum ControllerType {
            MOUSE_AND_KEYBOARD,
            XBOX_360_CONTROLLER,

        }

        [Header("Set you preferred controls here")]
        public KeyCode jumpKey = KeyCode.W;
        public KeyCode runLeftKey = KeyCode.A;
        public KeyCode runRightKey = KeyCode.D;
        public KeyCode crouchKey = KeyCode.S;
        public KeyCode reloadKey = KeyCode.R;
        public KeyCode switchWeaponRightKey = KeyCode.E;
        public KeyCode switchWeaponLeftKey = KeyCode.Q;
        public KeyCode fireKey = KeyCode.Space;
        [Header("Only mouse and keyboardcontrols are implemented at this point")]
        public ControllerType controller;
        [Header("Max amount camera will move along aim vector")]
        public float maxAimAheadDistance = 7f;
        [Header("Min amount of aim to start moving camera along aim vector")]
        public float startAimAheadDistance = 2f;

        CharacterController2D character;

        #region Control values

        float run;
        bool doJump;
        bool doReload;
        bool doCrouch;
        bool doFire;
        bool doAutoFire;
        bool doCycleGunsRight;
        bool doCycleGunsLeft;
        Vector2 aimPosition;
        float zoomAmount;

        #endregion

        void Awake() {
            character = GetComponent<CharacterController2D>();
        }

        void Update() {
            switch (controller) {
                case ControllerType.MOUSE_AND_KEYBOARD:
                    HandleKeyboardAndMouseControls();
                    break;
                case ControllerType.XBOX_360_CONTROLLER:
                    break;
                default:
                    break;
            }
            
        }

        void FixedUpdate() {

            if (doJump) {
                character.Jump();
                doJump = false;
            }

            character.Run(run);

            if (doFire) {
                character.Fire();
                doFire = false;
            }else if (doAutoFire)
                //Weapon will only respond if it has autofire
                character.AutoFire();
        }

        void HandleKeyboardAndMouseControls() {
            aimPosition = MouseAim();

            if (Input.GetKeyDown(jumpKey))
                doJump = true;
            if (Input.GetKey(runRightKey))
                run = 1;
            else if (Input.GetKey(runLeftKey))
                run = -1;
            else
                run = 0;
            
            if (Input.GetKeyDown(reloadKey))
                doReload = true;

            if(Input.mouseScrollDelta.y != 0) {
                zoomAmount = -(Input.mouseScrollDelta.y);
            }

            doCrouch =(Input.GetKey(crouchKey));

            if (Input.GetKeyDown(fireKey) || Input.GetMouseButtonDown(0))
                doFire = true;
            else if (Input.GetKey(fireKey) || Input.GetMouseButton(0))
                doAutoFire = true;
            else
                doAutoFire = false;


            if (Input.GetKeyDown(switchWeaponRightKey))
                doCycleGunsRight = true;
            if (Input.GetKeyDown(switchWeaponLeftKey))
                doCycleGunsLeft = true;
        }

        Vector2 MouseAim() {
            float z = 0;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = z - Camera.main.transform.position.z;
            Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);
            pos.z = z;
            return pos;
        }

        public Vector3 GetVelocity() {
            if (character)
                return character.velocity;
            else
                return Vector3.zero;
        }

        public Vector3 GetCameraAimPosition() {
            Vector3 dist = (Vector3)aimPosition - transform.position;
            dist.z = 0;
            float length = dist.magnitude;
            dist.Normalize();
            if (length < startAimAheadDistance)
                return transform.position;
            else
                length -= startAimAheadDistance;

            if (length > maxAimAheadDistance)
                length = maxAimAheadDistance;

            dist = length * dist;
            return transform.position + dist;
        }

        public float GetZoom() {
            float zoom = zoomAmount;
            zoomAmount = 0;
            return zoom;
        }
    }
}
