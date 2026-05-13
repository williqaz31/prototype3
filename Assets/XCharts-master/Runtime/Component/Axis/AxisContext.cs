using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public class AxisContext : MainComponentContext
    {
        /// <summary>
        ///     添加过的历史数据总数
        /// </summary>
        public int addedDataCount;

        public TextAnchor aligment;
        public double axisTooltipValue;
        public float bottom;
        public int dataZoomStartIndex;
        public Vector3 dire;
        public Vector3 end;

        internal List<string> filterData;
        private int filterEnd;
        private int filterMinShow;

        private int filterStart;
        public float height;
        internal bool isNeedUpdateFilterData;
        internal bool lastCheckInverse;
        public float left;
        public float length;

        private readonly List<string> m_EmptyFliter = new();

        /// <summary>
        ///     the current maximum value.
        ///     ||当前最大值。
        /// </summary>
        public double maxValue;

        public double minMaxRange;

        /// <summary>
        ///     the current minimun value.
        ///     ||当前最小值。
        /// </summary>
        public double minValue;

        /// <summary>
        ///     the offset of zero position.
        ///     ||坐标轴原点在坐标轴的偏移。
        /// </summary>
        public float offset;

        public Orient orient;
        public Vector3 pointerLabelPosition;
        public double pointerValue;
        public Vector3 position;
        public float right;
        public float scaleWidth;
        public Vector3 start;
        public float startAngle;

        /// <summary>
        ///     the tick value of value axis.
        ///     ||数值轴时每个tick的数值。
        /// </summary>
        public double tickValue;

        public float top;
        public float width;

        /// <summary>
        ///     坐标轴的起点X
        /// </summary>
        public float x;

        /// <summary>
        ///     坐标轴的起点Y
        /// </summary>
        public float y;

        /// <summary>
        ///     坐标轴原点X
        /// </summary>
        public float zeroX;

        /// <summary>
        ///     坐标轴原点Y
        /// </summary>
        public float zeroY;

        public double lastMinValue { get; internal set; }
        public double destMinValue { get; internal set; }
        public double lastMaxValue { get; internal set; }
        public double destMaxValue { get; internal set; }
        public bool needAnimation { get; internal set; }
        public List<string> runtimeData { get; } = new();

        public List<double> labelValueList { get; } = new();

        public List<ChartLabel> labelObjectList { get; } = new();

        public List<int> sortedDataIndices { get; } = new();

        internal void Clear()
        {
            addedDataCount = 0;
            runtimeData.Clear();
        }

        /// <summary>
        ///     更新dataZoom对应的类目数据列表
        /// </summary>
        /// <param name="dataZoom"></param>
        internal void UpdateFilterData(List<string> data, DataZoom dataZoom)
        {
            int start = 0, end = 0;
            var range = Mathf.RoundToInt(data.Count * (dataZoom.end - dataZoom.start) / 100);
            if (range <= 0)
                range = 1;

            if (dataZoom.context.invert)
            {
                end = Mathf.RoundToInt(data.Count * dataZoom.end / 100);
                start = end - range;
                if (start < 0) start = 0;
            }
            else
            {
                start = Mathf.RoundToInt(data.Count * dataZoom.start / 100);
                end = start + range;
                if (end > data.Count) end = data.Count;
            }

            var minZoomRatio = (int)(data.Count * dataZoom.minZoomRatio);
            if (start != filterStart ||
                end != filterEnd ||
                minZoomRatio != filterMinShow ||
                isNeedUpdateFilterData)
            {
                filterStart = start;
                filterEnd = end;
                filterMinShow = minZoomRatio;
                isNeedUpdateFilterData = false;

                if (data.Count > 0)
                {
                    if (range < minZoomRatio)
                    {
                        if (dataZoom.minZoomRatio > data.Count)
                            range = data.Count;
                        else
                            range = minZoomRatio;
                    }

                    if (range > data.Count - start)
                        start = data.Count - range;
                    if (start >= 0)
                    {
                        dataZoomStartIndex = start;
                        filterData = data.GetRange(start, range);
                    }
                    else
                    {
                        dataZoomStartIndex = 0;
                        filterData = data;
                    }
                }
                else
                {
                    dataZoomStartIndex = 0;
                    filterData = data;
                }
            }
            else if (end == 0)
            {
                dataZoomStartIndex = 0;
                filterData = m_EmptyFliter;
            }
        }
    }
}