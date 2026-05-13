using UnityEngine;
using XCharts.Runtime;

namespace XCharts.Example
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Example04_DataZoom : MonoBehaviour
    {
        private BaseChart chart;

        private void Awake()
        {
            chart = gameObject.GetComponent<BaseChart>();
            if (chart == null) return;
            var dataZoom = chart.GetChartComponent<DataZoom>();
            if (dataZoom == null) return;
            dataZoom.marqueeStyle.onStart = OnMarqueeStart;
            dataZoom.marqueeStyle.onEnd = OnMarqueeEnd;
            dataZoom.marqueeStyle.onGoing = OnMarquee;
        }

        private void OnMarqueeStart(DataZoom dataZoom)
        {
            //Debug.Log("OnMarqueeStart:" + dataZoom);
        }

        private void OnMarquee(DataZoom dataZoom)
        {
            //Debug.Log("OnMarquee:" + dataZoom);
        }

        private void OnMarqueeEnd(DataZoom dataZoom)
        {
            //Debug.Log("OnMarqueeEnd:" + dataZoom);
            var serie = chart.GetSerie(0);
            foreach (var serieData in serie.data)
                if (dataZoom.IsInMarqueeArea(serieData))
                    serieData.EnsureComponent<ItemStyle>().color = Color.red;
                else
                    serieData.EnsureComponent<ItemStyle>().color = Color.clear;
        }
    }
}