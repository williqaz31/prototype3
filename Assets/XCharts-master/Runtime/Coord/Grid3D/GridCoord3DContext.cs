using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public class GridCoord3DContext : MainComponentContext
    {
        public List<ChartLabel> endLabelList = new();
        public bool isPointerEnter;

        public Rect maxRect = new(0, 0, 0, 0);

        //public Vector3 position = Vector3.zero;
        public Vector3 pointA = Vector3.zero;
        public Vector3 pointB = Vector3.zero;
        public Vector3 pointC = Vector3.zero;
        public Vector3 pointD = Vector3.zero;
        public Vector3 pointE = Vector3.zero;
        public Vector3 pointF = Vector3.zero;
        public Vector3 pointG = Vector3.zero;
        public Vector3 pointH = Vector3.zero;
        public float x;
        public float y;
    }
}