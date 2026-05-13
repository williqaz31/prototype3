using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public class TooltipData
    {
        public List<SerieParams> param = new();
        public string title;
    }

    public class TooltipContext
    {
        public float angle;
        public TooltipData data = new();
        public float height;
        public Vector2 pointer;
        public Tooltip.Trigger trigger;
        public Tooltip.Type type;
        public float width;
        public int xAxisClickIndex = -1;
    }
}