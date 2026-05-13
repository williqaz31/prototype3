using System;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    public class ChildComponent
    {
        [NonSerialized] protected bool m_ComponentDirty;
        [NonSerialized] protected Painter m_Painter;

        [NonSerialized] protected bool m_VertsDirty;
        public virtual int index { get; set; }

        /// <summary>
        ///     图表重绘标记。
        /// </summary>
        public virtual bool vertsDirty => m_VertsDirty;

        /// <summary>
        ///     组件重新初始化标记。
        /// </summary>
        public virtual bool componentDirty => m_ComponentDirty;

        /// <summary>
        ///     需要重绘图表或重新初始化组件。
        /// </summary>
        public bool anyDirty => vertsDirty || componentDirty;

        public Painter painter
        {
            get => m_Painter;
            set => m_Painter = value;
        }

        public Action refreshComponent { get; set; }
        public GameObject gameObject { get; set; }

        public static void ClearVerticesDirty(ChildComponent component)
        {
            if (component != null)
                component.ClearVerticesDirty();
        }

        public static void ClearComponentDirty(ChildComponent component)
        {
            if (component != null)
                component.ClearComponentDirty();
        }

        public static bool IsVertsDirty(ChildComponent component)
        {
            return component == null ? false : component.vertsDirty;
        }

        public static bool IsComponentDirty(ChildComponent component)
        {
            return component == null ? false : component.componentDirty;
        }

        public virtual void SetVerticesDirty()
        {
            m_VertsDirty = true;
        }

        public virtual void ClearVerticesDirty()
        {
            m_VertsDirty = false;
        }

        public virtual void SetComponentDirty()
        {
            m_ComponentDirty = true;
        }

        public virtual void ClearComponentDirty()
        {
            m_ComponentDirty = false;
        }

        public virtual void ClearDirty()
        {
            ClearVerticesDirty();
            ClearComponentDirty();
        }

        public virtual void SetAllDirty()
        {
            SetVerticesDirty();
            SetComponentDirty();
        }
    }
}