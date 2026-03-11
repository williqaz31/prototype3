using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    public Tilemap tilemap;

    public TileBase DarkDirt;
    public TileBase Dirt;

    public string filePath = "/Tiles/Maps.txt";

    public Camera mainCamera;
    void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        tilemap.ClearAllTiles();

        string[] lines = File.ReadAllLines(Application.dataPath + filePath);

        // Dynamic width and height
        int width = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > width)
                width = lines[i].Length;
        }
        int height = lines.Length;

        // Center offset
        Vector3Int centerOffset = new Vector3Int(width / 2, -height / 2, 0);

        // Place Tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileBase tile = null;

                if (x < lines[y].Length)
                {
                    char currentChar = lines[y][x];

                    switch (currentChar)
                    {
                        case '1':
                            tile = DarkDirt;
                            break;
                        case '0':
                            tile = Dirt;
                            break;
                    }
                }

                if (tile != null)
                {
                    Vector3Int verticalOffset = new Vector3Int(0, -1, 0); // move 1 tile down
                    Vector3Int pos = new Vector3Int(x, -y, 0) - centerOffset + verticalOffset;

                    tilemap.SetTile(pos, tile);
                }
            }
        }
        // Adjust camera
        tilemap.CompressBounds();
        Bounds mapBounds = tilemap.localBounds;

        if (mainCamera != null)
        {


            float padding = 1f;
            float sizeByHeight = mapBounds.size.y / 2f + padding;
            float sizeByWidth = (mapBounds.size.x / 2f + padding) / mainCamera.aspect;

            mainCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
            mainCamera.transform.position = mapBounds.center + new Vector3(0, 0, -10);
        }
    }

    float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1f)
        {
            LoadMap();
            timer = 0;
        }
    }
}