using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XCharts.Runtime
{
    /// <summary>
    ///     UI组件基类。
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UIComponent : BaseGraph
    {
        [SerializeField] private bool m_DebugModel;
        [SerializeField] protected UIComponentTheme m_Theme = new();
        [SerializeField] protected Background m_Background = new() { show = true };
        private ThemeType m_CheckTheme = 0;

        protected bool m_DataDirty;

        public override HideFlags chartHideFlags => m_DebugModel ? HideFlags.None : HideFlags.HideInHierarchy;

        public UIComponentTheme theme
        {
            get => m_Theme;
            set => m_Theme = value;
        }

        /// <summary>
        ///     背景样式。
        /// </summary>
        public Background background
        {
            get => m_Background;
            set
            {
                m_Background = value;
                color = Color.white;
            }
        }

        protected override void Awake()
        {
            CheckTheme(true);
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
            if (m_DataDirty)
            {
                m_DataDirty = false;
                DataDirty();
            }
        }

        /// <summary>
        ///     Update chart theme.
        ///     ||切换内置主题。
        /// </summary>
        /// <param name="theme">theme</param>
        public bool UpdateTheme(ThemeType theme)
        {
            if (theme == ThemeType.Custom)
            {
                Debug.LogError("UpdateTheme: not support switch to Custom theme.");
                return false;
            }

            if (m_Theme.sharedTheme == null)
                m_Theme.sharedTheme = XCThemeMgr.GetTheme(ThemeType.Default);
            m_Theme.sharedTheme.CopyTheme(theme);
            m_Theme.SetAllDirty();
            return true;
        }

        [Since("v3.9.0")]
        public void SetDataDirty()
        {
            m_DataDirty = true;
            m_RefreshChart = true;
        }

        public override void SetAllDirty()
        {
            base.SetAllDirty();
            SetDataDirty();
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            m_RefreshChart = true;
        }

        protected override void InitComponent()
        {
            base.InitComponent();
            if (m_Theme.sharedTheme == null)
                m_Theme.sharedTheme = XCThemeMgr.GetTheme(ThemeType.Default);
            UIHelper.InitBackground(this);
        }

        protected override void CheckComponent()
        {
            base.CheckComponent();
            if (m_Theme.anyDirty)
            {
                if (m_Theme.componentDirty) SetAllComponentDirty();
                if (m_Theme.vertsDirty) RefreshGraph();
                m_Theme.ClearDirty();
            }
        }

        protected override void SetAllComponentDirty()
        {
            base.SetAllComponentDirty();
            InitComponent();
        }

        protected override void OnDrawPainterBase(VertexHelper vh, Painter painter)
        {
            vh.Clear();
            UIHelper.DrawBackground(vh, this);
        }

        protected virtual void DataDirty()
        {
        }

        protected virtual void CheckTheme(bool firstInit = false)
        {
            if (m_Theme.sharedTheme == null) m_Theme.sharedTheme = XCThemeMgr.GetTheme(ThemeType.Default);
            if (firstInit) m_CheckTheme = m_Theme.themeType;
            if (m_Theme.sharedTheme != null && m_CheckTheme != m_Theme.themeType)
            {
                m_CheckTheme = m_Theme.themeType;
                m_Theme.sharedTheme.CopyTheme(m_CheckTheme);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
                SetAllDirty();
                SetAllComponentDirty();
                OnThemeChanged();
            }
        }

        protected virtual void OnThemeChanged()
        {
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            Awake();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
}