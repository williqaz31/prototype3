using System;
using System.Collections.Generic;
using UnityEngine;
using XUGL;

namespace XCharts.Runtime
{
    /// <summary>
    ///     A data item of serie.
    ///     ||系列中的一个数据项。可存储数据名和1-n维个数据。
    /// </summary>
    [Serializable]
    public class SerieData : ChildComponent
    {
        public static List<string> extraFieldList = new()
        {
            "m_Id",
            "m_ParentId",
            "m_State",
            "m_Ignore",
            "m_Selected",
            "m_Radius"
        };

        public static Dictionary<Type, string> extraComponentMap = new()
        {
            { typeof(ItemStyle), "m_ItemStyles" },
            { typeof(LabelStyle), "m_Labels" },
            { typeof(LabelLine), "m_LabelLines" },
            { typeof(SerieSymbol), "m_Symbols" },
            { typeof(LineStyle), "m_LineStyles" },
            { typeof(AreaStyle), "m_AreaStyles" },
            { typeof(TitleStyle), "m_TitleStyles" },
            { typeof(EmphasisStyle), "m_EmphasisStyles" },
            { typeof(BlurStyle), "m_BlurStyles" },
            { typeof(SelectStyle), "m_SelectStyles" }
        };

        [SerializeField] private int m_Index;
        [SerializeField] private string m_Name;
        [SerializeField] private string m_Id;
        [SerializeField] private string m_ParentId;
        [SerializeField] private bool m_Ignore;
        [SerializeField] private bool m_Selected;
        [SerializeField] private float m_Radius;
        [SerializeField] [Since("v3.2.0")] private SerieState m_State = SerieState.Auto;
        [SerializeField] [IgnoreDoc] private List<ItemStyle> m_ItemStyles = new();
        [SerializeField] [IgnoreDoc] private List<LabelStyle> m_Labels = new();
        [SerializeField] [IgnoreDoc] private List<LabelLine> m_LabelLines = new();
        [SerializeField] [IgnoreDoc] private List<SerieSymbol> m_Symbols = new();
        [SerializeField] [IgnoreDoc] private List<LineStyle> m_LineStyles = new();
        [SerializeField] [IgnoreDoc] private List<AreaStyle> m_AreaStyles = new();
        [SerializeField] [IgnoreDoc] private List<TitleStyle> m_TitleStyles = new();
        [SerializeField] [IgnoreDoc] private List<EmphasisStyle> m_EmphasisStyles = new();
        [SerializeField] [IgnoreDoc] private List<BlurStyle> m_BlurStyles = new();
        [SerializeField] [IgnoreDoc] private List<SelectStyle> m_SelectStyles = new();
        [SerializeField] private List<double> m_Data = new();

        [NonSerialized] public SerieDataContext context = new();
        [NonSerialized] public InteractData interact = new();
        private List<bool> m_DataAddFlag = new();
        private List<float> m_DataAddTime = new();
        private List<bool> m_DataUpdateFlag = new();
        private List<float> m_DataUpdateTime = new();
        private List<Vector2> m_PolygonPoints = new();

        private List<double> m_PreviousData = new();

        private bool m_Show = true;

        public ChartLabel labelObject { get; set; }
        public ChartLabel titleObject { get; set; }
        public int sortIndex { get; set; }

        /// <summary>
        ///     the index of SerieData.
        ///     ||数据项索引。
        /// </summary>
        public override int index
        {
            get => m_Index;
            set => m_Index = value;
        }

        /// <summary>
        ///     the name of data item.
        ///     ||数据项名称。
        /// </summary>
        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        ///     the id of data.
        ///     ||数据项的唯一id。唯一id不是必须设置的。
        /// </summary>
        public string id
        {
            get => m_Id;
            set => m_Id = value;
        }

        /// <summary>
        ///     the id of parent SerieData.
        ///     ||父节点id。父节点id不是必须设置的。
        /// </summary>
        public string parentId
        {
            get => m_ParentId;
            set => m_ParentId = value;
        }

        /// <summary>
        ///     是否忽略数据。当为 true 时，数据不进行绘制。
        /// </summary>
        public bool ignore
        {
            get => m_Ignore;
            set
            {
                if (PropertyUtil.SetStruct(ref m_Ignore, value)) SetVerticesDirty();
            }
        }

