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

        int mapWidth = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > mapWidth)
                mapWidth = lines[i].Length;
        }
        int mapHeight = lines.Length;

        int tilesX = 20;
        int tilesY = 20;

        int startX = (tilesX - mapWidth) / 2;
        int startY = (tilesY - mapHeight) / 2;

        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                TileBase tile = DarkDirt; // dark dirt partout par défaut

                int mapX = x - startX;
                int mapY = y - startY;

                if (mapX >= 0 && mapX < mapWidth && mapY >= 0 && mapY < mapHeight)
                {
                    if (mapX < lines[mapY].Length)
                    {
                        char currentChar = lines[mapY][mapX];

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
                }

                Vector3Int centerOffset = new Vector3Int(tilesX / 2, -tilesY / 2, 0);
                Vector3Int verticalOffset = new Vector3Int(0, -1, 0);
                Vector3Int pos = new Vector3Int(x, -y, 0) - centerOffset + verticalOffset;
                tilemap.SetTile(pos, tile);
            }
        }

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