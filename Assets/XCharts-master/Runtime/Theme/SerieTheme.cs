using System;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    public class SerieTheme : ChildComponent
    {
        [SerializeField] protected float m_LineWidth;
        [SerializeField] protected float m_LineSymbolSize;
        [SerializeField] protected float m_ScatterSymbolSize;
        [SerializeField] protected Color32 m_CandlestickColor = new(235, 84, 84, 255);
        [SerializeField] protected Color32 m_CandlestickColor0 = new(71, 178, 98, 255);
        [SerializeField] protected float m_CandlestickBorderWidth = 1;
        [SerializeField] protected Color32 m_CandlestickBorderColor = new(235, 84, 84, 255);
        [SerializeField] protected Color32 m_CandlestickBorderColor0 = new(71, 178, 98, 255);

        public SerieTheme(ThemeType theme)
        {
            m_LineWidth = XCSettings.serieLineWidth;
            m_LineSymbolSize = XCSettings.serieLineSymbolSize;
            m_ScatterSymbolSize = XCSettings.serieScatterSymbolSize;
            m_CandlestickBorderWidth = XCSettings.serieCandlestickBorderWidth;
            switch (theme)
            {
                case ThemeType.Default:
                    m_CandlestickColor = ColorUtil.GetColor("#eb5454");
                    m_CandlestickColor0 = ColorUtil.GetColor("#47b262");
                    m_CandlestickBorderColor = ColorUtil.GetColor("#eb5454");
                    m_CandlestickBorderColor0 = ColorUtil.GetColor("#47b262");
                    break;
                case ThemeType.Light:
                    m_CandlestickColor = ColorUtil.GetColor("#eb5454");
                    m_CandlestickColor0 = ColorUtil.GetColor("#47b262");
                    m_CandlestickBorderColor = ColorUtil.GetColor("#eb5454");
                    m_CandlestickBorderColor0 = ColorUtil.GetColor("#47b262");
                    break;
                case ThemeType.Dark:
                    m_CandlestickColor = ColorUtil.GetColor("#f64e56");
                    m_CandlestickColor0 = ColorUtil.GetColor("#54ea92");
                    m_CandlestickBorderColor = ColorUtil.GetColor("#f64e56");
                    m_CandlestickBorderColor0 = ColorUtil.GetColor("#54ea92");
                    break;
            }
        }

        /// <summary>
        ///     the color of text.
        ///     ||文本颜色。
        /// </summary>
        public float lineWidth
        {
            get => m_LineWidth;
            set
            {
                if (PropertyUtil.SetStruct(ref m_LineWidth, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     the symbol size of line serie.
        ///     ||折线图的Symbol大小。
        /// </summary>
        public float lineSymbolSize
        {
            get => m_LineSymbolSize;
            set
            {
                if (PropertyUtil.SetStruct(ref m_LineSymbolSize, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     the symbol size of scatter serie.
        ///     ||散点图的Symbol大小。
        /// </summary>
        public float scatterSymbolSize
        {
            get => m_ScatterSymbolSize;
            set
            {
                if (PropertyUtil.SetStruct(ref m_ScatterSymbolSize, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     K线图阳线（涨）填充色
        /// </summary>
        public Color32 candlestickColor
        {
            get => m_CandlestickColor;
            set
            {
                if (PropertyUtil.SetColor(ref m_CandlestickColor, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     K线图阴线（跌）填充色
        /// </summary>
        public Color32 candlestickColor0
        {
            get => m_CandlestickColor0;
            set
            {
                if (PropertyUtil.SetColor(ref m_CandlestickColor0, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     K线图阳线（跌）边框色
        /// </summary>
        public Color32 candlestickBorderColor
        {
            get => m_CandlestickBorderColor;
            set
            {
                if (PropertyUtil.SetColor(ref m_CandlestickBorderColor, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     K线图阴线（跌）边框色
        /// </summary>
        public Color32 candlestickBorderColor0
        {
            get => m_CandlestickBorderColor0;
            set
            {
                if (PropertyUtil.SetColor(ref m_CandlestickBorderColor0, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     K线图边框宽度
        /// </summary>
        public float candlestickBorderWidth
        {
            get => m_CandlestickBorderWidth;
            set
            {
                if (PropertyUtil.SetStruct(ref m_CandlestickBorderWidth, value < 0 ? 0f : value)) SetVerticesDirty();
            }
        }

        public void Copy(SerieTheme theme)
        {
            m_LineWidth = theme.lineWidth;
            m_LineSymbolSize = theme.lineSymbolSize;
            m_ScatterSymbolSize = theme.scatterSymbolSize;
            m_CandlestickColor = theme.candlestickColor;
            m_CandlestickColor0 = theme.candlestickColor0;
            m_CandlestickBorderColor = theme.candlestickBorderColor;
            m_CandlestickBorderColor0 = theme.candlestickBorderColor0;
            m_CandlestickBorderWidth = theme.candlestickBorderWidth;
        }
    }
}