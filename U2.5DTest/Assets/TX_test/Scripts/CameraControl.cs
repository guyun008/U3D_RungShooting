using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    public GameObject Target = null;
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (Target != null)
        {
            Vector3 pos = Target.transform.position;
            Vector3 old_pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y+5, old_pos.z);
        }
    }
}
