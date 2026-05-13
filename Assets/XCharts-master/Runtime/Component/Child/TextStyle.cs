using System;
using UnityEngine;
#if dUI_TextMeshPro
using TMPro;
#endif

namespace XCharts.Runtime
{
    /// <summary>
    ///     Settings related to text.
    ///     ||文本的相关设置。
    /// </summary>
    [Serializable]
    public class TextStyle : ChildComponent
    {
        [SerializeField] private bool m_Show = true;
        [SerializeField] private Font m_Font;
        [SerializeField] private bool m_AutoWrap;
        [SerializeField] private bool m_AutoAlign = true;
        [SerializeField] private float m_Rotate;
        [SerializeField] private bool m_AutoColor;
        [SerializeField] private Color m_Color = Color.clear;
        [SerializeField] private int m_FontSize;
        [SerializeField] private FontStyle m_FontStyle = FontStyle.Normal;
        [SerializeField] private float m_LineSpacing = 1f;
        [SerializeField] private TextAnchor m_Alignment = TextAnchor.MiddleCenter;
#if dUI_TextMeshPro
        [SerializeField] private TMP_FontAsset m_TMPFont;
        [SerializeField] private FontStyles m_TMPFontStyle = FontStyles.Normal;
        [SerializeField][Since("v3.1.0")] private TMP_SpriteAsset m_TMPSpriteAsset;
#endif
        public bool show
        {
            get => m_Show;
            set
            {
                if (PropertyUtil.SetStruct(ref m_Show, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     Rotation of text.
        ///     ||文本的旋转。
        ///     [default: `0f`]
        /// </summary>
        public float rotate
        {
            get => m_Rotate;
            set
            {
                if (PropertyUtil.SetStruct(ref m_Rotate, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     是否开启自动颜色。当开启时，会自动设置颜色。
        /// </summary>
        public bool autoColor
        {
            get => m_AutoColor;
            set
            {
                if (PropertyUtil.SetStruct(ref m_AutoColor, value)) SetAllDirty();
            }
        }

        /// <summary>
        ///     the color of text.
        ///     ||文本的颜色。
        ///     [default: `Color.clear`]
        /// </summary>
        public Color color
        {
            get => m_Color;
            set
            {
                if (PropertyUtil.SetColor(ref m_Color, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     the font of text. When `null`, the theme's font is used by default.
        ///     ||文本字体。
        ///     [default: null]
        /// </summary>
        public Font font
        {
            get => m_Font;
            set
            {
                if (PropertyUtil.SetClass(ref m_Font, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     font size.
        ///     ||文本字体大小。
        ///     [default: 18]
        /// </summary>
        public int fontSize
        {
            get => m_FontSize;
            set
            {
                if (PropertyUtil.SetStruct(ref m_FontSize, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     font style.
        ///     ||文本字体的风格。
        ///     [default: FontStyle.Normal]
        /// </summary>
        public FontStyle fontStyle
        {
            get => m_FontStyle;
            set
            {
                if (PropertyUtil.SetStruct(ref m_FontStyle, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     text line spacing.
        ///     ||行间距。
        ///     [default: 1f]
        /// </summary>
        public float lineSpacing
        {
            get => m_LineSpacing;
            set
            {
                if (PropertyUtil.SetStruct(ref m_LineSpacing, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     是否自动换行。
        /// </summary>
        public bool autoWrap
        {
            get => m_AutoWrap;
            set
            {
                if (PropertyUtil.SetStruct(ref m_AutoWrap, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     文本是否让系统自动选对齐方式。为false时才会用alignment。
        /// </summary>
        public bool autoAlign
        {
            get => m_AutoAlign;
            set
            {
                if (PropertyUtil.SetStruct(ref m_AutoAlign, value)) SetComponentDirty();
            }
        }

        /// <summary>
        ///     对齐方式。
        /// </summary>
        public TextAnchor alignment
        {
            get => m_Alignment;
            set
            {
                if (PropertyUtil.SetStruct(ref m_Alignment, value)) SetComponentDirty();
            }
        }
#if dUI_TextMeshPro
        /// <summary>
        /// the font of textmeshpro.
        /// ||TextMeshPro字体。
        /// </summary>
        public TMP_FontAsset tmpFont
        {
            get { return m_TMPFont; }
            set { if (PropertyUtil.SetClass(ref m_TMPFont, value)) SetComponentDirty(); }
        }
        /// <summary>
        /// the font style of TextMeshPro.
        /// ||TextMeshPro字体类型。
        /// </summary>
        public FontStyles tmpFontStyle
        {
            get { return m_TMPFontStyle; }
            set { if (PropertyUtil.SetStruct(ref m_TMPFontStyle, value)) SetComponentDirty(); }
        }
        /// <summary>
        /// the sprite asset of TextMeshPro.
        /// ||TextMeshPro的Sprite Asset。
        /// </summary>
        public TMP_SpriteAsset tmpSpriteAsset
        {
            get { return m_TMPSpriteAsset; }
            set { if (PropertyUtil.SetClass(ref m_TMPSpriteAsset, value)) SetComponentDirty(); }
        }
#endif

        public TextStyle()
        {
        }

        public TextStyle(int fontSize)
        {
            this.fontSize = fontSize;
        }

        public TextStyle(int fontSize, FontStyle fontStyle)
        {
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;
        }

        public TextStyle(int fontSize, FontStyle fontStyle, Color color)
        {
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;
            this.color = color;
        }

        public TextStyle(int fontSize, FontStyle fontStyle, Color color, int rorate)
        {
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;
            this.color = color;
            rotate = rotate;
        }

        public void Copy(TextStyle textStyle)
        {
            font = textStyle.font;
            rotate = textStyle.rotate;
            color = textStyle.color;
            fontSize = textStyle.fontSize;
            fontStyle = textStyle.fontStyle;
            lineSpacing = textStyle.lineSpacing;
            alignment = textStyle.alignment;
            autoWrap = textStyle.autoWrap;
            autoAlign = textStyle.autoAlign;
#if dUI_TextMeshPro
            m_TMPFont = textStyle.tmpFont;
            m_TMPFontStyle = textStyle.tmpFontStyle;
            m_TMPSpriteAsset = textStyle.tmpSpriteAsset;
#endif
        }

        public void UpdateAlignmentByLocation(Location location)
        {
            m_Alignment = location.runtimeTextAlignment;
        }

        public Color GetColor(Color defaultColor)
        {
            if (ChartHelper.IsClearColor(color))
                return defaultColor;
            return color;
        }

        public int GetFontSize(ComponentTheme defaultTheme)
        {
            if (fontSize == 0)
                return defaultTheme.fontSize;
            return fontSize;
        }

        public TextAnchor GetAlignment(TextAnchor defaultAlignment)
        {
            return m_AutoAlign ? defaultAlignment : alignment;
        }
    }
}