        /// <summary>
        ///     自定义半径。可用在饼图中自定义某个数据项的半径。
        /// </summary>
        public float radius
        {
            get => m_Radius;
            set => m_Radius = value;
        }

        /// <summary>
        ///     Whether the data item is selected.
        ///     ||该数据项是否被选中。
        /// </summary>
        public bool selected
        {
            get => m_Selected;
            set => m_Selected = value;
        }

        /// <summary>
        ///     the state of serie data.
        ///     ||数据项的默认状态。
        /// </summary>
        public SerieState state
        {
            get => m_State;
            set => m_State = value;
        }

        /// <summary>
        ///     数据项图例名称。当数据项名称不为空时，图例名称即为系列名称；反之则为索引index。
        /// </summary>
        public string legendName => string.IsNullOrEmpty(name) ? ChartCached.IntToStr(index) : name;

        /// <summary>
        ///     单个数据项的标签设置。
        /// </summary>
        public LabelStyle labelStyle => m_Labels.Count > 0 ? m_Labels[0] : null;

        public LabelLine labelLine => m_LabelLines.Count > 0 ? m_LabelLines[0] : null;

        /// <summary>
        ///     单个数据项的样式设置。
        /// </summary>
        public ItemStyle itemStyle => m_ItemStyles.Count > 0 ? m_ItemStyles[0] : null;

        /// <summary>
        ///     单个数据项的标记设置。
        /// </summary>
        public SerieSymbol symbol => m_Symbols.Count > 0 ? m_Symbols[0] : null;

        public LineStyle lineStyle => m_LineStyles.Count > 0 ? m_LineStyles[0] : null;
        public AreaStyle areaStyle => m_AreaStyles.Count > 0 ? m_AreaStyles[0] : null;
        public TitleStyle titleStyle => m_TitleStyles.Count > 0 ? m_TitleStyles[0] : null;

        /// <summary>
        ///     高亮状态的样式
        /// </summary>
        public EmphasisStyle emphasisStyle => m_EmphasisStyles.Count > 0 ? m_EmphasisStyles[0] : null;

        /// <summary>
        ///     淡出状态的样式。
        /// </summary>
        public BlurStyle blurStyle => m_BlurStyles.Count > 0 ? m_BlurStyles[0] : null;

        /// <summary>
        ///     选中状态的样式。
        /// </summary>
        public SelectStyle selectStyle => m_SelectStyles.Count > 0 ? m_SelectStyles[0] : null;

        /// <summary>
        ///     An arbitrary dimension data list of data item.
        ///     ||可指定任意维数的数值列表。
        /// </summary>
        public List<double> data
        {
            get => m_Data;
            set => m_Data = value;
        }

        /// <summary>
        ///     [default:true] Whether the data item is showed.
        ///     ||该数据项是否要显示。
        /// </summary>
        public bool show
        {
            get => m_Show;
            set => m_Show = value;
        }

        public override bool vertsDirty =>
            m_VertsDirty ||
            IsVertsDirty(labelLine) ||
            IsVertsDirty(itemStyle) ||
            IsVertsDirty(symbol) ||
            IsVertsDirty(lineStyle) ||
            IsVertsDirty(areaStyle) ||
            IsVertsDirty(emphasisStyle) ||
            IsVertsDirty(blurStyle) ||
            IsVertsDirty(selectStyle);

        public override bool componentDirty =>
            m_ComponentDirty ||
            IsComponentDirty(labelStyle) ||
            IsComponentDirty(labelLine) ||
            IsComponentDirty(titleStyle) ||
            IsComponentDirty(emphasisStyle) ||
            IsComponentDirty(blurStyle) ||
            IsComponentDirty(selectStyle);

        public override void ClearVerticesDirty()
        {
            base.ClearVerticesDirty();
            ClearVerticesDirty(labelLine);
            ClearVerticesDirty(itemStyle);
            ClearVerticesDirty(lineStyle);
            ClearVerticesDirty(areaStyle);
            ClearVerticesDirty(emphasisStyle);
            ClearVerticesDirty(blurStyle);
            ClearVerticesDirty(selectStyle);
        }

