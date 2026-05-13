using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public struct PointInfo
    {
        public Vector3 position;
        public bool isIgnoreBreak;
        public double xValue;
        public double yValue;
        public double zValue;

        // public PointInfo(Vector3 pos, bool ignore)
        // {
        //     this.position = pos;
        //     this.isIgnoreBreak = ignore;
        // }

        public PointInfo(Vector3 pos, bool ignore, double x = 0, double y = 0, double z = 0)
        {
            position = pos;
            isIgnoreBreak = ignore;
            xValue = x;
            yValue = y;
            zValue = z;
        }
    }

    public class SerieContext
    {
        /// <summary>
        ///     中心点
        /// </summary>
        public Vector3 center;

        public double checkValue;
        public int clickTotalDataIndex;

        /// <summary>
        ///     theme的颜色索引
        /// </summary>
        public int colorIndex;

        /// <summary>
        ///     数据对应的位置坐标是否忽略（忽略时连线是透明的），dataIgnore 和 dataPoints 一一对应。
        /// </summary>
        public List<bool> dataIgnores = new();

        /// <summary>
        ///     数据对应的index索引。dataIndexs 和 dataPoints 一一对应。
        /// </summary>
        public List<int> dataIndexs = new();

        /// <summary>
        ///     最大值
        /// </summary>
        public double dataMax;

        /// <summary>
        ///     最小值
        /// </summary>
        public double dataMin;

        /// <summary>
        ///     数据对应的位置坐标。
        /// </summary>
        public List<Vector3> dataPoints = new();

        public int dataZoomStartIndex = 0;
        public int dataZoomStartIndexOffset = 0;

        /// <summary>
        ///     绘制点
        /// </summary>
        public List<PointInfo> drawPoints = new();

        /// <summary>
        ///     高
        /// </summary>
        public float height;

        /// <summary>
        ///     内半径
        /// </summary>
        public float insideRadius;

        /// <summary>
        ///     水平方向的
        /// </summary>
        public bool isHorizontal;

        public bool isTriggerByAxis = false;

        /// <summary>
        ///     线段终点
        /// </summary>
        public Vector3 lineEndPostion;

        public double lineEndValueX;
        public double lineEndValueY;
        public double lineEndValueZ;

        /// <summary>
        ///     外半径
        /// </summary>
        public float outsideRadius;

        public SerieParams param = new();

        /// <summary>
        ///     鼠标所在轴线上的数据项索引（可能有多个）
        /// </summary>
        public List<int> pointerAxisDataIndexs = new();

        /// <summary>
        ///     鼠标是否进入serie
        /// </summary>
        public bool pointerEnter;

        /// <summary>
        ///     鼠标当前指示的数据项维度
        /// </summary>
        public int pointerItemDataDimension = 1;

        /// <summary>
        ///     鼠标当前指示的数据项索引（单个）
        /// </summary>
        public int pointerItemDataIndex = -1;

        /// <summary>
        ///     矩形区域
        /// </summary>
        public Rect rect;

        public List<SerieData> rootData = new();

        /// <summary>
        ///     排序后的数据
        /// </summary>
        public List<SerieData> sortedData = new();

        public float startAngle;
        public Tooltip.Trigger tooltipTrigger;

        public Tooltip.Type tooltipType;
        public int totalDataIndex;

        /// <summary>
        ///     绘制顶点数
        /// </summary>
        public int vertCount;

        /// <summary>
        ///     宽
        /// </summary>
        public float width;

        /// <summary>
        ///     左下角坐标X
        /// </summary>
        public float x;

        /// <summary>
        ///     左下角坐标Y
        /// </summary>
        public float y;

        public ChartLabel titleObject { get; set; }
    }
}