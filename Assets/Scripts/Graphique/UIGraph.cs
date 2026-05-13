using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGraph : MonoBehaviour
{
    public RectTransform graphContainer;
    public Sprite dotSprite;

    public int maxPoints = 100;
    public float yMax = 200f;

    private readonly List<GameObject> points = new();
    private readonly Queue<float> values = new();

    private void Update()
    {
        // TEST : remplace par ta population
        var population = Mathf.PerlinNoise(Time.time, 0f) * 150f;
        AddPoint(population);
    }

    public void AddPoint(float value)
    {
        if (values.Count >= maxPoints)
        {
            values.Dequeue();
            Destroy(points[0]);
            points.RemoveAt(0);
        }

        values.Enqueue(value);

        var graphWidth = graphContainer.sizeDelta.x;
        var graphHeight = graphContainer.sizeDelta.y;

        var xSpacing = graphWidth / maxPoints;

        var index = values.Count - 1;

        var xPos = index * xSpacing;
        var yPos = value / yMax * graphHeight;

        var dot = CreateDot(new Vector2(xPos, yPos));
        points.Add(dot);

        UpdatePositions(xSpacing);
    }

    private GameObject CreateDot(Vector2 anchoredPosition)
    {
        var obj = new GameObject("dot", typeof(Image));
        obj.transform.SetParent(graphContainer, false);

        var image = obj.GetComponent<Image>();
        image.sprite = dotSprite;
        image.color = Color.green;

        var rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return obj;
    }

    private void UpdatePositions(float xSpacing)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var rt = points[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * xSpacing, rt.anchoredPosition.y);
        }
    }
}