        public override void ClearComponentDirty()
        {
            base.ClearComponentDirty();
            ClearComponentDirty(labelLine);
            ClearComponentDirty(itemStyle);
            ClearComponentDirty(lineStyle);
            ClearComponentDirty(areaStyle);
            ClearComponentDirty(symbol);
            ClearComponentDirty(emphasisStyle);
            ClearComponentDirty(blurStyle);
            ClearComponentDirty(selectStyle);
        }

        public void Reset()
        {
            index = 0;
            m_Id = null;
            m_ParentId = null;
            labelObject = null;
            m_Name = string.Empty;
            m_Show = true;
            context.Reset();
            interact.Reset();
            m_Data.Clear();
            m_PreviousData.Clear();
            m_DataUpdateTime.Clear();
            m_DataUpdateFlag.Clear();
            m_DataAddTime.Clear();
            m_DataAddFlag.Clear();
            m_Labels.Clear();
            m_LabelLines.Clear();
            m_ItemStyles.Clear();
            m_Symbols.Clear();
            m_LineStyles.Clear();
            m_AreaStyles.Clear();
            m_TitleStyles.Clear();
            m_EmphasisStyles.Clear();
            m_BlurStyles.Clear();
            m_SelectStyles.Clear();
        }

        public void OnAdd(AnimationStyle animation, double startValue = 0)
        {
            if (!animation.enable) return;
            if (!animation.context.enableSerieDataAddedAnimation)
            {
                animation.Addition();
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            m_DataAddTime.Clear();
            m_DataAddFlag.Clear();
            if (animation.GetAdditionDuration() > 0)
                for (var i = 0; i < m_Data.Count; i++)
                {
                    m_DataAddTime.Add(animation.unscaledTime ? Time.unscaledTime : Time.time);
                    m_DataAddFlag.Add(true);
                }
        }

        [Obsolete("GetOrAddComponent is obsolete. Use EnsureComponent instead.")]
        public T GetOrAddComponent<T>() where T : ChildComponent, ISerieDataComponent
        {
            return EnsureComponent<T>();
        }

        /// <summary>
        ///     Get the component of the serie data. return null if not exist.
        ///     ||获取数据项的指定类型的组件，如果不存在则返回null。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : ChildComponent, ISerieDataComponent
        {
            return GetComponentInternal(typeof(T), false) as T;
        }

        /// <summary>
        ///     Ensure the serie data has the component, if not, add it.
        ///     ||确保数据项有指定类型的组件，如果没有则添加。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Since("v3.6.0")]
        public T EnsureComponent<T>() where T : ChildComponent, ISerieDataComponent
        {
            return GetComponentInternal(typeof(T), true) as T;
        }

        /// <summary>
        ///     Ensure the serie data has the component, if not, add it.
        ///     ||确保数据项有指定类型的组件，如果没有则添加。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Since("v3.6.0")]
        public ISerieDataComponent EnsureComponent(Type type)
        {
            return GetComponentInternal(type, true);
        }

        private ISerieDataComponent GetComponentInternal(Type type, bool addIfNotExist)
        {
            if (type == typeof(ItemStyle))
            {
                if (m_ItemStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_ItemStyles.Add(new ItemStyle { show = true });
                    else
                        return null;
                }

                return m_ItemStyles[0];
            }

            if (type == typeof(LabelStyle))
            {
                if (m_Labels.Count == 0)
                {
                    if (addIfNotExist)
                        m_Labels.Add(new LabelStyle { show = true });
                    else
                        return null;
                }

                return m_Labels[0];
            }

            if (type == typeof(LabelLine))
            {
                if (m_LabelLines.Count == 0)
                {
                    if (addIfNotExist)
                        m_LabelLines.Add(new LabelLine { show = true });
                    else
                        return null;
                }

                return m_LabelLines[0];
            }

            if (type == typeof(EmphasisStyle))
            {
                if (m_EmphasisStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_EmphasisStyles.Add(new EmphasisStyle { show = true });
                    else
                        return null;
                }

                return m_EmphasisStyles[0];
            }

            if (type == typeof(BlurStyle))
            {
                if (m_BlurStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_BlurStyles.Add(new BlurStyle { show = true });
                    else
                        return null;
                }

                return m_BlurStyles[0];
            }

            if (type == typeof(SelectStyle))
            {
                if (m_SelectStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_SelectStyles.Add(new SelectStyle { show = true });
                    else
                        return null;
                }

                return m_SelectStyles[0];
            }

            if (type == typeof(SerieSymbol))
            {
                if (m_Symbols.Count == 0)
                {
                    if (addIfNotExist)
                        m_Symbols.Add(new SerieSymbol { show = true });
                    else
                        return null;
                }

                return m_Symbols[0];
            }

            if (type == typeof(LineStyle))
            {
                if (m_LineStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_LineStyles.Add(new LineStyle { show = true });
                    else
                        return null;
                }

                return m_LineStyles[0];
            }

            if (type == typeof(AreaStyle))
            {
                if (m_AreaStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_AreaStyles.Add(new AreaStyle { show = true });
                    else
                        return null;
                }

                return m_AreaStyles[0];
            }

            if (type == typeof(TitleStyle))
            {
                if (m_TitleStyles.Count == 0)
                {
                    if (addIfNotExist)
                        m_TitleStyles.Add(new TitleStyle { show = true });
                    else
                        return null;
                }

                return m_TitleStyles[0];
            }

            throw new Exception("SerieData not support component:" + type);
        }

        public void RemoveAllComponent()
        {
            m_ItemStyles.Clear();
            m_Labels.Clear();
            m_LabelLines.Clear();
            m_Symbols.Clear();
            m_EmphasisStyles.Clear();
            m_BlurStyles.Clear();
            m_SelectStyles.Clear();
            m_LineStyles.Clear();
            m_AreaStyles.Clear();
            m_TitleStyles.Clear();
        }

        public void RemoveComponent<T>() where T : ISerieDataComponent
        {
            RemoveComponent(typeof(T));
        }

        public void RemoveComponent(Type type)
        {
            if (type == typeof(ItemStyle))
                m_ItemStyles.Clear();
            else if (type == typeof(LabelStyle))
                m_Labels.Clear();
            else if (type == typeof(LabelLine))
                m_LabelLines.Clear();
            else if (type == typeof(EmphasisStyle))
                m_EmphasisStyles.Clear();
            else if (type == typeof(BlurStyle))
                m_BlurStyles.Clear();
            else if (type == typeof(SelectStyle))
                m_SelectStyles.Clear();
            else if (type == typeof(SerieSymbol))
                m_Symbols.Clear();
            else if (type == typeof(LineStyle))
                m_LineStyles.Clear();
            else if (type == typeof(AreaStyle))
                m_AreaStyles.Clear();
            else if (type == typeof(TitleStyle))
                m_TitleStyles.Clear();
            else
                throw new Exception("SerieData not support component:" + type);
        }

        public double GetData(int index, bool inverse = false)
        {
            if (index >= 0 && index < m_Data.Count) return inverse ? -m_Data[index] : m_Data[index];

            return 0;
        }

        public double GetData(int index, double min, double max)
        {
            if (index >= 0 && index < m_Data.Count)
            {
                var value = m_Data[index];
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }

            return 0;
        }

        public double GetPreviousData(int index, bool inverse = false)
        {
            if (index >= 0 && index < m_PreviousData.Count)
                return inverse ? -m_PreviousData[index] : m_PreviousData[index];

            return 0;
        }

        public double GetFirstData(bool unscaledTime, float animationDuration = 500f)
        {
            if (m_Data.Count > 0) return GetCurrData(0, 0, animationDuration, unscaledTime);
            return 0;
        }

        public double GetLastData()
        {
            if (m_Data.Count > 0) return m_Data[m_Data.Count - 1];
            return 0;
        }

        public double GetCurrData(int index, AnimationStyle animation, bool inverse = false, bool loop = false)
        {
            if (animation == null || !animation.enable)
                return GetData(index, inverse);
            return GetCurrData(index, animation.GetAdditionDuration(), animation.GetChangeDuration(),
                inverse, 0, 0, animation.unscaledTime, loop);
        }

        public double GetCurrData(int index, AnimationStyle animation, bool inverse, double min, double max,
            bool loop = false)
        {
            if (animation == null || !animation.enable)
                return GetData(index, inverse);
            return GetCurrData(index, animation.GetAdditionDuration(), animation.GetChangeDuration(),
                inverse, min, max, animation.unscaledTime, loop);
        }

        public double GetCurrData(int index, float dataAddDuration = 500f, float animationDuration = 500f,
            bool unscaledTime = false, bool inverse = false)
        {
            return GetCurrData(index, dataAddDuration, animationDuration, inverse, 0, 0, unscaledTime);
        }

        public double GetCurrData(int index, float dataAddDuration, float animationDuration, bool inverse, double min,
            double max, bool unscaledTime, bool loop = false)
        {
            if (dataAddDuration > 0)
                if (index < m_DataAddFlag.Count && m_DataAddFlag[index])
                {
                    var time = (unscaledTime ? Time.unscaledTime : Time.time) - m_DataAddTime[index];
                    var total = dataAddDuration / 1000;

                    var rate = time / total;
                    if (rate > 1) rate = 1;
                    if (rate < 1)
                    {
                        var prev = min > 0 ? min : 0;
                        var next = GetData(index);
                        var curr = MathUtil.Lerp(prev, next, rate);
                        curr = inverse ? -curr : curr;
                        return curr;
                    }

                    for (var i = 0; i < m_DataAddFlag.Count; i++)
                        m_DataAddFlag[i] = false;
                    return GetData(index, inverse);
                }

            if (animationDuration > 0)
            {
                if (index < m_DataUpdateFlag.Count && m_DataUpdateFlag[index])
                {
                    var time = (unscaledTime ? Time.unscaledTime : Time.time) - m_DataUpdateTime[index];
                    var total = animationDuration / 1000;

                    var rate = time / total;
                    if (rate > 1) rate = 1;
                    if (rate < 1)
                    {
                        CheckLastData(unscaledTime);
                        var prev = GetPreviousData(index);
                        var next = GetData(index);
                        if (loop && next <= min && prev != 0) next = max;
                        var curr = MathUtil.Lerp(prev, next, rate);
                        if (min != 0 || max != 0)
                        {
                            if (inverse)
                            {
                                var temp = min;
                                min = -max;
                                max = -temp;
                            }

                            var pre = m_PreviousData[index];
                            if (pre < min)
                            {
                                m_PreviousData[index] = min;
                                curr = min;
                            }
                            else if (pre > max)
                            {
                                m_PreviousData[index] = max;
                                curr = max;
                            }
                        }

                        curr = inverse ? -curr : curr;
                        return curr;
                    }

                    for (var i = 0; i < m_DataUpdateFlag.Count; i++)
                        m_DataUpdateFlag[i] = false;
                    return GetData(index, inverse);
                }

                return GetData(index, inverse);
            }

            return GetData(index, inverse);
        }

        public double GetAddAnimationData(double min, double max, float animationDuration = 500f,
            bool unscaledTime = false)
        {
            if (animationDuration > 0 && m_DataAddFlag.Count > 0 && m_DataAddFlag[0])
            {
                var time = (unscaledTime ? Time.unscaledTime : Time.time) - m_DataAddTime[0];
                var total = animationDuration / 1000;

                var rate = time / total;
                if (rate > 1) rate = 1;
                if (rate < 1)
                {
                    var curr = MathUtil.Lerp(min, max, rate);
                    return curr;
                }

                for (var i = 0; i < m_DataAddFlag.Count; i++)
                    m_DataAddFlag[i] = false;
                return max;
            }

            return max;
        }

        /// <summary>
        ///     the maxinum value.
        ///     ||最大值。
        /// </summary>
        public double GetMaxData(bool inverse = false, int startDimensionIndex = 0)
        {
            if (m_Data.Count == 0) return 0;
            var temp = double.MinValue;
            if (startDimensionIndex < 0) startDimensionIndex = 0;
            for (var i = startDimensionIndex; i < m_Data.Count; i++)
            {
                var value = GetData(i, inverse);
                if (value > temp) temp = value;
            }

            return temp;
        }

        /// <summary>
        ///     the mininum value.
        ///     ||最小值。
        /// </summary>
        public double GetMinData(bool inverse = false, int startDimensionIndex = 0)
        {
            if (m_Data.Count == 0) return 0;
            var temp = double.MaxValue;
            if (startDimensionIndex < 0) startDimensionIndex = 0;
            for (var i = startDimensionIndex; i < m_Data.Count; i++)
            {
                var value = GetData(i, inverse);
                if (value < temp) temp = value;
            }

            return temp;
        }

        public void GetMinMaxData(int startDimensionIndex, bool inverse, out double min, out double max)
        {
            if (m_Data.Count == 0)
            {
                min = 0;
                max = 0;
            }

            min = double.MaxValue;
            max = double.MinValue;
            for (var i = startDimensionIndex; i < m_Data.Count; i++)
            {
                var value = GetData(i, inverse);
                if (value < min) min = value;
                if (value > max) max = value;
            }
        }

        public double GetTotalData()
        {
            var total = 0d;
            foreach (var value in m_Data)
                total += value;
            return total;
        }

        public bool UpdateData(int dimension, double value, bool updateAnimation, bool unscaledTime,
            float animationDuration = 500f)
        {
            if (dimension >= 0 && dimension < data.Count)
            {
                CheckLastData(unscaledTime);
                m_PreviousData[dimension] = GetCurrData(dimension, 0, animationDuration, unscaledTime);
                m_DataUpdateTime[dimension] = unscaledTime ? Time.unscaledTime : Time.time;
                m_DataUpdateFlag[dimension] = updateAnimation;
                data[dimension] = value;
                return true;
            }

            return false;
        }

        public bool UpdateData(int dimension, double value)
        {
            if (dimension >= 0 && dimension < data.Count)
            {
                data[dimension] = value;
                return true;
            }

            return false;
        }

        private void CheckLastData(bool unscaledTime)
        {
            if (m_PreviousData.Count != m_Data.Count)
            {
                m_PreviousData.Clear();
                m_DataUpdateTime.Clear();
                m_DataUpdateFlag.Clear();
                for (var i = 0; i < m_Data.Count; i++)
                {
                    m_PreviousData.Add(m_Data[i]);
                    m_DataUpdateTime.Add(unscaledTime ? Time.unscaledTime : Time.time);
                    m_DataUpdateFlag.Add(false);
                }
            }
        }

        public bool IsDataChanged()
        {
            for (var i = 0; i < m_DataUpdateFlag.Count; i++)
                if (m_DataUpdateFlag[i])
                    return true;
            for (var i = 0; i < m_DataAddFlag.Count; i++)
                if (m_DataAddFlag[i])
                    return true;
            return false;
        }

        public float GetLabelWidth()
        {
            if (labelObject != null) return labelObject.GetTextWidth();
            return 0;
        }

        public float GetLabelHeight()
        {
            if (labelObject != null) return labelObject.GetTextHeight();
            return 0;
        }

        public void SetLabelActive(bool flag, bool force = false)
        {
            if (labelObject != null) labelObject.SetActive(flag, force);
            foreach (var labelObject in context.dataLabels) labelObject.SetActive(flag, force);
        }

        public void SetIconActive(bool flag)
        {
            if (labelObject != null) labelObject.SetIconActive(flag);
        }

        public void SetPolygon(params Vector2[] points)
        {
            m_PolygonPoints.Clear();
            m_PolygonPoints.AddRange(points);
        }

        public void SetPolygon(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            m_PolygonPoints.Clear();
            m_PolygonPoints.Add(p1);
            m_PolygonPoints.Add(p2);
            m_PolygonPoints.Add(p3);
            m_PolygonPoints.Add(p4);
        }

        public void SetPolygon(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5)
        {
            SetPolygon(p1, p2, p3, p4);
            m_PolygonPoints.Add(p5);
        }

        public bool IsInPolygon(Vector2 p)
        {
            return UGLHelper.IsPointInPolygon(p, m_PolygonPoints);
        }
    }
}