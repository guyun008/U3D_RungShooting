using UnityEngine;
using System.Collections.Generic;

using Pathfinding;
using Pathfinding.Serialization.JsonFx;
using Pathfinding.Serialization;
using System;

[JsonOptIn]
public class GridJumpGraph : GridGraph {
    [JsonMember]
    public float maxJumpHeight;
    [JsonMember]
    public float maxFlyHeight = 20;
    [JsonMember]
    public float maxDropHeight;

    [JsonMember]
    public LayerMask diggableLayers;
    [JsonMember]
    public uint walkableTag = 1;
    [JsonMember]
    public uint jumpableTag = 2;
    [JsonMember]
    public uint flyableTag = 3;
    [JsonMember]
    public uint diggableTag = 31;
    
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public override void UpdateNodePositionCollision(GridNode node, int x, int z, bool resetPenalty = true) {



        base.UpdateNodePositionCollision(node, x, z, resetPenalty);
        // Set the node's initial position with a y-offset of zero
        //node.position = GraphPointToWorld(x, z, 0);
        ///node.Walkable = true;
        // If the walkable flag has already been set to false, there is no point in checking for it again
        // Check for walkable ground under node
        //Determine if the node is possible to dig. Will return false if blocked by object OR if there is only thin air
        //TODO: Maybe we could connect this to voxel world and assign different costs to different materials as well as make some materials undiggable
        bool isDiggable = false;
        RaycastHit dighit;
        Ray digray = new Ray(new Vector3(node.position.x / Int3.FloatPrecision, node.position.y / Int3.FloatPrecision, node.position.z / Int3.FloatPrecision - 3), Vector3.forward);
        
        isDiggable |= Physics.Raycast(digray, out dighit, 3, diggableLayers);
        if (!node.Walkable && isDiggable) {
            node.Walkable = true;
        }

        if (node.Walkable) {
            RaycastHit hit;
            float rayCastLength = maxFlyHeight;
            float nearest = float.PositiveInfinity;
            bool didHit = false;

            Ray ray = new Ray(new Vector3(node.position.x / Int3.FloatPrecision, node.position.y / Int3.FloatPrecision, node.position.z / Int3.FloatPrecision), Vector3.down);
            didHit |= Physics.Raycast(ray, out hit, rayCastLength, collision.mask);
            nearest = Mathf.Min(nearest, hit.distance);

            ray = new Ray(new Vector3(node.position.x / Int3.FloatPrecision, node.position.y / Int3.FloatPrecision, node.position.z / Int3.FloatPrecision), new Vector3(1f, -1f, 0).normalized);
            didHit |= Physics.Raycast(ray, out hit, rayCastLength, collision.mask) && hit.normal.y > 0.1f;
            //nearest = Mathf.Min(nearest, hit.distance);

            ray = new Ray(new Vector3(node.position.x / Int3.FloatPrecision, node.position.y / Int3.FloatPrecision, node.position.z / Int3.FloatPrecision), new Vector3(-1f, -1f, 0).normalized);
            didHit |= Physics.Raycast(ray, out hit, rayCastLength, collision.mask) && hit.normal.y > 0.1f;
            //nearest = Mathf.Min(nearest, hit.distance);
            
            //We set all diggable as walkable for now. We may need to reconfig this as AI may get the idea she can dig straight up
            if (!isDiggable)
                node.Walkable = didHit;
            // Store walkability before erosion is applied
            // Used for graph updating
            node.WalkableErosion = node.Walkable;

            //Set proper tag
            if (isDiggable) {
                node.Tag = diggableTag;
            } else if (nearest > maxJumpHeight) {
                node.Tag = flyableTag;
            } else if (nearest <= maxJumpHeight && nearest > 0.5f) {
                node.Tag = jumpableTag;
            } else {
                node.Tag = walkableTag;
            }

            if (resetPenalty) {
                node.Penalty = (uint)(nearest * 1000);
                //TODO: Calculate penalty maybe based on height from ground only and use the same graph for everything?
                //Or maybe set tags and let the seeker know if he can jetpack or jump or whatever
        }
        }
    }

    public override bool IsValidConnection(GridNode n1, GridNode n2) {
        /*Ray ray = new Ray(new Vector3(n2.position.x / Int3.FloatPrecision, n2.position.y / Int3.FloatPrecision, n2.position.z / Int3.FloatPrecision), Vector3.down);
        if (!Physics.Raycast(ray, maxJumpHeight, collision.mask))
            return false;
            */
        return base.IsValidConnection(n1, n2);
    }
}
