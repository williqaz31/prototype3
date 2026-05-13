using UnityEngine;
using XCharts.Runtime;
#if INPUT_SYSTEM_ENABLED
using Input = XCharts.Runtime.InputHelper;
#endif

namespace XCharts.Example
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Example31_PieUpdateName : MonoBehaviour
    {
        private PieChart chart;

        private void Awake()
        {
            chart = gameObject.GetComponent<PieChart>();
            if (chart == null)
            {
                chart = gameObject.AddComponent<PieChart>();
                chart.Init();
            }

            var serieIndex = 0;
            var serie = chart.GetSerie(serieIndex);
            if (serie == null) return;
            serie.EnsureComponent<LabelStyle>();
            serie.label.show = true;
            serie.label.position = LabelStyle.Position.Outside;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) ClearAndAddData();
            //UpdateDataName();
            //UpdateDataName();
        }

        private void UpdateDataName()
        {
            var serieIndex = 0;
            var serie = chart.GetSerie(serieIndex);
            if (serie == null) return;
            for (var i = 0; i < serie.dataCount; i++)
            {
                var value = Random.Range(10, 100);
                chart.UpdateData(serieIndex, i, value);
                chart.UpdateDataName(serieIndex, i, "value=" + value);
            }
        }

        private void ResetSameName()
        {
            var serieIndex = 0;
            var serie = chart.GetSerie(serieIndex);
            if (serie == null) return;
            for (var i = 0; i < serie.dataCount; i++) chart.UpdateDataName(serieIndex, i, "piename");
        }

        private void ClearAndAddData()
        {
            var serieIndex = 0;
            var serie = chart.GetSerie(serieIndex);
            if (serie == null) return;
            var count = serie.dataCount;
            serie.ClearData();
            for (var i = 0; i < count; i++) chart.AddData(0, Random.Range(0, 100), "pie" + i);
        }
    }
}