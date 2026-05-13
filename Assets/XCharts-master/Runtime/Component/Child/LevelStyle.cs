using System;
using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    public class Level : ChildComponent
    {
        [SerializeField] [Since("v3.10.0")] private int m_Depth;
        [SerializeField] private LabelStyle m_Label = new();
        [SerializeField] private LabelStyle m_UpperLabel = new();
        [SerializeField] [Since("v3.10.0")] private LineStyle m_LineStyle = new();
        [SerializeField] private ItemStyle m_ItemStyle = new();

        /// <summary>
        ///     the depth of level.
        ///     ||层级深度。
        /// </summary>
        public int depth
        {
            get => m_Depth;
            set => m_Depth = value;
        }

        /// <summary>
        ///     the label style of level.
        ///     ||文本标签样式。
        /// </summary>
        public LabelStyle label => m_Label;

        /// <summary>
        ///     the upper label style of level.
        ///     ||上方的文本标签样式。
        /// </summary>
        public LabelStyle upperLabel => m_UpperLabel;

        /// <summary>
        ///     the line style of level.
        ///     ||线条样式。
        /// </summary>
        public LineStyle lineStyle => m_LineStyle;

        /// <summary>
        ///     the item style of level.
        ///     ||数据项样式。
        /// </summary>
        public ItemStyle itemStyle => m_ItemStyle;
    }

    [Serializable]
    public class LevelStyle : ChildComponent
    {
        [SerializeField] private bool m_Show;
        [SerializeField] private List<Level> m_Levels = new() { new Level() };

        /// <summary>
        ///     是否启用LevelStyle
        /// </summary>
        public bool show
        {
            get => m_Show;
            set => m_Show = value;
        }

        /// <summary>
        ///     各层节点对应的配置。当enableLevels为true时生效，levels[0]对应的第一层的配置，levels[1]对应第二层，依次类推。当levels中没有对应层时用默认的设置。
        /// </summary>
        public List<Level> levels => m_Levels;
    }
}