using UnityEngine;

namespace XCharts.Runtime
{
    public class InteractData
    {
        private float m_CurrentValue = float.NaN;
        private Color32 m_PreviousColor = ColorUtil.clearColor32;
        private Vector3 m_PreviousPosition = Vector3.one;
        private Color32 m_PreviousToColor = ColorUtil.clearColor32;
        private Color32 m_TargetColor = ColorUtil.clearColor32;
        private Vector3 m_TargetPosition = Vector3.one;
        private Color32 m_TargetToColor = ColorUtil.clearColor32;
        private float m_UpdateTime;

        internal float targetVaue { get; private set; } = float.NaN;

        internal float previousValue { get; private set; }

        internal bool valueEnable { get; private set; }

        internal bool updateFlag { get; private set; }

        public override string ToString()
        {
            return string.Format(
                "m_PreviousValue:{0},m_TargetValue:{1},m_UpdateTime:{2},m_UpdateFlag:{3},m_ValueEnable:{4},m_PreviousPosition:{5},m_TargetPosition:{6}",
                previousValue, targetVaue, m_UpdateTime, updateFlag, valueEnable, m_PreviousPosition, m_TargetPosition);
        }

        public void SetValue(ref bool needInteract, float value, bool highlight, float rate = 1.3f)
        {
            value = highlight && rate != 0 ? value * rate : value;
            SetValue(ref needInteract, value);
        }

        public void SetValue(ref bool needInteract, float value, bool previousValueZero = false)
        {
            if (targetVaue != value)
            {
                needInteract = true;
                if (!valueEnable)
                    previousValue = previousValueZero ? 0 : value;
                else
                    previousValue = m_CurrentValue;
                UpdateStart();
                targetVaue = value;
            }
            else if (updateFlag)
            {
                needInteract = true;
            }
        }

        public void SetPosition(ref bool needInteract, Vector3 pos)
        {
            if (m_TargetPosition != pos)
            {
                needInteract = true;
                UpdateStart();
                m_PreviousPosition = m_TargetPosition == Vector3.one ? pos : m_TargetPosition;
                m_TargetPosition = pos;
            }
        }

        public void SetColor(ref bool needInteract, Color32 color)
        {
            if (!ChartHelper.IsValueEqualsColor(color, m_TargetColor))
            {
                needInteract = true;
                UpdateStart();
                m_PreviousColor = ChartHelper.IsClearColor(m_TargetColor) ? color : m_TargetColor;
                m_TargetColor = color;
            }
            else if (updateFlag)
            {
                needInteract = true;
            }
        }

        public void SetColor(ref bool needInteract, Color32 color, Color32 toColor)
        {
            SetColor(ref needInteract, color);
            if (!ChartHelper.IsValueEqualsColor(toColor, m_TargetToColor))
            {
                needInteract = true;
                UpdateStart();
                m_PreviousToColor = ChartHelper.IsClearColor(m_TargetToColor) ? color : m_TargetToColor;
                m_TargetToColor = toColor;
            }
        }

        public void SetValueAndColor(ref bool needInteract, float value, Color32 color)
        {
            SetValue(ref needInteract, value);
            SetColor(ref needInteract, color);
        }

        public void SetValueAndColor(ref bool needInteract, float value, Color32 color, Color32 toColor)
        {
            SetValue(ref needInteract, value);
            SetColor(ref needInteract, color, toColor);
        }

        public bool TryGetValue(ref float value, ref bool interacting, float animationDuration = 250)
        {
            if (!IsValueEnable() || animationDuration == 0)
                return false;
            if (float.IsNaN(targetVaue))
                return false;
            if (updateFlag && !float.IsNaN(previousValue))
            {
                var rate = GetRate(animationDuration);
                if (rate < 1)
                {
                    interacting = true;
                    value = Mathf.Lerp(previousValue, targetVaue, rate);
                    m_CurrentValue = value;
                    return true;
                }

                UpdateEnd();
            }

            value = targetVaue;
            return true;
        }

        public bool TryGetPosition(ref Vector3 pos, ref bool interacting, float animationDuration = 250)
        {
            if (!IsValueEnable() || animationDuration == 0)
                return false;
            if (m_TargetPosition == Vector3.one) return false;
            if (updateFlag && m_PreviousPosition != Vector3.one)
            {
                var rate = GetRate(animationDuration);
                if (rate < 1)
                {
                    interacting = true;
                    pos = Vector3.Lerp(m_PreviousPosition, m_TargetPosition, rate);
                    return true;
                }

                UpdateEnd();
            }

            pos = m_TargetPosition;
            return true;
        }

