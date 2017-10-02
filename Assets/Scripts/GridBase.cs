using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaster
{
    public class GridBase : MonoBehaviour
    {
        //Setting up the grid
        public int maxX = 10;
        public int maxY = 3;
        public int maxZ = 10;

        //Offset relates to the world positions only.
        public float offsetX = 1;
        public float offsetY = 1;
        public float offsetZ = 1;

        public Node[,,] grid; //Our grid

        public GameObject gridFloorPrefab;

        public Vector3 startNodePosition;
        public Vector3 endNodePosition;

        public int agents;

        void Start()
        {
            //Typical way to create a grid.
            grid = new Node[maxX, maxY, maxZ];

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        //Apply the offsets and create the world object for each node.
                        float posX = x * offsetX;
                        float posY = y * offsetY;
                        float posZ = z * offsetZ;

                        //Create our tile object.
                        GameObject go = (GameObject)Instantiate(gridFloorPrefab, new Vector3(posX, posY, posZ), Quaternion.identity);

                        //Rename it...
                        go.transform.name = string.Format("({0}, {1}, {2})", x.ToString(), y.ToString(), z.ToString());
                        //And parent it under this transform to be more organized.
                        go.transform.SetParent(this.transform);

                        //Create a new node and set it's values.
                        Node node = new Node();
                        node.x = x;
                        node.y = y;
                        node.z = z;
                        node.worldObject = go;

                        RaycastHit[] hits = Physics.BoxCastAll(new Vector3(posX, posY, posZ), new Vector3(1, 0, 1), Vector3.up);

                        for (int i = 0; i < hits.Length; i++)
                        {
                            if (!hits[i].transform.GetComponentInChildren<Collider>().isTrigger)
                            {
                                node.isWalkable = false;
                            }
                        }

                        if(!node.isWalkable)
                        {
                            node.worldObject.GetComponentInChildren<Renderer>().material.color = new Color(1.0f, 0f, 0f, 1.0f);
                        }

                        //Then place it in the grid.
                        grid[x, y, z] = node;
                    }
                }
            }
        }

        //Quick way to visualize the path
        public bool start;
        void Update()
        {
            if (start)
            {
                start = false;

                //Create a new pathfinder class.
                //Pathfinding.Pathfinder path = new Pathfinding.Pathfinder();

                //To test the avoidance, we're going to make a node unwalkable.
                grid[1, 0, 1].isWalkable = false;

                //Pass the target nodes
                Node startNode = GetNodeFromVector3(startNodePosition);
                Node end = GetNodeFromVector3(endNodePosition);

                //path.startPosition = startNode;
                //path.endPosition = end;

                //Find the path...
                //List<Node> p = path.FindPath();

                //and disable the world object for each node we are passing over.
                startNode.worldObject.SetActive(false);

                for (int i = 0; i < agents; i++)
                {
                    Pathfinding.PathfindMaster.GetInstance().RequestPathfind(startNode, end, ShowPath);
                }
                
            }
        }

        public void ShowPath(List<Node> path)
        {
            foreach (Node n in path)
            {
                n.worldObject.SetActive(false);
            }

            Debug.Log("Agent Complete.");
        }

        public Node GetNode(int x, int y, int z)
        {
            //Used to get a node from a grid. If it's greater than all the max values we have, then it will return null.

            Node retVal = null;

            if(x < maxX && x >=0 &&
               y >= 0 && y < maxY &&
               z >= 0 && z < maxZ)
            {
                retVal = grid[x, y, z];
            }

            return retVal;
        }

        public Node GetNodeFromVector3(Vector3 pos)
        {
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);
            int z = Mathf.RoundToInt(pos.z);

            Node retVal = GetNode(x, y, z);
            return retVal;
        }

        #region Singleton Shenanigans
        public static GridBase instance;
        public static GridBase GetInstance()
        {
            return instance;
        }

        void Awake()
        {
            instance = this;
        }
        #endregion
    }
}
