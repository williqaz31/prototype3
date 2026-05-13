using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public class ParallelCoordContext : MainComponentContext
    {
        public float bottom;
        public float height;
        public float left;
        internal List<ParallelAxis> parallelAxes = new();
        public Vector3 position;
        public float right;
        public bool runtimeIsPointerEnter;
        public float top;
        public float width;
        public float x;
        public float y;
    }
}