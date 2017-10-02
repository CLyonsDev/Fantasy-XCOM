using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridMaster;
using System.Threading;

namespace Pathfinding
{
    public class PathfindMaster : MonoBehaviour
    {
        private static PathfindMaster instance;

        void Awake()
        {
            instance = this;
        }

        public static PathfindMaster GetInstance()
        {
            return instance;
        }

        public int maxJobs = 3;

        public delegate void PathfindingJobComplete(List<Node> path);

        private List<Pathfinder> currentJobs;
        private List<Pathfinder> todoJobs;

        void Start()
        {
            currentJobs = new List<Pathfinder>();
            todoJobs = new List<Pathfinder>();
        }

        void Update()
        {
            int i = 0;

            while(i < currentJobs.Count)
            {
                if (currentJobs[i].jobDone)
                {
                    currentJobs[i].NotifyComplete();
                    currentJobs.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if(todoJobs.Count > 0 && currentJobs.Count < maxJobs)
            {
                Pathfinder job = todoJobs[0];
                todoJobs.RemoveAt(0);
                currentJobs.Add(job);

                //Start a new thread.

                Thread jobThread = new Thread(job.FindPath);
                jobThread.Start();
            }
        }

        public void RequestPathfind(Node start, Node target, PathfindingJobComplete completeCallback)
        {
            Pathfinder newJob = new Pathfinder(start, target, completeCallback);
            todoJobs.Add(newJob);
        }
    }
}

