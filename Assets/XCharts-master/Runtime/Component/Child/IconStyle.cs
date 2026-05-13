using System;
using UnityEngine;
using UnityEngine.UI;

namespace XCharts.Runtime
{
    [Serializable]
    public class IconStyle : ChildComponent
    {
        public enum Layer
        {
            /// <summary>
            ///     The icon is display under the label text.
            ///     图标在标签文字下
            /// </summary>
            UnderText,

            /// <summary>
            ///     The icon is display above the label text.
            ///     图标在标签文字上
            /// </summary>
            AboveText
        }

        [SerializeField] private bool m_Show;
        [SerializeField] private Layer m_Layer;
        [SerializeField] private Align m_Align = Align.Left;
        [SerializeField] private Sprite m_Sprite;
        [SerializeField] private Image.Type m_Type;
        [SerializeField] private Color m_Color = Color.white;
        [SerializeField] private float m_Width = 20;
        [SerializeField] private float m_Height = 20;
        [SerializeField] private Vector3 m_Offset;
        [SerializeField] private bool m_AutoHideWhenLabelEmpty;

        /// <summary>
        ///     Whether the data icon is show.
        ///     ||是否显示图标。
        /// </summary>
        public bool show
        {
            get => m_Show;
            set => m_Show = value;
        }

        /// <summary>
        ///     显示在上层还是在下层。
        /// </summary>
        public Layer layer
        {
            get => m_Layer;
            set => m_Layer = value;
        }

        /// <summary>
        ///     The image of icon.
        ///     ||图标的图片。
        /// </summary>
        public Sprite sprite
        {
            get => m_Sprite;
            set => m_Sprite = value;
        }

        /// <summary>
        ///     How to display the icon.
        ///     ||图片的显示类型。
        /// </summary>
        public Image.Type type
        {
            get => m_Type;
            set => m_Type = value;
        }

        /// <summary>
        ///     图标颜色。
        /// </summary>
        public Color color
        {
            get => m_Color;
            set => m_Color = value;
        }

        /// <summary>
        ///     图标宽。
        /// </summary>
        public float width
        {
            get => m_Width;
            set => m_Width = value;
        }

        /// <summary>
        ///     图标高。
        /// </summary>
        public float height
        {
            get => m_Height;
            set => m_Height = value;
        }

        /// <summary>
        ///     图标偏移。
        /// </summary>
        public Vector3 offset
        {
            get => m_Offset;
            set => m_Offset = value;
        }

        /// <summary>
        ///     水平方向对齐方式。
        /// </summary>
        public Align align
        {
            get => m_Align;
            set => m_Align = value;
        }

        /// <summary>
        ///     当label内容为空时是否自动隐藏图标
        /// </summary>
        public bool autoHideWhenLabelEmpty
        {
            get => m_AutoHideWhenLabelEmpty;
            set => m_AutoHideWhenLabelEmpty = value;
        }

        public void Reset()
        {
            m_Show = false;
            m_Layer = Layer.UnderText;
            m_Sprite = null;
            m_Color = Color.white;
            m_Width = 20;
            m_Height = 20;
            m_Offset = Vector3.zero;
            m_AutoHideWhenLabelEmpty = false;
        }

        public IconStyle Clone()
        {
            var iconStyle = new IconStyle();
            iconStyle.show = show;
            iconStyle.layer = layer;
            iconStyle.sprite = sprite;
            iconStyle.type = type;
            iconStyle.color = color;
            iconStyle.width = width;
            iconStyle.height = height;
            iconStyle.offset = offset;
            iconStyle.align = align;
            iconStyle.autoHideWhenLabelEmpty = autoHideWhenLabelEmpty;
            return iconStyle;
        }

        public void Copy(IconStyle iconStyle)
        {
            show = iconStyle.show;
            layer = iconStyle.layer;
            sprite = iconStyle.sprite;
            type = iconStyle.type;
            color = iconStyle.color;
            width = iconStyle.width;
            height = iconStyle.height;
            offset = iconStyle.offset;
            align = iconStyle.align;
            autoHideWhenLabelEmpty = iconStyle.autoHideWhenLabelEmpty;
        }
    }
}