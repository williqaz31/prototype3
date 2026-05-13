using UnityEngine;
using XCharts.Runtime;

namespace XCharts.Example
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Example50_Scatter : MonoBehaviour
    {
        private ScatterChart chart;

        private void Awake()
        {
            chart = gameObject.GetComponent<ScatterChart>();
            if (chart == null) return;
            foreach (var serie in chart.series) serie.symbol.sizeFunction = SymbolSize;
        }

        private float SymbolSize(float defaultSize, SerieData serieData)
        {
            return defaultSize;
        }
    }
}