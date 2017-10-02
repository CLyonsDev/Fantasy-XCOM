using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaster
{
    public class Node
    {
        //Node's position in the grid
        public int x;
        public int y;
        public int z;

        //Node's costs for pathfinding purposes
        public float hCost;
        public float gCost;

        public float fCost
        {
            get //The fCost is the gCost+hCost. We can use this to get it easily.
            {
                return gCost + hCost;
            }
        }

        public Node parentNode;
        public bool isWalkable = true;

        //Reference to the world object so we can have the world position of the node... among other things.
        public GameObject worldObject;

        public NodeType nodeType;
        public enum NodeType
        {
            ground,
            air
        };
    }
}
