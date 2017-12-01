using UnityEngine;
using System.Collections;

public interface IDamageable {
    void ApplyDamage(float damage, Vector3 position, Vector3 incomingDirection);
}