        public bool TryGetColor(ref Color32 color, ref bool interacting, float animationDuration = 250)
        {
            if (!IsValueEnable() || animationDuration == 0)
                return false;
            if (updateFlag)
            {
                var rate = GetRate(animationDuration);
                if (rate < 1)
                {
                    interacting = true;
                    color = Color32.Lerp(m_PreviousColor, m_TargetColor, rate);
                    return true;
                }

                UpdateEnd();
            }

            color = m_TargetColor;
            return true;
        }

        public bool TryGetColor(ref Color32 color, ref Color32 toColor, ref bool interacting,
            float animationDuration = 250)
        {
            if (!IsValueEnable() || animationDuration == 0)
                return false;
            if (updateFlag)
            {
                var rate = GetRate(animationDuration);
                if (rate < 1)
                {
                    interacting = true;
                    color = Color32.Lerp(m_PreviousColor, m_TargetColor, rate);
                    toColor = Color32.Lerp(m_PreviousToColor, m_TargetToColor, rate);
                    return true;
                }

                UpdateEnd();
            }

            color = m_TargetColor;
            toColor = m_TargetToColor;
            return true;
        }

        public bool TryGetValueAndColor(ref float value, ref Color32 color, ref Color32 toColor, ref bool interacting,
            float animationDuration = 250)
        {
            if (!IsValueEnable() || animationDuration == 0)
                return false;
            if (float.IsNaN(targetVaue))
                return false;
            if (updateFlag && !float.IsNaN(previousValue))
            {
                var rate = GetRate(animationDuration);
                if (rate < 1)
                {
                    interacting = true;
                    value = Mathf.Lerp(previousValue, targetVaue, rate);
                    color = Color32.Lerp(m_PreviousColor, m_TargetColor, rate);
                    toColor = Color32.Lerp(m_PreviousToColor, m_TargetToColor, rate);
                    m_CurrentValue = value;
                    return true;
                }

                UpdateEnd();
            }

            value = targetVaue;
            color = m_TargetColor;
            toColor = m_TargetToColor;
            return true;
        }

        private float GetRate(float animationDuration)
        {
            var time = Time.time - m_UpdateTime;
            var total = animationDuration / 1000;
            var rate = time / total;
            if (rate > 1) rate = 1;
            return rate;
        }

        private void UpdateStart()
        {
            valueEnable = true;
            updateFlag = true;
            m_UpdateTime = Time.time;
        }

        private void UpdateEnd()
        {
            if (!updateFlag) return;
            updateFlag = false;
            m_PreviousColor = m_TargetColor;
            m_PreviousToColor = m_TargetToColor;
            previousValue = targetVaue;
            m_CurrentValue = targetVaue;
            m_PreviousPosition = m_TargetPosition;
        }

        public bool TryGetValueAndColor(ref float value, ref Vector3 pos, ref Color32 color, ref Color32 toColor,
            ref bool interacting, float animationDuration = 250)
        {
            var flag = TryGetValueAndColor(ref value, ref color, ref toColor, ref interacting, animationDuration);
            flag |= TryGetPosition(ref pos, ref interacting, animationDuration);
            return flag;
        }

        public bool TryGetValueAndColor(ref float value, ref Vector3 pos, ref bool interacting,
            float animationDuration = 250)
        {
            var flag = TryGetValue(ref value, ref interacting, animationDuration);
            flag |= TryGetPosition(ref pos, ref interacting, animationDuration);
            return flag;
        }

        public void Reset()
        {
            updateFlag = false;
            valueEnable = false;
            targetVaue = float.NaN;
            previousValue = float.NaN;
            m_CurrentValue = float.NaN;
            m_PreviousPosition = Vector3.one;
            m_TargetPosition = Vector3.one;
            m_TargetColor = ColorUtil.clearColor32;
            m_TargetToColor = ColorUtil.clearColor32;
            m_PreviousColor = ColorUtil.clearColor32;
            m_PreviousToColor = ColorUtil.clearColor32;
        }

        private bool IsValueEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif
            return valueEnable;
        }
    }
}