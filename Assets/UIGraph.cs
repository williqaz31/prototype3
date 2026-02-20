using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGraph : MonoBehaviour
{
    public RectTransform graphContainer;
    public Sprite dotSprite;

    public int maxPoints = 100;
    public float yMax = 200f;

    private List<GameObject> points = new List<GameObject>();
    private Queue<float> values = new Queue<float>();

    void Update()
    {
        // TEST : remplace par ta population
        float population = Mathf.PerlinNoise(Time.time, 0f) * 150f;
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

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float xSpacing = graphWidth / maxPoints;

        int index = values.Count - 1;

        float xPos = index * xSpacing;
        float yPos = (value / yMax) * graphHeight;

        GameObject dot = CreateDot(new Vector2(xPos, yPos));
        points.Add(dot);

        UpdatePositions(xSpacing);
    }

    GameObject CreateDot(Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject("dot", typeof(Image));
        obj.transform.SetParent(graphContainer, false);

        Image image = obj.GetComponent<Image>();
        image.sprite = dotSprite;
        image.color = Color.green;

        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return obj;
    }

    void UpdatePositions(float xSpacing)
    {
        for (int i = 0; i < points.Count; i++)
        {
            RectTransform rt = points[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * xSpacing, rt.anchoredPosition.y);
        }
    }
}
