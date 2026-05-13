using UnityEngine;

namespace XCharts.Runtime
{
    public class PolarCoordContext : MainComponentContext
    {
        /// <summary>
        ///     the center position of polar in container.
        ///     ||极坐标在容器中的具体中心点。
        /// </summary>
        public Vector3 center;

        /// <summary>
        ///     the true radius of polar.
        ///     ||极坐标的运行时实际内半径。
        /// </summary>
        public float insideRadius;

        public bool isPointerEnter;

        /// <summary>
        ///     the true radius of polar.
        ///     ||极坐标的运行时实际外半径。
        /// </summary>
        public float outsideRadius;

        public float radius;
    }
}