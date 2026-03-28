using System.Collections.Generic;
using UnityEngine;

public class RealTimeGraph : MonoBehaviour
{
    
    public LineRenderer lineRenderer;

    public float xSpacing = 0.1f;
    public float yScale = 0.01f;
    
    private List<float> values = new List<float>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float newValue = Mathf.PerlinNoise(Time.time, 0f) * 100f;
        AddPoint(newValue);
    }

    private void AddPoint(float newValue)
    {
        values.Add(newValue);
        lineRenderer.positionCount = values.Count;
        
        for(int i = 0;i < values.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(i * xSpacing, values[i] * yScale,0));
        }
        throw new System.NotImplementedException();
    }
}
