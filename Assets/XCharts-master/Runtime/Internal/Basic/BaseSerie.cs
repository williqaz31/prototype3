using System;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    public abstract class BaseSerie
    {
        [NonSerialized] public SerieContext context = new();
        [NonSerialized] public InteractData interact = new();
        [NonSerialized] protected bool m_ComponentDirty;
        [NonSerialized] protected Painter m_Painter;

        [NonSerialized] protected bool m_VertsDirty;
        public virtual bool vertsDirty => m_VertsDirty;
        public virtual bool componentDirty => m_ComponentDirty;

        public virtual SerieColorBy defaultColorBy => SerieColorBy.Serie;
        public virtual bool titleJustForSerie => false;
        public virtual bool useSortData => false;
        public virtual bool multiDimensionLabel => false;
        public bool anyDirty => vertsDirty || componentDirty;

        public Painter painter
        {
            get => m_Painter;
            set => m_Painter = value;
        }

        public Action refreshComponent { get; set; }
        public GameObject gameObject { get; set; }

        public SerieHandler handler { get; set; }


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

        public virtual void ClearData()
        {
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

        public virtual void OnRemove()
        {
            if (handler != null)
                handler.RemoveComponent();
        }

        public virtual void OnDataUpdate()
        {
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            OnDataUpdate();
        }

        public void RefreshLabel()
        {
            if (handler != null)
                handler.RefreshLabelNextFrame();
        }
    }
}