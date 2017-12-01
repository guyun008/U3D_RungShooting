using UnityEngine;
using System.Collections;

namespace TencentProgramerTest {
    [CreateAssetMenuAttribute()]
    public class FirearmData : PickupData {
        public enum FireModeType {
            SINGLE_SHOT,
            BURST,
            FULL_AUTO,

        }

        public float muzzleVelocity = 10;
        [Header("Spread in degrees")]
        public float spread = 10;
        public float muzzleVelocityVariation = 1f;
        public int bulletsFiredAtATime = 1;
        public int maxAmmo = 6;
        public float reloadTime = 1f;
        public float fireCooldown = 0.1f;
        public float damage = 1f;
        public FireModeType fireMode = FireModeType.SINGLE_SHOT;

    }
}