using System.Collections;
using UnityEngine;

namespace LevelControl
{
    public class LevelInitializer : MonoBehaviour
    {

        public GameObject gridBasePrefab;

        public int unitCount = 1;
        public GameObject unitPrefabs;

        private WaitForEndOfFrame waitEF;

        public GridStats gridStats;

        [System.Serializable]
        public class GridStats
        {
            int maxX = 10;
            int maxY = 3;
            int maxZ = 10;

            public float offsetX = 1;
            public float offsetY = 1;
            public float offsetZ = 1;
        }
    }
}
