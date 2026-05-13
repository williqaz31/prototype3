using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public class GridCoordContext : MainComponentContext
    {
        public Vector3 center;
        public List<ChartLabel> endLabelList = new();
        public float height;
        public bool isPointerEnter;
        public Vector3 position;
        public float width;
        public float x;
        public float y;
    }
}