using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public sealed class AnimationInfoContext
    {
        public float currDuration;
        public Vector3 currPoint;
        public int currPointIndex;
        public float currProgress;
        public Dictionary<int, float> dataCurrProgress = new();
        public Dictionary<int, float> dataDestProgress = new();
        public Vector3 destPoint;
        public int destPointIndex;
        public float destProgress;
        public bool end;
        public bool init;
        public bool pause;
        public float sizeProgress;
        public bool start;
        public float startTime;
        public float totalProgress;
    }
}