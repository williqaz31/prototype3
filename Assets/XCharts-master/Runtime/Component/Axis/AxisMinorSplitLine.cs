using System;
using UnityEngine;

namespace XCharts.Runtime
{
    /// <summary>
    ///     Minor split line of axis in grid area.
    ///     ||坐标轴在 grid 区域中的次分隔线。次分割线会对齐次刻度线 minorTick。
    /// </summary>
    [Serializable]
    [Since("v3.2.0")]
    public class AxisMinorSplitLine : BaseLine
    {
        [SerializeField] private float m_Distance;
        [SerializeField] private bool m_AutoColor;

        /// <summary>
        ///     The distance between the split line and axis line.
        ///     ||刻度线与轴线的距离。
        /// </summary>
        public float distance
        {
            get => m_Distance;
            set => m_Distance = value;
        }

        /// <summary>
        ///     auto color.
        ///     ||自动设置颜色。
        /// </summary>
        public bool autoColor
        {
            get => m_AutoColor;
            set => m_AutoColor = value;
        }

        public override bool vertsDirty => m_VertsDirty || m_LineStyle.anyDirty;

        public static AxisMinorSplitLine defaultMinorSplitLine =>
            new()
            {
                m_Show = false
            };

        public override void ClearVerticesDirty()
        {
            base.ClearVerticesDirty();
            m_LineStyle.ClearVerticesDirty();
        }

        public AxisMinorSplitLine Clone()
        {
            var axisSplitLine = new AxisMinorSplitLine();
            axisSplitLine.show = show;
            axisSplitLine.distance = distance;
            axisSplitLine.autoColor = autoColor;
            axisSplitLine.lineStyle = lineStyle.Clone();
            return axisSplitLine;
        }

        public void Copy(AxisMinorSplitLine splitLine)
        {
            base.Copy(splitLine);
            distance = splitLine.distance;
            autoColor = splitLine.autoColor;
        }
    }
}