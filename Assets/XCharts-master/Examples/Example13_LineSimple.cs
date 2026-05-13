using UnityEngine;
using XCharts.Runtime;
#if INPUT_SYSTEM_ENABLED
using Input = XCharts.Runtime.InputHelper;
#endif

namespace XCharts.Example
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Example13_LineSimple : MonoBehaviour
    {
        private void Awake()
        {
            AddData();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) AddData();
        }

        private void AddData()
        {
            var chart = gameObject.GetComponent<LineChart>();
            if (chart == null)
            {
                chart = gameObject.AddComponent<LineChart>();
                chart.Init();
            }

            chart.EnsureChartComponent<Title>().show = true;
            chart.EnsureChartComponent<Title>().text = "Line Simple";

            chart.EnsureChartComponent<Tooltip>().show = true;
            chart.EnsureChartComponent<Legend>().show = false;

            var xAxis = chart.EnsureChartComponent<XAxis>();
            var yAxis = chart.EnsureChartComponent<YAxis>();
            xAxis.show = true;
            yAxis.show = true;
            xAxis.type = Axis.AxisType.Category;
            yAxis.type = Axis.AxisType.Value;

            xAxis.splitNumber = 10;
            xAxis.boundaryGap = true;

            chart.RemoveData();
            chart.AddSerie<Line>();
            chart.AddSerie<Line>();
            for (var i = 0; i < 20; i++)
            {
                chart.AddXAxisData("x" + i);
                chart.AddData(0, Random.Range(10, 20));
                chart.AddData(1, Random.Range(10, 20));
            }
        }
    }
}