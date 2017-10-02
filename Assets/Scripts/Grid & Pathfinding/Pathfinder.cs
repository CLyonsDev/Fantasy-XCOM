using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridMaster;

namespace Pathfinding
{
    public class Pathfinder
    {
        GridBase gridBase;
        public Node startPosition;
        public Node endPosition;

        public volatile bool jobDone = false;
        PathfindMaster.PathfindingJobComplete completeCallback;
        List<Node> foundPath;

        public Pathfinder(Node start, Node target, PathfindMaster.PathfindingJobComplete callback)
        {
            startPosition = start;
            endPosition = target;
            completeCallback = callback;
            gridBase = GridBase.GetInstance();
        }

        public void FindPath()
        {
            foundPath = FindPathActual(startPosition, endPosition);

            jobDone = true;
        }

        public void NotifyComplete()
        {
            if(completeCallback != null)
            {
                completeCallback(foundPath);
            }
        }

        private List<Node> FindPathActual(Node start, Node target)
        {
            //Typical A* algorythm...
            List<Node> foundPath = new List<Node>();

            //We need two lists, one for the nodes we need to check, and one for nodes we've already checked.
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(start);

            while(openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                for (int i = 0; i < openSet.Count; i++)
                {
                    //We check the costs for the current node. More is possible but not important now.
                    if(openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost &&
                        openSet[i].hCost < currentNode.hCost))
                    {
                        //We assign a new current node.
                        if (!currentNode.Equals(openSet[i]))
                        {
                            currentNode = openSet[i];
                        }
                    }
                }

                //We remove the current node from the open set and add to the closed set.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                //If our current node is the target node...
                if (currentNode.Equals(target))
                {
                    //We've found our destination, it's time to trace our path.
                    foundPath = RetracePath(start, currentNode);
                    break;
                }

                //If we haven't reached the target, we need to start looking at the neighbors.
                foreach (Node neighbor in GetNeighbors(currentNode, true))
                {
                    if (!closedSet.Contains(neighbor))
                    {
                        //We create a new movement cost for our neighbors
                        float newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                        //And if it is lower than the neighbor's cost...
                        if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                        {
                            //We allocate new costs
                            neighbor.gCost = newMovementCostToNeighbor;
                            neighbor.hCost = GetDistance(neighbor, target);

                            //Assign the parent node
                            neighbor.parentNode = currentNode;

                            //And add the neighbor node to the open set
                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);
                            }
                        }
                    }
                }
            }

            //We return the path at the end.
            return foundPath;
        }

        private List<Node> RetracePath(Node startNode, Node endNode)
        {
            //Go from endNode to startNode...
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while(currentNode != startNode)
            {
                path.Add(currentNode);
                //by using the parentNodes we assigned earlier!
                currentNode = currentNode.parentNode;
            }

            //And then reverse the list.
            path.Reverse();

            return path;
        }

        private List<Node> GetNeighbors(Node node, bool getVerticalNeighbors = false)
        {
            //This is where we start getting our neighbors
            List<Node> retList = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int yIndex = -1; yIndex <= 1; yIndex++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int y = yIndex;

                        //If we don't want 3d A*, then don't search the y!!!
                        if(!getVerticalNeighbors)
                        {
                            y = 0;
                        }

                        if(x == 0 && y == 0 && z == 0)
                        {
                            //0,0,0 is current node.
                        }
                        else
                        {
                            Node searchPos = new Node();

                            //The nodes we want are what's forwards/back, left/righgt, up/down from us.
                            searchPos.x = node.x + x;
                            searchPos.y = node.y + y;
                            searchPos.z = node.z + z;

                            Node newNode = GetNeighborNode(searchPos, true, node);

                            if(newNode != null)
                            {
                                retList.Add(newNode);
                            }
                        }
                    }
                }
            }

            return retList;
        }

        private Node GetNeighborNode(Node adjPos, bool searchTopDown, Node currentNodePos)
        {
            //This is the meat. We can add all of the checks we need to tweak the alg.
            //First, the usual A* stuff.

            Node retVal = null;

            //Let's take the node from the adjacent positions we passed
            Node node = GetNode(adjPos.x, adjPos.y, adjPos.z);

            //if it's not null and we can walk on it...
            if(node != null && node.isWalkable)
            {
                //We can use this node.
                retVal = node;
            }//Otherwise...
            else if(searchTopDown) //and we want 3d A*
            {
                //Then look at what the node has under it
                adjPos.y -= 1;
                Node bottomBlock = gridBase.GetNode(adjPos.x, adjPos.y, adjPos.z);
                
                //If there is a bottom block and we can walk on it
                if(bottomBlock != null && bottomBlock.isWalkable)
                {
                    //We can return that.
                    retVal = bottomBlock;
                }
                else
                {
                    //Otherwise, let's look up instead.
                    adjPos.y += 2;
                    Node topBlock = gridBase.GetNode(adjPos.x, adjPos.y, adjPos.z);

                    //Same as above.
                    if(topBlock != null && topBlock.isWalkable)
                    {
                        retVal = topBlock;
                    }
                }
            }

            //If the node is diagonal to the current node then we check it's neighbors.
            //To move diagonally, we need all 4 nodes walkable.
            int originalX = adjPos.x - currentNodePos.x;
            int originalZ = adjPos.z - currentNodePos.z;

            if(Mathf.Abs(originalX) == 1 && Mathf.Abs(originalZ) == 1)
            {
                //The first block is originalX, 0 and the second to check is 0, originalZ.
                //Both need to be walkable.
                Node neighbor1 = gridBase.GetNode(currentNodePos.x + originalX, currentNodePos.y, currentNodePos.z + originalZ);
                if(neighbor1 == null || !neighbor1.isWalkable)
                {
                    retVal = null;
                }

                Node neighbor2 = gridBase.GetNode(currentNodePos.x, currentNodePos.y, currentNodePos.z + originalZ);
                if (neighbor2 == null || !neighbor2.isWalkable)
                {
                    retVal = null;
                }   
            }

            //Here we can add additional checks.
            if (retVal != null)
            {
                //EX. Do not approach any nodes from the left
                /*if(node.x < currentNodePos.x)
                {
                    node = null;
                }*/
            }

            return retVal;
        }

        private Node GetNode(int x, int y, int z)
        {
            Node n = null;

            lock (gridBase)
            {
                n = gridBase.GetNode(x, y, z);
            }

            return n;
        }

        private int GetDistance(Node posA, Node posB)
        {
            //We find the distance between each node.

            int distX = Mathf.Abs(posA.x - posB.x);
            int distY = Mathf.Abs(posA.y - posB.y);
            int distZ = Mathf.Abs(posA.z - posB.z);

            if(distX > distZ)
            {
                return 14 * distZ + 10 * (distX - distZ) + 10 * distY;
            }

            return 14 * distX + 10 * (distZ - distX) + 10 * distY;
        }
    }
}
