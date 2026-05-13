using System;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    public class StageColor : ChildComponent
    {
        [SerializeField] private float m_Percent;
        [SerializeField] private Color32 m_Color;

        public StageColor(float percent, Color32 color)
        {
            m_Percent = percent;
            m_Color = color;
        }

        /// <summary>
        ///     结束位置百分比。
        /// </summary>
        public float percent
        {
            get => m_Percent;
            set => m_Percent = value;
        }

        /// <summary>
        ///     颜色。
        /// </summary>
        public Color32 color
        {
            get => m_Color;
            set => m_Color = value;
        }
    }
}