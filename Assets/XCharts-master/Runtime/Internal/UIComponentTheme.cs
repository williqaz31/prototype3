using System;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    public class UIComponentTheme : ChildComponent
    {
        [SerializeField] private bool m_Show = true;
        [SerializeField] private Theme m_SharedTheme;
        [SerializeField] private bool m_TransparentBackground;

        public bool show => m_Show;

        /// <summary>
        ///     the theme of chart.
        ///     ||主题类型。
        /// </summary>
        public ThemeType themeType => sharedTheme.themeType;

        /// <summary>
        ///     theme name.
        ///     ||主题名字。
        /// </summary>
        public string themeName => sharedTheme.themeName;

        /// <summary>
        ///     the asset of theme.
        ///     ||主题配置。
        /// </summary>
        public Theme sharedTheme
        {
            get => m_SharedTheme;
            set
            {
                m_SharedTheme = value;
                SetAllDirty();
            }
        }

        /// <summary>
        ///     the background color of chart.
        ///     ||背景颜色。
        /// </summary>
        public Color32 backgroundColor
        {
            get
            {
                if (m_TransparentBackground) return ColorUtil.clearColor32;
                if (sharedTheme != null) return sharedTheme.backgroundColor;
                return ColorUtil.clearColor32;
            }
        }

        public Color32 GetBackgroundColor(Background background)
        {
            if (background != null && background.show && !background.autoColor)
                return background.imageColor;
            return backgroundColor;
        }
    }
}