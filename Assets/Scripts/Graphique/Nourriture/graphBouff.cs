using Unity.VisualScripting;
using UnityEngine;
using XCharts.Runtime;

public class graphBouff : MonoBehaviour
{
    public LineChart chart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        chart = gameObject.GetComponent<LineChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<LineChart>();
            chart.Init();
        }
        
        var title = chart.EnsureChartComponent<Title>();
        title.text = "Nourriture";
        title.labelStyle.textStyle.fontSize = 10;
     
        var tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        
      
        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.axisLabel.textStyle.fontSize = 10;
        xAxis.splitNumber = 2;
        xAxis.boundaryGap = true;
        xAxis.type = Axis.AxisType.Category;
        
        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisLabel.textStyle.fontSize = 10;
        
        chart.RemoveData();

        chart.AddSerie<Line>("nourriture");


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}