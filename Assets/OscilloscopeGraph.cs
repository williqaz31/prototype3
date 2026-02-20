using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OscilloscopeGraph : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public float xSpacing = 0.1f;
    public float yScale = 1f;
    public int maxPoints = 200;

    private List<float> values = new List<float>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 10;

        transform.position = Vector3.zero;
    }

    void Update()
    {
        float newValue = Mathf.Sin(Time.time) * 2f;
        AddPoint(newValue);
    }

    void AddPoint(float value)
    {
        if (values.Count >= maxPoints)
            values.RemoveAt(0);

        values.Add(value);

        lineRenderer.positionCount = values.Count;

        for (int i = 0; i < values.Count; i++)
        {
            Vector3 pos = transform.position +
                          new Vector3(i * xSpacing, values[i] * yScale, 0);

            lineRenderer.SetPosition(i, pos);
        }
    }
}