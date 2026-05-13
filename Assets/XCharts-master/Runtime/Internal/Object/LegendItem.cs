using UnityEngine;
using UnityEngine.UI;

namespace XCharts.Runtime
{
    public class LegendItem
    {
        private Image m_Background;
        private float m_Gap;
        private Image m_Icon;
        private RectTransform m_IconRect;
        private readonly bool m_LabelAutoSize = true;
        private readonly float m_LabelPaddingLeftRight = 0f;
        private readonly float m_LabelPaddingTopBottom = 0f;
        private RectTransform m_Rect;
        private Image m_TextBackground;
        private RectTransform m_TextBackgroundRect;
        private RectTransform m_TextRect;

        public int index { get; set; }

        public string name { get; set; }

        public string legendName { get; set; }

        public GameObject gameObject { get; private set; }

        public Button button { get; private set; }

        public ChartText text { get; private set; }

        public float width
        {
            get
            {
                if (m_IconRect && m_TextBackgroundRect)
                    return m_IconRect.sizeDelta.x + m_Gap + m_TextBackgroundRect.sizeDelta.x;

                return 0;
            }
        }

        public float height
        {
            get
            {
                if (m_IconRect && m_TextBackgroundRect)
                    return Mathf.Max(m_IconRect.sizeDelta.y, m_TextBackgroundRect.sizeDelta.y);

                return text.GetPreferredHeight();
            }
        }

        public void SetObject(GameObject obj)
        {
            gameObject = obj;
            button = obj.GetComponent<Button>();
            m_Rect = obj.GetComponent<RectTransform>();
            m_Icon = obj.transform.Find("icon").gameObject.GetComponent<Image>();
            m_Background = obj.GetComponent<Image>();
            m_TextBackground = obj.transform.Find("content").gameObject.GetComponent<Image>();
            text = new ChartText(obj);
            m_IconRect = m_Icon.gameObject.GetComponent<RectTransform>();
            m_TextRect = text.gameObject.GetComponent<RectTransform>();
            m_TextBackgroundRect = m_TextBackground.gameObject.GetComponent<RectTransform>();
        }

        public void SetButton(Button button)
        {
            this.button = button;
        }

        public void SetIcon(Image icon)
        {
            m_Icon = icon;
        }

        public void SetText(ChartText text)
        {
            this.text = text;
        }

        public void SetTextBackground(Image image)
        {
            m_TextBackground = image;
        }

        public void SetIconSize(float width, float height)
        {
            if (m_IconRect) m_IconRect.sizeDelta = new Vector2(width, height);
        }

        public Rect GetIconRect()
        {
            if (gameObject && m_IconRect)
            {
                var pos = gameObject.transform.localPosition;
                var sizeDelta = m_IconRect.sizeDelta;
                var y = pos.y - (m_Rect.sizeDelta.y - sizeDelta.y) / 2 - sizeDelta.y;
                return new Rect(pos.x, y, m_IconRect.sizeDelta.x, m_IconRect.sizeDelta.y);
            }

            return Rect.zero;
        }

        public Color GetIconColor()
        {
            if (m_Icon) return m_Icon.color;
            return Color.clear;
        }

        public void SetIconColor(Color color)
        {
            if (m_Icon) m_Icon.color = color;
        }

        public void SetIconImage(Sprite image)
        {
            if (m_Icon) m_Icon.sprite = image;
        }

        public void SetIconActive(bool active)
        {
            if (m_Icon) m_Icon.gameObject.SetActive(active);
        }

        public void SetContentColor(Color color)
        {
            if (text != null) text.SetColor(color);
        }

        public void SetContentBackgroundColor(Color color)
        {
            if (m_TextBackground) m_TextBackground.color = color;
        }

        public void SetContentPosition(Vector3 offset)
        {
            m_Gap = offset.x;
            if (m_TextBackgroundRect)
            {
                var posX = m_IconRect.sizeDelta.x + offset.x;
                m_TextBackgroundRect.anchoredPosition3D = new Vector3(posX, offset.y, 0);
            }
        }

        public bool SetContent(string content)
        {
            if (text == null) return false;
            if (!text.GetText().Equals(content))
            {
                text.SetText(content);
                if (m_LabelAutoSize)
                {
                    var newSize = string.IsNullOrEmpty(content)
                        ? Vector2.zero
                        : new Vector2(text.GetPreferredWidth(), text.GetPreferredHeight());
                    var sizeChange = newSize.x != m_TextRect.sizeDelta.x || newSize.y != m_TextRect.sizeDelta.y;
                    if (sizeChange)
                    {
                        m_TextRect.sizeDelta = newSize;
                        m_TextRect.anchoredPosition3D = new Vector3(m_LabelPaddingLeftRight, 0);
                        m_TextBackgroundRect.sizeDelta = new Vector2(
                            text.GetPreferredWidth() + m_LabelPaddingLeftRight * 2,
                            text.GetPreferredHeight() + m_LabelPaddingTopBottom * 2 - 4);
                    }

                    m_Rect.sizeDelta = new Vector3(width, height);
                    return sizeChange;
                }
            }

            m_Rect.sizeDelta = new Vector3(width, height);
            return false;
        }

        public void SetPosition(Vector3 position)
        {
            if (gameObject) gameObject.transform.localPosition = position;
        }

        public void SetActive(bool active)
        {
            if (gameObject) gameObject.SetActive(active);
        }

        public void SetBackground(ImageStyle imageStyle)
        {
            ChartHelper.SetBackground(m_Background, imageStyle);
        }
    }
}